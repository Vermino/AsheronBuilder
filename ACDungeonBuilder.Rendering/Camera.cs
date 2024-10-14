using OpenTK.Mathematics;
using System;
using System.Diagnostics;

namespace ACDungeonBuilder.Rendering
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 Front;
        public Vector3 Up;
        public Vector3 Right;
        public float AspectRatio { get; set; }
        public float Yaw { get; set; } = -MathHelper.PiOver2;
        public float Pitch { get; set; }

        public void Rotate(float deltaYaw, float deltaPitch)
        {
            Yaw += deltaYaw;
            Pitch += deltaPitch;
            Pitch = Math.Clamp(Pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            UpdateVectors();
        }

        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = new Vector3(0, 10, 10);
            AspectRatio = aspectRatio;
            Yaw = -90f;
            Pitch = -45f;
            Front = -Vector3.UnitZ;
            Up = Vector3.UnitY;
            UpdateVectors();
        }

        public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Front, Up);

        public Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.1f, 1000f);

        public void UpdateVectors()
        {
            Front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
            Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
            Front.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
            Front = Vector3.Normalize(Front);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        public void Move(Vector3 offset) => Position += offset;
        
    }
}