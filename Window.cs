using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace RaycastEngine
{
    public class Window
    {
        public string Titile 
        {
            get
            {
                return SDL.SDL_GetWindowTitle(window);
            }
            set
            {
                SDL.SDL_SetWindowTitle(window, value);
            }
        }

        private IntPtr window;
        private IntPtr renderer;

        public Window(string title, int width, int height)
        {
            // Create a new window given a title, size, and passes it a flag indicating it should be shown.
            window = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            if (window == IntPtr.Zero)
            {
                Console.WriteLine($"There was an issue creating the window. {SDL.SDL_GetError()}");
            }

            // Creates a new SDL hardware renderer using the default graphics device with VSYNC enabled.
            renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                                        SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine($"There was an issue creating the renderer. {SDL.SDL_GetError()}");
            }
        }

        public void GetSize(out int w, out int h)
        {
            SDL.SDL_GetWindowSize(window, out w, out h);
        }

        public int GetWight()
        {            
            GetSize(out int w, out _);
            return w;
        }

        public int GetHeight()
        {
            GetSize(out _, out int h);
            return h;
        }

        public void Resize(int wight, int height)
        {
            GetSize(out int w, out int h);

            if (wight != w && height != h)
            {
                SDL.SDL_SetWindowSize(window, wight, height);
            }
        }

        public IntPtr GetNative()
        {
            return window;
        }

        public IntPtr GetRenderer() 
        {
            return renderer;
        }
    }

}
