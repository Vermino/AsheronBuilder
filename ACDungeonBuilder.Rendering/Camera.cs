// ACDungeonBuilder.Rendering/Camera.cs

using OpenTK.Mathematics;
using System;

namespace ACDungeonBuilder.Rendering
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        private Vector3 _front = -Vector3.UnitZ;
        public Vector3 Front
        {
            get => _front;
            private set => _front = value;
        }
        public Vector3 Up { get; private set; } = Vector3.UnitY;
        public Vector3 Right { get; private set; }

        public float AspectRatio { get; set; }

        public float Yaw { get; set; } = -MathHelper.PiOver2;
        public float Pitch { get; set; }

        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            UpdateVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        public void UpdateVectors()
        {
            _front.X = MathF.Cos(Pitch) * MathF.Cos(Yaw);
            _front.Y = MathF.Sin(Pitch);
            _front.Z = MathF.Cos(Pitch) * MathF.Sin(Yaw);
            _front = Vector3.Normalize(_front);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        public void Move(Vector3 offset)
        {
            Position += offset;
        }

        public void Rotate(float deltaYaw, float deltaPitch)
        {
            Yaw += deltaYaw;
            Pitch += deltaPitch;

            Pitch = Math.Clamp(Pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);

            UpdateVectors();
        }
    }
}