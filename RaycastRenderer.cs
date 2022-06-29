using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SDL2;


namespace RaycastEngine
{
    public class RaycastRenderer
    {
        private string path = "";
        private WorldMap worldMap;

        Window window = new Window("SDL .NET 6 Tutorial", 640, 480);
        Camera camera = new Camera(new Vector3(22, 12, 0), new Vector2(-1, 0), new Vector2(0, 0.66f), 0f);

        static int numSprites = 19;

        string[] texturePath = {"/res/pics/eagle.png", "/res/pics/redbrick.png",
                                "/res/pics/purplestone.png", "/res/pics/greystone.png", "/res/pics/bluestone.png",
                                "/res/pics/mossy.png", "/res/pics/wood.png", "/res/pics/colorstone.png",
                                "/res/pics/barrel.png", "/res/pics/pillar.png", "/res/pics/greenlight.png"};
        List<Image> textures = new List<Image>();

        public RaycastRenderer()
        {
            path = Environment.CurrentDirectory;
            string directoryPath = "";
            if (path.Contains(@"\bin\Debug"))
            {
                path = path.Remove((path.Length - (@"\bin\Debug\net6.0").Length));
                directoryPath = path + "/res/map2.txt";
            }
            worldMap = new WorldMap(directoryPath);

            for (int i = 0; i < texturePath.Length; i++)
            {
                textures.Add(new Image(path + texturePath[i], i));
            }
        }

        public void Start()
        {
            float time = 0; //time of current frame
            float oldTime = 0; //time of previous frame

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
            }

            if (SDL_ttf.TTF_Init() < 0)
            {
                Console.WriteLine($"There was an issue initilizing SDL_ttf. {SDL_ttf.TTF_GetError()}");
            }

            Sprite[] sprite = new Sprite[]
            {
                //green light in front of playerstart
                new Sprite (new Vector2(20.5f, 11.5f), 10), 
              
                  //green lights in every room
                new Sprite (new Vector2(18.5f, 4.5f), 10),
                new Sprite (new Vector2(10.0f, 4.5f), 10),
                new Sprite (new Vector2(10.0f, 12.5f),10),
                new Sprite (new Vector2(3.5f, 6.5f), 10),
                new Sprite (new Vector2(3.5f, 20.5f), 10),
                new Sprite (new Vector2(3.5f, 14.5f), 10),
                new Sprite (new Vector2(14.5f,20.5f), 10),

                //row of pillars in front of wall: fisheye test
                new Sprite (new Vector2(18.5f, 10.5f), 9),
                new Sprite (new Vector2(18.5f, 11.5f), 9),
                new Sprite (new Vector2(18.5f, 12.5f), 9),

                //some barrels around the map
                new Sprite (new Vector2(21.5f, 1.5f), 8, 2, 2, 128.0f),
                new Sprite (new Vector2(15.5f, 1.5f), 8, 2, 2, 128.0f),
                new Sprite (new Vector2(16.0f, 1.8f), 8, 2, 2, 128.0f),
                new Sprite (new Vector2(16.2f, 1.2f), 8, 2, 2, 128.0f),
                new Sprite (new Vector2(3.5f, 2.5f), 8, 2, 2, 128.0f),
                new Sprite (new Vector2(9.5f, 15.5f), 8, 2, 2, 128.0f),
                new Sprite (new Vector2(10.0f, 15.1f), 8, 2, 2, 128.0f),
                new Sprite (new Vector2(10.5f, 15.8f), 8, 2, 2, 128.0f)
            };

            RenderText renderText = new RenderText();

            window.Titile = "Test";

            var renderer = window.GetRenderer();

            var running = true;

            var windowWidth = window.GetWight();
            var windowHeight = window.GetHeight();

            UInt32[] buffer = new UInt32[windowHeight * windowWidth];

            //1D Zbuffer
            double[] ZBuffer = new double[windowWidth];

            //arrays used to sort the sprites
            int[] spriteOrder = new int[numSprites];
            double[] spriteDistance = new double[numSprites];

            //timing for input and FPS counter
            oldTime = time;
            time = SDL.SDL_GetTicks();
            float frameTime = (time - oldTime) / 1000.0f; //frameTime is the time this frame has taken, in seconds

            float moveSpeed = frameTime * 0.3f; //the constant value is in squares/second
            float vertMoveSpeed = frameTime * 1.0f; //vertical cam movement
            float rotSpeed = frameTime * 0.1f;//the constant value is in radians/second
            float mrotSpeed = frameTime * 0.005f;//the constant value is in radians/second

            SDL.SDL_GetMouseState(out int mCamPosX, out int mCamPosY);
            float oldMousePosX = mCamPosX;
            float oldMousePosY = mCamPosY;

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
                                int mouseCamPosY = e.motion.y;

