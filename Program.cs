// See https://aka.ms/new-console-template for more information
using System;
using SDL2;

//Initilizes SDL._Init
if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
{
    Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
}

RaycastEngine.Window window = new RaycastEngine.Window("SDL .NET 6 Tutorial", 640, 480);

window.Titile = "Test";

// Initilizes SDL_image for use with png files.
if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
{
    Console.WriteLine($"There was an issue initilizing SDL2_Image {SDL_image.IMG_GetError()}");
}

var renderer = window.GetRenderer();

var running = true;

//Creating varibles for SDL.SDL_Point[]

int pointNum = 0;

SDL.SDL_Point[] points = new SDL.SDL_Point[window.GetWight() * 2];

//Filling SDL.SDL_Point[] with coordinates
for (int x = 0; x < window.GetWight(); x++)
{
    points[pointNum].x = x;
    points[pointNum].y = window.GetHeight() - (window.GetHeight() / 4) * 3;
    points[pointNum + 1].x = x;
    points[pointNum + 1].y = window.GetHeight() - window.GetHeight() / 4;

    pointNum += 2;
}

//Creating list for saving position where "click" was
List<SDL.SDL_Point> lastPosition = new List<SDL.SDL_Point>();

bool mouseState = false;

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

            case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    mouseState = true;
                }
                break;

            case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                if (e.button.button == SDL.SDL_BUTTON_LEFT)
                {
                    mouseState = false;
                }
                break;

            case SDL.SDL_EventType.SDL_KEYDOWN:
                if (e.key.keysym.sym == SDL.SDL_Keycode.SDLK_c) 
                {
                    lastPosition.Clear();
                }
                break;
        }
    }
    if (mouseState)
    {
        SDL.SDL_GetMouseState(out int x_, out int y_);
        lastPosition.Add(new SDL.SDL_Point() { x = x_, y = y_ });
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
    SDL.SDL_RenderDrawLines(renderer, points, points.Count());

    
    foreach (var pos in lastPosition)
    {
        Rect savedRect = new Rect(50, 50);
        savedRect.SetPosition(pos.x, pos.y);
        savedRect.Draw(renderer);
    }

    Rect drawRect = new Rect(50, 50);
    drawRect.SetPosition(x, y);
    drawRect.Draw(renderer);
      

    // Switches out the currently presented render surface with the one we just did work on.
    SDL.SDL_RenderPresent(renderer);


}

// Clean up the resources that were created.
SDL.SDL_DestroyRenderer(renderer);
SDL.SDL_DestroyWindow(renderer);
SDL.SDL_Quit();

class Rect
{
    public SDL.SDL_Vertex[] Vertices;
    public int[] Indices;

    public Rect(int Width, int Height)
    {
        Vertices = new SDL.SDL_Vertex[4];           //Should be lowercase
        Indices = new int[] { 0, 1, 2, 0, 3, 2 };

        int halfWight = Width / 2;
        int halfHight = Height / 2;

        Vertices[0].position.x = -halfWight;
        Vertices[0].position.y = -halfHight;
        Vertices[0].color.r = 255;
        Vertices[0].color.g = 0;
        Vertices[0].color.b = 0;
        Vertices[0].color.a = 255;

        Vertices[1].position.x = halfWight;
        Vertices[1].position.y = -halfHight;
        Vertices[1].color.r = 0;
        Vertices[1].color.g = 255;
        Vertices[1].color.b = 0;
        Vertices[1].color.a = 255;

        Vertices[2].position.x = halfWight;
        Vertices[2].position.y = halfHight;
        Vertices[2].color.r = 0;
        Vertices[2].color.g = 0;
        Vertices[2].color.b = 255;
        Vertices[2].color.a = 255;

        Vertices[3].position.x = -halfWight;
        Vertices[3].position.y = halfHight;
        Vertices[3].color.r = 0;
        Vertices[3].color.g = 255;
        Vertices[3].color.b = 0;
        Vertices[3].color.a = 255;
    }

    // Render a square out of 2 triangles
    public void Draw(IntPtr renderer)
    {
        SDL.SDL_RenderGeometry(renderer, IntPtr.Zero, Vertices, Vertices.Count(), Indices, Indices.Count());
    }

    public void SetPosition(int x, int y)
    {
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].position.x += x;
            Vertices[i].position.y += y;
        }
    }
}
