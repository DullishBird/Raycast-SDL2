using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;


namespace RaycastEngine
{
    public class RaycastRendererTest
    {
        public static void AlphaRaycast()
        {

            RaycastEngine.WorldMap worldMap = new RaycastEngine.WorldMap("D:/dev/sharp/Doom-SDL-remake/res/map.txt");            

            int mapWidth = 24;
            int mapHeight = 24;
            int screenWidth = 640;
            int screenHeight = 480;

            double posX = 22, posY = 12;  //x and y start position
            double dirX = -1, dirY = 0; //initial direction vector
            double planeX = 0, planeY = 0.66; //the 2d raycaster version of camera plane

            double time = 0; //time of current frame
            double oldTime = 0; //time of previous frame
            
            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
            }

            if (SDL_ttf.TTF_Init() < 0)
            {
                Console.WriteLine($"There was an issue initilizing SDL_ttf. {SDL_ttf.TTF_GetError()}");
            }

            RaycastEngine.Window window = new RaycastEngine.Window("SDL .NET 6 Tutorial", 640, 480);

            window.Titile = "Test";

            var renderer = window.GetRenderer();

            var running = true;

            var windowWight = window.GetWight();
            var windowHeight = window.GetHeight();

            //timing for input and FPS counter
            oldTime = time;
            time = SDL.SDL_GetTicks();
            double frameTime = (time - oldTime) / 1000.0; //frameTime is the time this frame has taken, in seconds
            
            double moveSpeed =  frameTime * 5.0; //the constant value is in squares/second
            double rotSpeed =  frameTime * 3.0; //the constant value is in radians/second

            var font = SDL_ttf.TTF_OpenFont("C:/Windows/Fonts/Arial.ttf", 20);

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
                     
                        //move forward if no wall in front of you
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_w)
                            {
                                if(worldMap.GetWallType((int)(posX + dirX * moveSpeed), (int)posY) == 0) posX += dirX * moveSpeed;
                                if(worldMap.GetWallType((int)posX, (int)(posY + dirY * moveSpeed)) == 0) posY += dirY * moveSpeed;
                            }
                            //move backwards if no wall behind you
                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s)
                            {
                                if (worldMap.GetWallType((int)(posX - dirX * moveSpeed), (int)posY) == 0) posX -= dirX * moveSpeed;
                                if (worldMap.GetWallType((int)(posX), (int)(posY - dirY * moveSpeed)) == 0) posY -= dirY * moveSpeed;
                            }
                            //both camera direction and camera plane must be rotated
                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_d)
                            {
                                double oldDirX = dirX;
                                dirX = dirX * Math.Cos(-rotSpeed) - dirY * Math.Sin(-rotSpeed);
                                dirY = oldDirX * Math.Sin(-rotSpeed) + dirY * Math.Cos(-rotSpeed);
                                double oldPlaneX = planeX;
                                planeX = planeX * Math.Cos(-rotSpeed) - planeY * Math.Sin(-rotSpeed);
                                planeY = oldPlaneX * Math.Sin(-rotSpeed) + planeY * Math.Cos(-rotSpeed);
                            }

                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_a)
                            {
                                double oldDirX = dirX;
                                dirX = dirX * Math.Cos(rotSpeed) - dirY * Math.Sin(rotSpeed);
                                dirY = oldDirX * Math.Sin(rotSpeed) + dirY * Math.Cos(rotSpeed);
                                double oldPlaneX = planeX;
                                planeX = planeX * Math.Cos(rotSpeed) - planeY * Math.Sin(rotSpeed);
                                planeY = oldPlaneX * Math.Sin(rotSpeed) + planeY * Math.Cos(rotSpeed);
                            }
                            break;
                    }

                }
                SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                if (SDL.SDL_RenderClear(renderer) < 0)
                {
                    Console.WriteLine($"There was an issue with clearing the render surface. {SDL.SDL_GetError()}");
                }


                for (int x = 0; x < windowWight; x++)
                {
                    //calculate ray position and direction
                    double cameraX = 2 * x / (double)windowWight - 1; //x-coordinate in camera space
                    double rayDirX = dirX + planeX * cameraX;
                    double rayDirY = dirY + planeY * cameraX;

                    //which box of the map we're in
                    int mapX = (int)posX;
                    int mapY = (int)posY;

                    //length of ray from current position to next x or y-side
                    double sideDistX;
                    double sideDistY;

                    //length of ray from one x or y-side to next x or y-side
                    double deltaDistX = (rayDirX == 0) ? 1e30 : Math.Abs(1 / rayDirX);
                    double deltaDistY = (rayDirY == 0) ? 1e30 : Math.Abs(1 / rayDirY);
                    double perpWallDist;

                    //what direction to step in x or y-direction (either +1 or -1)
                    int stepX;
                    int stepY;

                    int hit = 0; //was there a wall hit?
                    int side = 0; //was a NS or a EW wall hit?

                    //calculate step and initial sideDist
                    if (rayDirX < 0)
                    {
                        stepX = -1;
                        sideDistX = (posX - mapX) * deltaDistX;
                    }
                    else
                    {
                        stepX = 1;
                        sideDistX = (mapX + 1.0 - posX) * deltaDistX;
                    }
                    if (rayDirY < 0)
                    {
                        stepY = -1;
                        sideDistY = (posY - mapY) * deltaDistY;
                    }
                    else
                    {
                        stepY = 1;
                        sideDistY = (mapY + 1.0 - posY) * deltaDistY;
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

                oldTime = time;
                time = SDL.SDL_GetTicks();
                double frameTimee = (time - oldTime) / 1000.0; //frameTime is the time this frame has taken, in seconds
                SDL.SDL_Color textColor = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
                
                string fpsCounterText = (Math.Round(1.0 / frameTimee)).ToString();

                IntPtr message = SDL.SDL_CreateTextureFromSurface(renderer, SDL_ttf.TTF_RenderText_Solid(font, fpsCounterText, textColor));
                //IntPtr message = SDL.SDL_CreateTextureFromSurface(renderer, IntPtr.Zero);
                //Console.WriteLine(1.0 / frameTimee); //FPS counter

                var rect = new SDL.SDL_Rect
                {
                    x = 0,
                    y = 0,
                    w = 20,
                    h = 20
                };

                SDL.SDL_RenderCopy(renderer, message, ref rect, ref rect);
                
                // Switches out the currently presented render surface with the one we just did work on.
                SDL.SDL_RenderPresent(renderer);                
                SDL.SDL_DestroyTexture(message);
            }
            // Clean up the resources that were created.
            SDL_ttf.TTF_CloseFont(font);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(renderer);
            SDL.SDL_Quit();
        }

    }
}
