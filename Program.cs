// See https://aka.ms/new-console-template for more information
using System;
using SDL2;

// Initilizes SDL.
if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
{
    Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
}

// Create a new window given a title, size, and passes it a flag indicating it should be shown.
int windowHeight = 480;
int windowWidth = 640;
var window = SDL.SDL_CreateWindow("SDL .NET 6 Tutorial", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED,
                                windowWidth, windowHeight, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

if (window == IntPtr.Zero)
{
    Console.WriteLine($"There was an issue creating the window. {SDL.SDL_GetError()}");
}

// Creates a new SDL hardware renderer using the default graphics device with VSYNC enabled.
var renderer = SDL.SDL_CreateRenderer(window, 
                                        -1, 
                                        SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | 
                                        SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

if (renderer == IntPtr.Zero)
{
    Console.WriteLine($"There was an issue creating the renderer. {SDL.SDL_GetError()}");
}

// Initilizes SDL_image for use with png files.
if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
{
    Console.WriteLine($"There was an issue initilizing SDL2_Image {SDL_image.IMG_GetError()}");
}

var running = true;

//Creating varibles for SDL.SDL_Point[]

int pointNum = 0;

SDL.SDL_Point[] points = new SDL.SDL_Point[windowWidth * 2];

//Filling SDL.SDL_Point[] with coordinates
for (int x = 0; x < windowWidth; x++)
{
    points[pointNum].x = x;
    points[pointNum].y = windowHeight - (windowHeight / 4) * 3;
    points[pointNum + 1].x = x;
    points[pointNum + 1].y = windowHeight - windowHeight / 4;

    pointNum += 2;
}

// Main loop for the program
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
        }
    }

    // Sets the color that the screen will be cleared with.
    if (SDL.SDL_SetRenderDrawColor(renderer, 135, 206, 235, 255) < 0)
    {
        Console.WriteLine($"There was an issue with setting the render draw color. {SDL.SDL_GetError()}");
    }

    // Clears the current render surface.
    if (SDL.SDL_RenderClear(renderer) < 0)
    {
        Console.WriteLine($"There was an issue with clearing the render surface. {SDL.SDL_GetError()}");
    }

    SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);

    SDL.SDL_RenderDrawLine(renderer, 0, 0, 640, 480);

    var rect = new SDL.SDL_Rect
    {
        x = 300,
        y = 100,
        w = 50,
        h = 50  
    };

    SDL.SDL_SetRenderDrawColor(renderer, 255, 150, 10, 255);

    // Render an square
    SDL.SDL_RenderFillRect(renderer, ref rect);
    
    SDL.SDL_SetRenderDrawColor(renderer, 0, 11, 250, 255);

    // Render a dot
    SDL.SDL_RenderDrawPoint(renderer, 20, 20);

    // Getting mouse xy coordinates
    SDL.SDL_GetMouseState(out int x, out int y);
    // Console.WriteLine(string.Format("x {0}, y {1}", x, y)); 

    // Render block of lines
    SDL.SDL_RenderDrawLines(renderer, points, windowWidth * 2);

    SDL.SDL_Vertex[] array = new SDL.SDL_Vertex[4];

    array[0].position.x = x - 25;
    array[0].position.y = y - 25;
    array[0].color.r = 255;
    array[0].color.g = 0;
    array[0].color.b = 0;
    array[0].color.a = 255;

    array[1].position.x = x + 25;
    array[1].position.y =  y - 25;
    array[1].color.r = 0;
    array[1].color.g = 255;
    array[1].color.b = 0;
    array[1].color.a = 255;

    array[2].position.x = x + 25;
    array[2].position.y = y + 25;
    array[2].color.r = 0;
    array[2].color.g = 0;
    array[2].color.b = 255;
    array[2].color.a = 255;

    array[3].position.x = x - 25;
    array[3].position.y =  y + 25;
    array[3].color.r = 0;
    array[3].color.g = 255;
    array[3].color.b = 0;
    array[3].color.a = 255;

    int[] indices = { 0, 1, 2, 0, 3, 2 };

    // Render a square out of 2 triangles
    SDL.SDL_RenderGeometry(renderer, IntPtr.Zero, array, array.Count(), indices, indices.Count());

    // Switches out the currently presented render surface with the one we just did work on.
    SDL.SDL_RenderPresent(renderer);
    
    
}

// Clean up the resources that were created.
SDL.SDL_DestroyRenderer(renderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();