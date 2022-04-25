using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SDL2;


namespace RaycastEngine
{
    public class RaycastRenderer
    {
        private WorldMap worldMap = new WorldMap("D:/dev/sharp/Raycast-SDL2/res/map.txt"); // экзем

        //getting path here
        //public RaycastRendererTest()
        //{
        //    string path = Directory.GetCurrentDirectory() + "/res/map.txt";
        //    worldMap = new WorldMap(path);
        //}

        public void Start()
        {
            float time = 0; //time of current frame
            float oldTime = 0; //time of previous frame
            Vector2 camDir = new Vector2(-1, 0);
            Vector2 camPos = new Vector2(22, 12);
            Vector2 camPlane = new Vector2(0, 0.66f);
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
            }

            if (SDL_ttf.TTF_Init() < 0)
            {
                Console.WriteLine($"There was an issue initilizing SDL_ttf. {SDL_ttf.TTF_GetError()}");
            }

            Window window = new Window("SDL .NET 6 Tutorial", 640, 480);
            RenderText renderText = new RenderText();

            window.Titile = "Test";

            var renderer = window.GetRenderer();

            var running = true;

            var windowWight = window.GetWight();
            var windowHeight = window.GetHeight();

            //timing for input and FPS counter
            oldTime = time;
            time = SDL.SDL_GetTicks();
            float frameTime = (time - oldTime) / 1000.0f; //frameTime is the time this frame has taken, in seconds
            
