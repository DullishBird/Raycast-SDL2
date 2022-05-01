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

        int texWidth = 64;
        int texHeight = 64;

        List<UInt32>[] texture = new List<UInt32>[11];
        Window window = new Window("SDL .NET 6 Tutorial", 640, 480);

        struct Sprite 
        {
            public double x;
            public double y;
            public int texture;
            public int uDiv;
            public int vDiv;
            public float vMove;

            public Sprite(double x_, double y_, int texture_, int uDiv_ = 1, int vDiv_ = 1, float vMove_ = 0.0f)
            {
                x = x_;
                y = y_;
                texture = texture_;
                uDiv = uDiv_;
                vDiv = vDiv_;
                vMove = vMove_;
            }
        }

        static int numSprites = 19;
                
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
        }

        public void Start()
        {
            float time = 0; //time of current frame
            float oldTime = 0; //time of previous frame
            Vector2 camDir = new Vector2(-1, 0);
            Vector2 camPos = new Vector2(22, 12);
            Vector2 camPlane = new Vector2(0, 0.66f);
            float pitch = 0f;
            float posZ = 0f;

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
                new Sprite (20.5, 11.5, 10), 
              
                  //green lights in every room
                new Sprite (18.5,4.5, 10),
                new Sprite (10.0,4.5, 10),
                new Sprite (10.0,12.5,10),
                new Sprite (3.5, 6.5, 10),
                new Sprite (3.5, 20.5,10),
                new Sprite (3.5, 14.5,10),
                new Sprite (14.5,20.5,10),

                //row of pillars in front of wall: fisheye test
                new Sprite (18.5, 10.5, 9),
                new Sprite (18.5, 11.5, 9),
                new Sprite (18.5, 12.5, 9),

                //some barrels around the map
                new Sprite (21.5, 1.5, 8, 2, 2, 128.0f),
                new Sprite (15.5, 1.5, 8, 2, 2, 128.0f),
                new Sprite (16.0, 1.8, 8, 2, 2, 128.0f),
                new Sprite (16.2, 1.2, 8, 2, 2, 128.0f),
                new Sprite (3.5,  2.5, 8, 2, 2, 128.0f),
                new Sprite (9.5, 15.5, 8, 2, 2, 128.0f),
                new Sprite (10.0, 15.1,8, 2, 2, 128.0f),
                new Sprite (10.5, 15.8,8, 2, 2, 128.0f)
            };

            //Window window = new Window("SDL .NET 6 Tutorial", 640, 480);
            RenderText renderText = new RenderText();

            window.Titile = "Test";

            var renderer = window.GetRenderer();

            var running = true;

            var windowWight = window.GetWight();
            var windowHeight = window.GetHeight();

            UInt32[] buffer = new UInt32[windowHeight * windowWight];
            
            //1D Zbuffer
            double[] ZBuffer = new double[windowWight];

            //arrays used to sort the sprites
            int[] spriteOrder = new int[numSprites];
            double[] spriteDistance = new double[numSprites];

            for (int i = 0; i < 11; i++) texture[i] = Enumerable.Repeat(0u, windowHeight * windowWight).ToList();

            //Choose between generated textures and Wolfenstein 3D textures here
            
            //LoadGeneratedTexures();
            LoadTexures();
            
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

                                pitch += (oldMousePosY - mouseCamPosY) * vertMoveSpeed;

                                if (pitch > 200) pitch = 200;
                                if (pitch < -200) pitch = -200;

                                float currentMouseSpeed = mrotSpeed * (oldMousePosX - mouseCamPosX);
                                Rotating(currentMouseSpeed, ref camDir, ref camPlane);
                                oldMousePosX = windowWight / 2;
                                oldMousePosY = windowHeight / 2;

                                break;
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
                    if (worldMap.GetWallType((int)(camPos.X + camDir.X * moveSpeed), (int)camPos.Y) == 0) camPos.X += camDir.X * moveSpeed;
                    if (worldMap.GetWallType((int)camPos.X, (int)(camPos.Y + camDir.Y * moveSpeed)) == 0) camPos.Y += camDir.Y * moveSpeed;
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_S] == 1)
                {
                    if (worldMap.GetWallType((int)(camPos.X - camDir.X * moveSpeed), (int)camPos.Y) == 0) camPos.X -= camDir.X * moveSpeed;
                    if (worldMap.GetWallType((int)(camPos.X), (int)(camPos.Y - camDir.Y * moveSpeed)) == 0) camPos.Y -= camDir.Y * moveSpeed;
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_A] == 1)
                {
                    if (worldMap.GetWallType((int)(camPos.X - camPlane.X * moveSpeed), (int)camPos.Y) == 0) camPos.X -= camPlane.X * moveSpeed;
                    if (worldMap.GetWallType((int)camPos.X, (int)(camPos.Y - camPlane.Y * moveSpeed)) == 0) camPos.Y -= camPlane.Y * moveSpeed;
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_D] == 1)
                {
                    if (worldMap.GetWallType((int)(camPos.X + camPlane.X * moveSpeed), (int)camPos.Y) == 0) camPos.X += camPlane.X * moveSpeed;
                    if (worldMap.GetWallType((int)camPos.X, (int)(camPos.Y + camPlane.Y * moveSpeed)) == 0) camPos.Y += camPlane.Y * moveSpeed;
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_Q] == 1)
                {
                    Rotating(rotSpeed, ref camDir, ref camPlane);
                }

                if (keyState[(int)SDL.SDL_Scancode.SDL_SCANCODE_E] == 1)
                {
                    Rotating(-rotSpeed, ref camDir, ref camPlane);
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
                        renderer,
                        buffer,
                        ZBuffer,
                        spriteOrder,
                        spriteDistance,
                        sprite, pitch, posZ);

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
                            pitch_ = windowWight * 4;

                        IntPtr surface = SDL.SDL_CreateRGBSurfaceFrom(bufferIntPtr, windowWight, windowHeight, depth, pitch_, rmask, gmask, bmask, amask);

                        frameTexture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
                        SDL.SDL_FreeSurface(surface);
                    }
                }

                //Now render the texture target to our screen, but upside down
                
                if (SDL.SDL_RenderCopyEx(renderer, frameTexture, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE) < 0)
                {
                    Console.WriteLine($"There was an issue with SDL_RenderCopyEx. {SDL.SDL_GetError()}");
                };

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
                SDL.SDL_WarpMouseInWindow(window.GetNative(), windowWight / 2, windowHeight / 2);

                // Switches out the currently presented render surface with the one we just did work on.
                SDL.SDL_RenderPresent(renderer);
            }
            // Clean up the resources that were created.
            SDL.SDL_Quit();
        }

        private void DrawMap(int windowWight, int windowHeight, Vector2 camDir, Vector2 camPlane,
                             Vector2 camPos, IntPtr renderer, UInt32[] buffer, double[] ZBuffer,
                             int[] spriteOrder, double[] spriteDistance, Sprite[] sprite, float pitch, float posZ)
        {
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
                float camZ = (float)(is_floor ? (0.5 * windowHeight + posZ) : (0.5 * windowHeight - posZ));
                //float posZ = 0.5f * windowHeight;

                // Horizontal distance from the camera to the floor for the current row.
                // 0.5 is the z position exactly in the middle between floor and ceiling.
                float rowDistance = camZ / p;

                // calculate the real world step vector we have to add for each x (parallel to camera plane)
                // adding step by step avoids multiplications with a weight in the inner loop
                float floorStepX = rowDistance * (rayDirX1 - rayDirX0) / windowWight;
                float floorStepY = rowDistance * (rayDirY1 - rayDirY0) / windowWight;

                // real world coordinates of the leftmost column. This will be updated as we step to the right.
                float floorX = camPos.X + rowDistance * rayDirX0;
                float floorY = camPos.Y + rowDistance * rayDirY0;

                for (int x = 0; x < windowWight; ++x)
                {
                    // the cell coord is simply got from the integer parts of floorX and floorY
                    int cellX = (int)(floorX);
                    int cellY = (int)(floorY);

                    // get the texture coordinate from the fractional part
                    int tx = (int)(texWidth * (floorX - cellX)) & (texWidth - 1);
                    int ty = (int)(texHeight * (floorY - cellY)) & (texHeight - 1);

                    floorX += floorStepX;
                    floorY += floorStepY;

                    // choose texture and draw the pixel
                    int floorTexture = 3;
                    int ceilingTexture = 6;
                    UInt32 color;

                    if (is_floor)
                    {
                        // floor
                        color = texture[floorTexture][texWidth * ty + tx];
                        //color = (color >> 1) & 8355711; // make a bit darker
                        buffer[y * windowWight + x] = color;
                    }
                    else
                    {
                        //ceiling (symmetrical, at screenHeight - y - 1 instead of y)
                        color = texture[ceilingTexture][texWidth * ty + tx];
                        //color = (color >> 1) & 8355711; // make a bit darker
                        buffer[y * windowWight + x] = color;
                    }
                }
            }
            //WALL CASTING
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
                else           perpWallDist = (sideDistY - deltaDistY);

                //Calculate height of line to draw on screen
                int lineHeight = (int)(windowHeight / perpWallDist);

                //calculate lowest and highest pixel to fill in current stripe
                int drawStart = (int)(-lineHeight / 2 + windowHeight / 2 + pitch + (posZ / perpWallDist));
                if (drawStart < 0) drawStart = 0;

                int drawEnd = (int)(lineHeight / 2 + windowHeight / 2 + pitch + (posZ / perpWallDist));
                if (drawEnd >= windowHeight) drawEnd = windowHeight - 1;
                //texturing calculations
                int texNum = worldMap.GetWallType(mapX, mapY) - 1; //1 subtracted from it so that texture 0 can be used!

                //calculate value of wallX
                double wallX; //where exactly the wall was hit
                if (side == 0) wallX = camPos.Y + perpWallDist * rayCamDirY;
                else wallX = camPos.X + perpWallDist * rayCamDirX;
                wallX -= Math.Floor((wallX));

                //x coordinate on the texture
                int texX = (int)(wallX * (double)(texWidth));
                if (side == 0 && rayCamDirX > 0)
                    texX = texWidth - texX - 1;
                if (side == 1 && rayCamDirY < 0)
                    texX = texWidth - texX - 1;

                // How much to increase the texture coordinate per screen pixel
                double step = 1.0 * texHeight / lineHeight;
                // Starting texture coordinate
                double texPos = (drawStart - pitch - (posZ / perpWallDist) - windowHeight / 2 + lineHeight / 2) * step;
                for (int y = drawStart; y < drawEnd; y++)
                {
                    // Cast the texture coordinate to integer, and mask with (texHeight - 1) in case of overflow
                    int texY = (int)texPos & (texHeight - 1);
                    texPos += step;
                    UInt32 color = texture[texNum][texHeight * texY + texX];
                    ////make color darker for y-sides: R, G and B byte each divided through two with a "shift" and an "and"
                    //if (side == 1) color = (color >> 1) & 8355711;

                    buffer[y * windowWight + x] = color;
                }
                ZBuffer[x] = perpWallDist;
            }
            //SPRITE CASTING
            //sort sprites from far to close
            for (int i = 0; i < numSprites; i++)
            {
                spriteOrder[i] = i;
                spriteDistance[i] = ((camPos.X - sprite[i].x) * (camPos.X - sprite[i].x) + (camPos.Y - sprite[i].y) * (camPos.Y - sprite[i].y)); //sqrt not taken, unneeded
            }
            SortSprites(spriteOrder, spriteDistance);

            //after sorting the sprites, do the projection and draw them
            for (int i = 0; i < numSprites; i++)
            {
                //translate sprite position to relative to camera
                double spriteX = sprite[spriteOrder[i]].x - camPos.X;
                double spriteY = sprite[spriteOrder[i]].y - camPos.Y;

                //transform sprite with the inverse camera matrix
                // [ planeX   dirX ] -1                                       [ dirY      -dirX ]
                // [               ]       =  1/(planeX*dirY-dirX*planeY) *   [                 ]
                // [ planeY   dirY ]                                          [ -planeY  planeX ]

                double invDet = 1.0 / (camPlane.X * camDir.Y - camDir.X * camPlane.Y); //required for correct matrix multiplication

                double transformX = invDet * (camDir.Y * spriteX - camDir.X * spriteY);
                double transformY = invDet * (-camPlane.Y * spriteX + camPlane.X * spriteY); //this is actually the depth inside the screen, that what Z is in 3D

                int spriteScreenX = (int)((windowWight / 2) * (1 + transformX / transformY));

                //parameters for scaling and moving the sprites
                //int uDiv = 1;
                //int vDiv = 1;
                //float vMove = sprite[vMove];

                
                int vMoveScreen = (int)((sprite[spriteOrder[i]].vMove / transformY) + pitch + posZ / transformY);
                //calculate height of the sprite on screen
                int spriteHeight = Math.Abs((int)(windowHeight / (transformY))) / sprite[spriteOrder[i]].vDiv; //using 'transformY' instead of the real distance prevents fisheye
                //calculate lowest and highest pixel to fill in current stripe
                
                int drawStartY = -spriteHeight / 2 + windowHeight / 2 + vMoveScreen;
                if (drawStartY < 0) drawStartY = 0;
                int drawEndY = spriteHeight / 2 + windowHeight / 2 + vMoveScreen;
                if (drawEndY >= windowHeight) drawEndY = windowHeight - 1;

                //calculate width of the sprite
                int spriteWidth = Math.Abs((int)(windowHeight / (transformY))) / sprite[spriteOrder[i]].uDiv;
                int drawStartX = -spriteWidth / 2 + spriteScreenX;
                if (drawStartX < 0) drawStartX = 0;
                int drawEndX = spriteWidth / 2 + spriteScreenX;
                if (drawEndX >= windowWight) drawEndX = windowWight - 1;

                //loop through every vertical stripe of the sprite on screen
                for (int stripe = drawStartX; stripe < drawEndX; stripe++)
                {
                    int texX = (int)(256 * (stripe - (-spriteWidth / 2 + spriteScreenX)) * texWidth / spriteWidth) / 256;
                    if (texX < 0) texX = 0; //костыль
                    //the conditions in the if are:
                    //1) it's in front of camera plane so you don't see things behind you
                    //2) it's on the screen (left)
                    //3) it's on the screen (right)
                    //4) ZBuffer, with perpendicular distance
                    if (transformY > 0 && stripe > 0 && stripe < windowWight && transformY < ZBuffer[stripe])
                        for (int y = drawStartY; y < drawEndY; y++) //for every pixel of the current stripe
                        {
                            int d = (y - vMoveScreen) * 256 - windowHeight * 128 + spriteHeight * 128; //256 and 128 factors to avoid floats
                            int texY = ((d * texHeight) / spriteHeight) / 256;
                            if (texY < 0) texY = 0; //тут тоже
                            UInt32 color = texture[sprite[spriteOrder[i]].texture][texWidth * texY + texX]; //get current color from the texture
                            if ((color & 0xFFFFFF00) != 0) buffer[y * windowWight + stripe] = color; //paint pixel if it isn't black, black is the invisible color
                        }
                }
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

        private List<uint> GetTexturePixels(string path)
        {
            List<uint> pixels = new List<uint>();
            IntPtr image = SDL_image.IMG_Load(path);

            SDL.SDL_Surface surfaceImage = (SDL.SDL_Surface)Marshal.PtrToStructure(image, typeof(SDL.SDL_Surface));
            SDL.SDL_PixelFormat format = (SDL.SDL_PixelFormat)Marshal.PtrToStructure(surfaceImage.format, typeof(SDL.SDL_PixelFormat));

            unsafe
            {
                var srcPixelPtr = (byte*)surfaceImage.pixels.ToPointer();
                for (int y = 0; y < surfaceImage.h; y++)
                {
                    for (int x = 0; x < surfaceImage.w; x++)
                    {
                        UInt32 pixelColor = *(UInt32*)(srcPixelPtr + y * surfaceImage.pitch + x * format.BytesPerPixel);

                        UInt32 red = pixelColor & format.Rmask;
                        UInt32 green = pixelColor & format.Gmask;
                        UInt32 blue = pixelColor & format.Bmask;
                        UInt32 alpha = 0x000000ff;

                        UInt32 redWithShift = red << 24;
                        UInt32 greenWithShift = green << 8;
                        UInt32 blueWithShift = blue >> 8;

                        pixelColor = redWithShift;
                        pixelColor |= greenWithShift;
                        pixelColor |= blueWithShift;
                        pixelColor |= alpha;

                        pixels.Add(pixelColor);
                    }
                }
            }
            return pixels;
        }
        private List<uint>[] LoadGeneratedTexures()
        {
            for (int i = 0; i < 8; i++) 
                texture[i] = Enumerable.Repeat(0u, window.GetHeight() * window.GetWight()).ToList();

            for (int x = 0; x < texWidth; x++)
            {
                for (int y = 0; y < texHeight; y++)
                {
                    int xorcolor = (x * 256 / texWidth) ^ (y * 256 / texHeight);
                    int ycolor = y * 256 / texWidth;
                    int xycolor = y * 128 / texHeight + x * 128 / texWidth;

                    texture[0][texWidth * y + x] = 65536 * 254 * Convert.ToUInt32(x != y && x != texWidth - y); //flat red texture with black cross
                    texture[1][texWidth * y + x] = (UInt32)(xycolor + 256 * xycolor + 65536 * xycolor); //sloped greyscale
                    texture[2][texWidth * y + x] = (UInt32)(256 * xycolor + 65536 * xycolor); //sloped yellow gradient
                    texture[3][texWidth * y + x] = (UInt32)(xorcolor + 256 * xorcolor + 65536 * xorcolor); //xor greyscale
                    texture[4][texWidth * y + x] = (UInt32)(256 * xorcolor); //xor green
                    texture[5][texWidth * y + x] = (UInt32)(65536 * 192 * (x % 16 & y % 16)); //red bricks
                    texture[6][texWidth * y + x] = (UInt32)(65536 * ycolor); //red gradient
                    texture[7][texWidth * y + x] = 128 + 256 * 128 + 65536 * 128; //flat grey texture
                }
            }
            return texture;
        }

        private List<uint>[] LoadTexures()
        {
            for (int i = 0; i < 8; i++)
                texture[i] = Enumerable.Repeat(0u, window.GetHeight() * window.GetWight()).ToList();
            //load textures
            texture[0] = GetTexturePixels(path + "/res/pics/eagle.png");
            texture[1] = GetTexturePixels(path + "/res/pics/redbrick.png");
            texture[2] = GetTexturePixels(path + "/res/pics/purplestone.png");
            texture[3] = GetTexturePixels(path + "/res/pics/greystone.png");
            texture[4] = GetTexturePixels(path + "/res/pics/bluestone.png");
            texture[5] = GetTexturePixels(path + "/res/pics/mossy.png");
            texture[6] = GetTexturePixels(path + "/res/pics/wood.png");
            texture[7] = GetTexturePixels(path + "/res/pics/colorstone.png");
            //load sprites
            texture[8] = GetTexturePixels(path + "/res/pics/barrel.png");
            texture[9] = GetTexturePixels(path + "/res/pics/pillar.png");
            texture[10] = GetTexturePixels(path + "/res/pics/greenlight.png");
            return texture;
        }

        public void SortSprites(int[] spriteOrder, double[] spriteDistance)
        {
            Array.Sort(spriteDistance, spriteOrder);
            Array.Reverse(spriteOrder);
            Array.Reverse(spriteDistance);
        }
    }   
}