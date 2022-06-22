using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SDL2;

namespace RaycastEngine
{
    public class Image
    {
        private List<uint> pixels = new List<uint>();
       
        private int widht = 0;
        private int hight = 0;

        public Image(string path, int i)
        {
            Load(path);
        }

        private void Load(string path)
        {
            IntPtr image = SDL_image.IMG_Load(path);

            SDL.SDL_Surface surfaceImage = (SDL.SDL_Surface)Marshal.PtrToStructure(image, typeof(SDL.SDL_Surface));
            SDL.SDL_PixelFormat format = (SDL.SDL_PixelFormat)Marshal.PtrToStructure(surfaceImage.format, typeof(SDL.SDL_PixelFormat));
            
            widht = surfaceImage.w;
            hight = surfaceImage.h;

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
        }

        public uint this[int x, int y]
        {
            get
            {
                return pixels[y * widht + x];
            }
        }
    }
}