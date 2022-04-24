using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace RaycastEngine
{
    public class RenderText
    {
        private IntPtr font;
        private SDL.SDL_Color color;


        public RenderText()
        {
            font = SDL_ttf.TTF_OpenFont("C:/Windows/Fonts/Arial.ttf", 15);

            color = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };
        }

        public void Draw(IntPtr renderer, string text, int x, int y)
        {
            IntPtr textSolid = SDL_ttf.TTF_RenderText_Solid(font, text, color);
            SDL.SDL_Surface surface = (SDL.SDL_Surface)Marshal.PtrToStructure(textSolid, typeof(SDL.SDL_Surface));
            
            SDL.SDL_Rect dstrect = new SDL.SDL_Rect
            {
                x = x,
                y = y,
                w = surface.w,
                h = surface.h
            };

            SDL.SDL_Rect srcrect = new SDL.SDL_Rect
            {
                x = 0,
                y = 0,
                w = surface.w,
                h = surface.h
            };

            IntPtr message = SDL.SDL_CreateTextureFromSurface(renderer, textSolid);
            SDL.SDL_RenderCopy(renderer, message, ref srcrect, ref dstrect);
            SDL.SDL_DestroyTexture(message);            
        }

        ~RenderText()
        {
            SDL_ttf.TTF_CloseFont(font);
        }
    }
}
