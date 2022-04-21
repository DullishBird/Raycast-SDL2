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
        private SDL.SDL_Rect rect;
        private SDL.SDL_Color color;


        public RenderText()
        {
            font = SDL_ttf.TTF_OpenFont("C:/Windows/Fonts/Arial.ttf", 15);

            color = new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 };

            rect = new SDL.SDL_Rect
            {
                x = 0,
                y = 0,
                w = 20,
                h = 20
            };
        }
        public void Draw(IntPtr renderer, string text, int x, int y)
        {
            IntPtr solid = SDL_ttf.TTF_RenderText_Solid(font, text, color);
            SDL.SDL_Surface surface = (SDL.SDL_Surface)Marshal.PtrToStructure(solid, typeof(SDL.SDL_Surface));
            SDL.SDL_Rect oldRect = new SDL.SDL_Rect
            {
                x = 0,
                y = 0,
                w = surface.w,
                h = surface.h
            };
            rect.w = surface.w;
            rect.h = surface.h;
            rect.x = x;
            rect.y = y;
            IntPtr message = SDL.SDL_CreateTextureFromSurface(renderer, solid);
            SDL.SDL_RenderCopy(renderer, message, ref oldRect, ref GetRect());
            SDL.SDL_DestroyTexture(message);            
        }

        //public string CreateText(double frameTime)
        //{
        //    return Math.Round(1.0 / frameTime).ToString();
        //}

        public IntPtr GetFont()
        {
            return font;
        }

        public ref SDL.SDL_Rect GetRect()
        {
            return ref rect;
        }

        ~RenderText()
        {
            SDL_ttf.TTF_CloseFont(font);
        }
        //public IntPtr CreateMessage(IntPtr renderer, double frameTime)
        //{
        //    return SDL.SDL_CreateTextureFromSurface(renderer, SDL_ttf.TTF_RenderText_Solid(font, CreateText(frameTime), color));
        //}
    }
}
