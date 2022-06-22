using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaycastEngine
{
    public class Sprite
    {
        //RaycastRenderer raycast = new RaycastRenderer();

        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        private int textureIndex;
        private int uDiv;
        private int vDiv;
        private float vMove;
        private float transperency = 1f;
        public float Transperency
        {
            get { return transperency; }
            set { transperency = value; }
        }
        public Sprite(Vector2 position_, int textureIndex_, int uDiv_ = 1, int vDiv_ = 1, float vMove_ = 0.0f)
        {
            position = position_;
            textureIndex = textureIndex_;
            uDiv = uDiv_;
            vDiv = vDiv_;
            vMove = vMove_;
        }

        public void Draw(Vector3 camPos, Vector2 camDir, Vector2 camPlane, int windowWight,
                         int windowHeight, float pitch,/* int texWidth, int texHeight,*/
                         uint[] buffer, double[] ZBuffer, List<Image> textures) 
        {
            checkTextureIndex(textureIndex);
            //translate sprite position to relative to camera
            double spriteX = position.X - camPos.X;
            double spriteY = position.Y - camPos.Y;

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


            int vMoveScreen = (int)((vMove / transformY) + pitch + camPos.Z / transformY);
            //calculate height of the sprite on screen
            int spriteHeight = Math.Abs((int)(windowHeight / (transformY))) / vDiv; //using 'transformY' instead of the real distance prevents fisheye
                                                                                                           //calculate lowest and highest pixel to fill in current stripe

            int drawStartY = -spriteHeight / 2 + windowHeight / 2 + vMoveScreen;
            if (drawStartY < 0) drawStartY = 0;
            int drawEndY = spriteHeight / 2 + windowHeight / 2 + vMoveScreen;
            if (drawEndY >= windowHeight) drawEndY = windowHeight - 1;

            //calculate width of the sprite
            int spriteWidth = Math.Abs((int)(windowHeight / (transformY))) / uDiv;
            int drawStartX = -spriteWidth / 2 + spriteScreenX;
            if (drawStartX < 0) drawStartX = 0;
            int drawEndX = spriteWidth / 2 + spriteScreenX;
            if (drawEndX >= windowWight) drawEndX = windowWight - 1;

            //loop through every vertical stripe of the sprite on screen
            for (int stripe = drawStartX; stripe < drawEndX; stripe++)
            {
                int texX = (int)(256 * (stripe - (-spriteWidth / 2 + spriteScreenX)) * textures[textureIndex].Widht / spriteWidth) / 256;
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
                        int texY = ((d * textures[textureIndex].Hight) / spriteHeight) / 256;
                        if (texY < 0) texY = 0; //тут тоже
                        UInt32 color = textures[textureIndex][texX, texY]; //get current color from the texture
                        
                        //if ((color & 0xFFFFFF00) != 0) buffer[y * windowWight + stripe] = color; //paint pixel if it isn't black, black is the invisible color
                        if ((color & 0xFFFFFF00) != 0) buffer[y * windowWight + stripe] = ColorMultiply(color, transperency) + ColorMultiply(buffer[y * windowWight + stripe], 1 - transperency);
                    }
            }
        }

        private static uint ColorMultiply(uint color, float factor)
        {
            UInt32 rmask, gmask, bmask, amask;
            rmask = 0xff000000;
            gmask = 0x00ff0000;
            bmask = 0x0000ff00;
            amask = 0x000000ff;

            uint red = color & rmask;
            uint green = color & gmask;
            uint blue = color & bmask;
            red = (uint)((red >> 24) * (factor)) << 24;
            green = (uint)((green >> 16) * (factor)) << 16;
            blue = (uint)((blue >> 8) * (factor)) << 8;

            color = red | green | blue | amask;
            return color;
        }
        
        private void checkTextureIndex(int textureIndex)
        {
            if (textureIndex == 9)
            {
                transperency = 0.5f;
            }
        }
        // сделать гетсет для texture, в клиенте цикл(все колонны прозрачные на 0,5
    }
}
