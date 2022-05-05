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
        public Vector2 Dir
        {
            get { return dir; }
            set { dir = value; }
        }
        private Vector2 pos;
        public Vector2 Pos
            { get { return pos; } set { pos = value; } }
        private Vector2 plane;
        public Vector2 Plane { get { return plane; } set { plane = value; } }
        //private float pitch;
        //private float posZ;

        public Camera()
        {
            dir = new Vector2(-1, 0);
            pos = new Vector2(22, 12);
            plane = new Vector2(0, 0.66f);
            //pitch = 0f;
            //posZ = 0f;
        }
        public void ChangeFOV(float fov)
        {
            plane.Y = fov;
        }

        public Vector2 GetDir() { return dir; }
        public Vector2 GetPos() { return pos; }
        public Vector2 GetPlane() { return plane; }

        //public static implicit operator Vector2(Camera v)
        //{
        //    throw new NotImplementedException();
        //}
        //public float GetPitch() { return pitch; }
        //public float GetPosZ() { return posZ; }


    }
}
