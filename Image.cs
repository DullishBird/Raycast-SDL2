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
        private List<UInt32>[] texture = new List<UInt32>[11];
        public List<UInt32>[] Texture { get { return texture; } }
        public List<UInt32>[] FillTextureList(int windowWight, int windowHeight)
        {
            for (int i = 0; i < 11; i++) texture[i] = Enumerable.Repeat(0u, windowHeight * windowWight).ToList();
            return texture;
        }

        public List<uint>[] LoadTexures(string path)
        {
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
    }
}
