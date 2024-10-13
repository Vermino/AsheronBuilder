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
            Position = new Vector3(0, 5, 15); // Move the camera further back
            AspectRatio = aspectRatio;
            Front = -Vector3.UnitZ;
            Up = Vector3.UnitY;
            UpdateVectors();
            Debug.WriteLine($"Camera initialized at position {Position}");
        }

        public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Front, Up);

        public Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.1f, 1000f);

        private void UpdateVectors()
        {
            Front.X = MathF.Cos(Pitch) * MathF.Cos(Yaw);
            Front.Y = MathF.Sin(Pitch);
            Front.Z = MathF.Cos(Pitch) * MathF.Sin(Yaw);
            Front = Vector3.Normalize(Front);

            Vector3 right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(right, Front));
        }

        public void Move(Vector3 offset) => Position += offset;
        
    }
}