                                camera.Pitch += (oldMousePosY - mouseCamPosY) * vertMoveSpeed;

                                if (camera.Pitch > 200) camera.Pitch = 200;
                                if (camera.Pitch < -200) camera.Pitch = -200;

                                float currentMouseSpeed = mrotSpeed * (oldMousePosX - mouseCamPosX);
                                camera.Rotate(currentMouseSpeed);
                                oldMousePosX = windowWidth / 2;
                                oldMousePosY = windowHeight / 2;

                                break;
                            }
                    }
                    if (SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN == e.type)
                    {
                        if (SDL.SDL_BUTTON_LEFT == e.button.button)
                        {
                            //AABB(renderer);
                            //Console.WriteLine("Left mouse button is down");
                        }
                    }
                }
                
                IntPtr keysPtr = SDL.SDL_GetKeyboardState(out int numkeys);
                byte[] keyState = new byte[numkeys];
                Marshal.Copy(keysPtr, keyState, 0, numkeys);

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE] == 1)
                {
                    running = false;
                    break;
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_W] == 1)
                {
                    if (worldMap.GetWallType((int)(camera.Pos.X + camera.Dir.X * moveSpeed), (int)camera.Pos.Y) == 0) camera.Pos = new Vector3(camera.Pos.X + camera.Dir.X * moveSpeed, camera.Pos.Y, camera.Pos.Z);
                    if (worldMap.GetWallType((int)camera.Pos.X, (int)(camera.Pos.Y + camera.Dir.Y * moveSpeed)) == 0) camera.Pos = new Vector3(camera.Pos.X, camera.Pos.Y + camera.Dir.Y * moveSpeed, camera.Pos.Z);
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_S] == 1)
                {
                    if (worldMap.GetWallType((int)(camera.Pos.X - camera.Dir.X * moveSpeed), (int)camera.Pos.Y) == 0) camera.Pos = new Vector3(camera.Pos.X - camera.Dir.X * moveSpeed, camera.Pos.Y, camera.Pos.Z);
                    if (worldMap.GetWallType((int)camera.Pos.X, (int)(camera.Pos.Y - camera.Dir.Y * moveSpeed)) == 0) camera.Pos = new Vector3(camera.Pos.X, camera.Pos.Y - camera.Dir.Y * moveSpeed, camera.Pos.Z);
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_A] == 1)
                {
                    if (worldMap.GetWallType((int)(camera.Pos.X - camera.Plane.X * moveSpeed), (int)camera.Pos.Y) == 0) camera.Pos = new Vector3(camera.Pos.X - camera.Plane.X * moveSpeed, camera.Pos.Y, camera.Pos.Z);
                    if (worldMap.GetWallType((int)camera.Pos.X, (int)(camera.Pos.Y - camera.Plane.Y * moveSpeed)) == 0) camera.Pos = new Vector3(camera.Pos.X, camera.Pos.Y - camera.Plane.Y * moveSpeed, camera.Pos.Z);
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_D] == 1)
                {
                    if (worldMap.GetWallType((int)(camera.Pos.X + camera.Plane.X * moveSpeed), (int)camera.Pos.Y) == 0) camera.Pos = new Vector3(camera.Pos.X + camera.Plane.X * moveSpeed, camera.Pos.Y, camera.Pos.Z);
                    if (worldMap.GetWallType((int)camera.Pos.X, (int)(camera.Pos.Y + camera.Plane.Y * moveSpeed)) == 0) camera.Pos = new Vector3(camera.Pos.X, camera.Pos.Y + camera.Plane.Y * moveSpeed, camera.Pos.Z);
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_Q] == 1)
                {
                    camera.Rotate(rotSpeed);
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_E] == 1)
                {
                    camera.Rotate(-rotSpeed);
                }

                SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
                if (SDL.SDL_RenderClear(renderer) < 0)
                {
                    Console.WriteLine($"There was an issue with clearing the render surface. {SDL.SDL_GetError()}");
                }
                
                DrawMap(windowWidth,
                        windowHeight,
                        camera,
                        buffer,
                        ZBuffer,
                        spriteDistance,
                        sprite);
                
                IntPtr frameTexture = IntPtr.Zero;

                //Now render to the texture
                unsafe
                {
                    fixed (UInt32* bufferRawPtr = buffer)
                    {
                        IntPtr bufferIntPtr = (IntPtr)bufferRawPtr;

                        UInt32 rmask, gmask, bmask, amask;
                        rmask = 0xff000000;
                        gmask = 0x00ff0000;
                        bmask = 0x0000ff00;
                        amask = 0x000000ff;

                        int depth = 4 * 8,
                            pitch_ = windowWidth * 4;

                        IntPtr surface = SDL.SDL_CreateRGBSurfaceFrom(bufferIntPtr, windowWidth, windowHeight, depth, pitch_, rmask, gmask, bmask, amask);

                        frameTexture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
                        SDL.SDL_FreeSurface(surface);
                    }
                }

                //Now render the texture target to our screen, but upside down

                if (SDL.SDL_RenderCopyEx(renderer, frameTexture, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE) < 0)
                {
                    Console.WriteLine($"There was an issue with SDL_RenderCopyEx. {SDL.SDL_GetError()}");
                };
                CheckSpritesIntersection(renderer, sprite);
                SDL.SDL_DestroyTexture(frameTexture);
                for (int i = 0; i < buffer.Length; i++) buffer[i] = 0; //clear the buffer instead of cls()

                //creating fps counter
                oldTime = time;
                time = SDL.SDL_GetTicks();
                float frameCounter = (time - oldTime) / 1000.0f; //frameTime is the time this frame has taken, in seconds

                //SDL.SDL_RenderCopy(renderer, message, ref renderText.GetRect(), ref renderText.GetRect());
                string messageText = MathF.Round(1.0f / frameCounter).ToString();
                renderText.Draw(renderer, messageText, 0, 0);

                //setting mouse position in the centre of the screen
                SDL.SDL_WarpMouseInWindow(window.GetNative(), windowWidth / 2, windowHeight / 2);

                // Switches out the currently presented render surface with the one we just did work on.
                SDL.SDL_RenderPresent(renderer);
            }
            // Clean up the resources that were created.
            SDL.SDL_Quit();
        }

        private void DrawMap(int windowWidth, int windowHeight, Camera camera, UInt32[] buffer, double[] ZBuffer,
                             double[] spriteDistance, Sprite[] sprite)
        {
            
            FloorCasting(windowHeight, windowWidth, camera, buffer);
            WallCasting(windowHeight, windowWidth, camera, buffer, ZBuffer);
            SpriteCasting(sprite, spriteDistance, windowHeight, windowWidth, camera, buffer, ZBuffer);
        }

        private void FloorCasting(int windowHeight, int windowWidth, Camera camera, UInt32[] buffer)
        {
            var camDir = camera.Dir;
            var camPos = camera.Pos;
            var camPlane = camera.Plane;
            var pitch = camera.Pitch;
            //FLOOR CASTING
            for (int y = 0; y < windowHeight; y++)
            {
                // whether this section is floor or ceiling
                bool is_floor = y > windowHeight / 2 + pitch;

                // rayDir for leftmost ray (x = 0) and rightmost ray (x = w)
                float rayDirX0 = camDir.X - camPlane.X;
                float rayDirY0 = camDir.Y - camPlane.Y;
                float rayDirX1 = camDir.X + camPlane.X;
                float rayDirY1 = camDir.Y + camPlane.Y;

                // Current y position compared to the center of the screen (the horizon)
                int p = (int)(is_floor ? (y - windowHeight / 2 - pitch) : (windowHeight / 2 - y + pitch));

                // Vertical position of the camera.
                float camZ = (float)(is_floor ? (0.5 * windowHeight + camPos.Z) : (0.5 * windowHeight - camPos.Z));
                //float posZ = 0.5f * windowHeight;

                // Horizontal distance from the camera to the floor for the current row.
                // 0.5 is the z position exactly in the middle between floor and ceiling.
                float rowDistance = camZ / p;

                // calculate the real world step vector we have to add for each x (parallel to camera plane)
                // adding step by step avoids multiplications with a weight in the inner loop
                float floorStepX = rowDistance * (rayDirX1 - rayDirX0) / windowWidth;
                float floorStepY = rowDistance * (rayDirY1 - rayDirY0) / windowWidth;

                // real world coordinates of the leftmost column. This will be updated as we step to the right.
                float floorX = camPos.X + rowDistance * rayDirX0;
                float floorY = camPos.Y + rowDistance * rayDirY0;

                for (int x = 0; x < windowWidth; ++x)
                {
                    // the cell coord is simply got from the integer parts of floorX and floorY
                    int cellX = (int)(floorX);
                    int cellY = (int)(floorY);

                    // choose texture and draw the pixel
                    int floorTexture = 3;
                    int ceilingTexture = 6;
                    var texureIndex = is_floor ? floorTexture : ceilingTexture;
                    // get the texture coordinate from the fractional part
                    int tx = (int)(textures[texureIndex].Widht * (floorX - cellX)) & (textures[texureIndex].Widht - 1);
                    int ty = (int)(textures[texureIndex].Hight * (floorY - cellY)) & (textures[texureIndex].Hight - 1);

                    floorX += floorStepX;
                    floorY += floorStepY;

                    UInt32 color = textures[texureIndex][tx, ty];
                    //color = (color >> 1) & 8355711; // make a bit darker
                    buffer[y * windowWidth + x] = color;
                }
            }
        }

        private void WallCasting(int windowHeight, int windowWidth, Camera camera, UInt32[] buffer, double[] ZBuffer)
        {
            var camDir = camera.Dir;
            var camPos = camera.Pos;
            var camPlane = camera.Plane;
            var pitch = camera.Pitch;

            //WALL CASTING
            for (int x = 0; x < windowWidth; x++)
            {
                //calculate ray position and direction
                float cameraX = 2 * x / (float)windowWidth - 1; //x-coordinate in camera space
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
                int drawStart = (int)(-lineHeight / 2 + windowHeight / 2 + pitch + (camPos.Z / perpWallDist));
                if (drawStart < 0) drawStart = 0;

                int drawEnd = (int)(lineHeight / 2 + windowHeight / 2 + pitch + (camPos.Z / perpWallDist));
                if (drawEnd >= windowHeight) drawEnd = windowHeight - 1;
                //texturing calculations
                int texNum = worldMap.GetWallType(mapX, mapY) - 1; //1 subtracted from it so that texture 0 can be used!

                //calculate value of wallX
                double wallX; //where exactly the wall was hit
                if (side == 0) wallX = camPos.Y + perpWallDist * rayCamDirY;
                else wallX = camPos.X + perpWallDist * rayCamDirX;
                wallX -= Math.Floor((wallX));

                //x coordinate on the texture
                int texX = (int)(wallX * (double)(textures[texNum].Widht));
                if (side == 0 && rayCamDirX > 0)
                    texX = textures[texNum].Widht - texX - 1;
                if (side == 1 && rayCamDirY < 0)
                    texX = textures[texNum].Widht - texX - 1;

                // How much to increase the texture coordinate per screen pixel
                double step = 1.0 * textures[texNum].Hight / lineHeight;
                // Starting texture coordinate
                double texPos = (drawStart - pitch - (camPos.Z / perpWallDist) - windowHeight / 2 + lineHeight / 2) * step;
                for (int y = drawStart; y < drawEnd; y++)
                {
                    // Cast the texture coordinate to integer, and mask with (texHeight - 1) in case of overflow
                    int texY = (int)texPos & (textures[texNum].Hight - 1);
                    texPos += step;
                    UInt32 color = textures[texNum][texX, texY];
                    ////make color darker for y-sides: R, G and B byte each divided through two with a "shift" and an "and"
                    //if (side == 1) color = (color >> 1) & 8355711;

                    buffer[y * windowWidth + x] = color;
                }
                ZBuffer[x] = perpWallDist;
            }
        }

        private void SpriteCasting(Sprite[] sprite ,double[] spriteDistance, int windowHeight, int windowWidth, Camera camera, UInt32[] buffer, double[] ZBuffer)
        {
            var camDir = camera.Dir;
            var camPos = camera.Pos;
            var camPlane = camera.Plane;
            var pitch = camera.Pitch;
            //SPRITE CASTING
            //sort sprites from far to close
            for (int i = 0; i < numSprites; i++)
            {
                spriteDistance[i] = ((camPos.X - sprite[i].Position.X) * (camPos.X - sprite[i].Position.X) + (camPos.Y - sprite[i].Position.Y) * (camPos.Y - sprite[i].Position.Y)); //sqrt not taken, unneeded
            }
            SortSprites(sprite, spriteDistance);

            //after sorting the sprites, do the projection and draw them
            for (int i = 0; i < numSprites; i++)
            {
                sprite[i].Draw(camPos, camDir, camPlane, windowWidth, windowHeight, pitch, buffer, ZBuffer, textures);
            }
        }

        private void SortSprites(Sprite[] sprites, double[] spriteDistance)
        {
            Array.Sort(spriteDistance, sprites);
            Array.Reverse(sprites);
            Array.Reverse(spriteDistance);
        }

        private void CheckSpritesIntersection(IntPtr renderer, Sprite[] sprites)
        {
            var rect = new SDL.SDL_Rect
            {
                x = window.GetWight() / 2 - 25,
                y = window.GetHeight() / 2 - 25,
                w = 50,
                h = 50
            };
            
            SDL.SDL_RenderDrawRect(renderer, ref rect);

            foreach (var sprite in sprites)
            {
                SDL.SDL_Rect result = sprite.GetSpriteIntersectionArea(camera.Pos, camera.Dir, camera.Plane, window.GetWight(),
                         window.GetHeight(), camera.Pitch, rect);
                if (result.h != 0 && result.w != 0)
                {
                    Console.WriteLine($"x:{result.x}, y: {result.y}");
                    SDL.SDL_RenderDrawRect(renderer, ref result);
                }
            }
        }
    }
}