﻿using System;
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
            int mapWidth = 24;
            int mapHeight = 24;
            int screenWidth = 640;
            int screenHeight = 480;

            int[,] worldMap =
                {
              {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
              {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,0,0,0,3,0,0,0,1},
              {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,4,0,0,0,0,5,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,4,0,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
              {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

            double posX = 22, posY = 12;  //x and y start position
            double dirX = -1, dirY = 0; //initial direction vector
            double planeX = 0, planeY = 0.66; //the 2d raycaster version of camera plane

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
            }

            RaycastEngine.Window window = new RaycastEngine.Window("SDL .NET 6 Tutorial", 640, 480);

            window.Titile = "Test";

            var renderer = window.GetRenderer();

            var running = true;

            var windowWight = window.GetWight();
            var windowHeight = window.GetHeight();

            double moveSpeed = 1.0; //the constant value is in squares/second
            double rotSpeed = 0.1; //the constant value is in radians/second

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
                                if(worldMap[(int)(posX + dirX * moveSpeed),(int)posY] == 0) posX += dirX * moveSpeed;
                                if(worldMap[(int)posX,(int)(posY + dirY * moveSpeed)] == 0) posY += dirY * moveSpeed;
                            }
                            //move backwards if no wall behind you
                            if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_s)
                            {
                                if (worldMap[(int)(posX - dirX * moveSpeed), (int)posY] == 0) posX -= dirX * moveSpeed;
                                if (worldMap[(int)(posX), (int)(posY - dirY * moveSpeed)] == 0) posY -= dirY * moveSpeed;
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
                        if (worldMap[mapX, mapY] > 0) 
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
                    switch (worldMap[mapX,mapY])
                    {
                        case 1: color = new SDL.SDL_Color { r = 255, g = 0, b = 0, a = 255}; break; //red
                        case 2: color = new SDL.SDL_Color { r = 0, g = 255, b = 0, a = 255 }; break; //green
                        case 3: color = new SDL.SDL_Color { r = 0, g = 0, b = 255, a = 255 }; break; //blue
                        case 4: color = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 }; break; //white
                        default: color = new SDL.SDL_Color { r = 255, g = 255, b = 0, a = 255 }; break; //yellow
                    }

                    //give x and y sides different brightness
                    //if (side == 1) { color = color / 2; }

                    //draw the pixels of the stripe as a vertical line
                    SDL.SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, color.a);
                    SDL.SDL_RenderDrawLine(renderer, x, drawStart, x, drawEnd);
                }
                // Switches out the currently presented render surface with the one we just did work on.
                SDL.SDL_RenderPresent(renderer);

            }
            // Clean up the resources that were created.
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(renderer);
            SDL.SDL_Quit();
        }

    }
}
