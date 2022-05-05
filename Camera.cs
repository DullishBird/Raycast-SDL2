using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaycastEngine
{
    public class Camera
    {
        private Vector2 dir;
        public Vector2 Dir { get { return dir; } set { dir = value; } }
        private Vector3 pos;
        public Vector3 Pos { get { return pos; } set { pos = value; } }
        private Vector2 plane;
        public Vector2 Plane { get { return plane; } set { plane = value; } }
        private float pitch;
        public float Pitch { get { return pitch; } set { pitch = value; } }

        public Camera(Vector3 pos_, Vector2 dir_, Vector2 plane_, float pitch_)
        {
            dir = dir_;
            pos = pos_;
            plane = plane_;
            pitch = pitch_;
        }

        public void Rotate(float rotSpeed)
        {
            float oldCamDirX = Dir.X;
            Dir = new Vector2(Dir.X * MathF.Cos(rotSpeed) - Dir.Y * MathF.Sin(rotSpeed),
                              oldCamDirX * MathF.Sin(rotSpeed) + Dir.Y * MathF.Cos(rotSpeed));

            float oldCamPlaneX = Plane.X;
            Plane = new Vector2(Plane.X * MathF.Cos(rotSpeed) - Plane.Y * MathF.Sin(rotSpeed),
                                oldCamPlaneX * MathF.Sin(rotSpeed) + Plane.Y * MathF.Cos(rotSpeed));
        }
    }
}