            float moveSpeed =  frameTime * 5.0f; //the constant value is in squares/second
            float rotSpeed =  frameTime * 3.0f;//the constant value is in radians/second
            float mrotSpeed = frameTime * 0.025f;//the constant value is in radians/second
            SDL.SDL_GetMouseState(out int mCamPosX, out int _);
            float oldMousePos = mCamPosX;
            while (running)
            {
                // Check to see if there are any events and continue to do so until the queue is empty.
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            running = false;
                            break;

                        case SDL.SDL_EventType.SDL_MOUSEMOTION:
                            {
                                int mouseCamPosX = e.motion.x;
                                float currentMouseSpeed = mrotSpeed * (oldMousePos - mouseCamPosX);
                                Rotating(currentMouseSpeed, ref camDir, ref camPlane);                              
                                oldMousePos = windowWight / 2;
                                break;
                            }

                        //move forward if no wall in front of you
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            if(e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
                            {
                                SDL.SDL_Quit();
                                running = false;
                                break;
                            }

                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_w)
                            {
                                if(worldMap.GetWallType((int)(camPos.X + camDir.X * moveSpeed), (int)camPos.Y) == 0) camPos.X += camDir.X * moveSpeed;
                                if(worldMap.GetWallType((int)camPos.X, (int)(camPos.Y + camDir.Y * moveSpeed)) == 0) camPos.Y += camDir.Y * moveSpeed;
                            }
                            //move backwards if no wall behind you
                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s)
                            {
                                if (worldMap.GetWallType((int)(camPos.X - camDir.X * moveSpeed), (int)camPos.Y) == 0) camPos.X -= camDir.X * moveSpeed;
                                if (worldMap.GetWallType((int)(camPos.X), (int)(camPos.Y - camDir.Y * moveSpeed)) == 0) camPos.Y -= camDir.Y * moveSpeed;
                            }
                            //both camera direction and camera plane must be rotated
                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_d)
                            {
                                Rotating(-rotSpeed, ref camDir, ref camPlane);
                            }

                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_a)
                            {
                                Rotating(rotSpeed, ref camDir, ref camPlane);
                            }
                            break;
                    }

                }
                SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                if (SDL.SDL_RenderClear(renderer) < 0)
                {
                    Console.WriteLine($"There was an issue with clearing the render surface. {SDL.SDL_GetError()}");
                }

                DrawMap(windowWight,
                        windowHeight,
                        camDir,
                        camPlane,
                        camPos,
                        renderer);
                
                //creating fps counter
                oldTime = time;
                time = SDL.SDL_GetTicks();
                float frameCounter = (time - oldTime) / 1000.0f; //frameTime is the time this frame has taken, in seconds

                //SDL.SDL_RenderCopy(renderer, message, ref renderText.GetRect(), ref renderText.GetRect());
                string messageText = MathF.Round(1.0f / frameCounter).ToString();
                renderText.Draw(renderer, messageText, 0, 0);

                SDL.SDL_WarpMouseInWindow(window.GetNative(), windowWight / 2, windowHeight / 2);
                // Switches out the currently presented render surface with the one we just did work on.
                SDL.SDL_RenderPresent(renderer);
            }
            // Clean up the resources that were created.
            SDL.SDL_Quit();
        }

        private void DrawMap(int windowWight, int windowHeight, Vector2 camDir, Vector2 camPlane,
                               Vector2 camPos, IntPtr renderer)
        {
            for (int x = 0; x < windowWight; x++)
            {
                //calculate ray position and direction
                float cameraX = 2 * x / (float)windowWight - 1; //x-coordinate in camera space
                float rayCamDirX = camDir.X + camPlane.X * cameraX;
                float rayCamDirY = camDir.Y + camPlane.Y * cameraX;

                //which box of the map we're in
                int mapX = (int)camPos.X;
                int mapY = (int)camPos.Y;

                //length of ray from current position to next x or y-side
                float sideDistX;
                float sideDistY;

                //length of ray from one x or y-side to next x or y-side
                float deltaDistX = (rayCamDirX == 0) ? 1e30f : MathF.Abs(1 / rayCamDirX);
                float deltaDistY = (rayCamDirY == 0) ? 1e30f : MathF.Abs(1 / rayCamDirY);
                float perpWallDist;

                //what direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; //was there a wall hit?
                int side = 0; //was a NS or a EW wall hit?

                //calculate step and initial sideDist
                if (rayCamDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (camPos.X - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0f - camPos.X) * deltaDistX;
                }
                if (rayCamDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (camPos.Y - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0f - camPos.Y) * deltaDistY;
                }

                //perform DDA
                while (hit == 0)
                {
                    //jump to next map square, either in x-direction, or in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }
                    //Check if ray has hit a wall
                    if (worldMap.GetWallType(mapX, mapY) > 0)
                        hit = 1;
                }

                if (side == 0) perpWallDist = (sideDistX - deltaDistX);
                else perpWallDist = (sideDistY - deltaDistY);

                //Calculate height of line to draw on screen
                int lineHeight = (int)(windowHeight / perpWallDist);

                //calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + windowHeight / 2;
                if (drawStart < 0) drawStart = 0;

                int drawEnd = lineHeight / 2 + windowHeight / 2;
                if (drawEnd >= windowHeight) drawEnd = windowHeight - 1;

                //choose wall color
                SDL.SDL_Color color;
                switch (worldMap[mapX, mapY])
                {
                    case 1: color = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255 }; break; //red
                    case 2: color = new SDL.SDL_Color { r = 0, g = 255, b = 0, a = 255 }; break; //green
                    case 3: color = new SDL.SDL_Color { r = 0, g = 0, b = 255, a = 255 }; break; //blue
                    case 4: color = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 }; break; //white
                    default: color = new SDL.SDL_Color { r = 255, g = 255, b = 0, a = 255 }; break; //yellow
                }

                if (side == 1)
                {
                    color.r /= (byte)2;
                    color.g /= (byte)2;
                    color.b /= (byte)2;
                }                
                //draw the pixels of the stripe as a vertical line
                SDL.SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, color.a);
                SDL.SDL_RenderDrawLine(renderer, x, drawStart, x, drawEnd);
            }
        }

        private void Rotating(float rotSpeed, ref Vector2 camDir, ref Vector2 camPlane)
        {
            float oldCamDirX = camDir.X;
            camDir.X = camDir.X * MathF.Cos(rotSpeed) - camDir.Y * MathF.Sin(rotSpeed);
            camDir.Y = oldCamDirX * MathF.Sin(rotSpeed) + camDir.Y * MathF.Cos(rotSpeed);
            float oldCamPlaneX = camPlane.X;
            camPlane.X = camPlane.X * MathF.Cos(rotSpeed) - camPlane.Y * MathF.Sin(rotSpeed);
            camPlane.Y = oldCamPlaneX * MathF.Sin(rotSpeed) + camPlane.Y * MathF.Cos(rotSpeed);
        }
    }
}
