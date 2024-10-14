// AsheronBuilder.Rendering/Camera.cs
using OpenTK.Mathematics;
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AsheronBuilder.Rendering
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
        private float _movementSpeed = 0.1f;
        private float _mouseSensitivity = 0.1f;
        private bool _isRightMouseDown = false;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            Front = -Vector3.UnitZ;
            Up = Vector3.UnitY;
            UpdateVectors();
        }

        public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Front, Up);

        public Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, AspectRatio, 0.1f, 1000f);

        public void UpdateVectors()
        {
            Front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
            Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
            Front.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
            Front = Vector3.Normalize(Front);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        public void Move(Vector3 direction, float deltaTime)
        {
            Position += direction * _movementSpeed * deltaTime;
        }

        public void Rotate(float deltaX, float deltaY)
        {
            Yaw += deltaX * _mouseSensitivity;
            Pitch -= deltaY * _mouseSensitivity; // Inverted Y-axis for right-click
            Pitch = Math.Clamp(Pitch, -89f, 89f);
            UpdateVectors();
        }

        public void HandleKeyboardInput(bool[] keyStates, float deltaTime)
        {
            Vector3 moveDirection = Vector3.Zero;

            if (keyStates[(int)Keys.W]) moveDirection += Front;
            if (keyStates[(int)Keys.S]) moveDirection -= Front;
            if (keyStates[(int)Keys.A]) moveDirection -= Right;
            if (keyStates[(int)Keys.D]) moveDirection += Right;
            if (keyStates[(int)Keys.Space]) moveDirection += Vector3.UnitY;
            if (keyStates[(int)Keys.LeftShift]) moveDirection -= Vector3.UnitY;

            if (moveDirection != Vector3.Zero)
            {
                moveDirection.Normalize();
                Move(moveDirection, deltaTime);
            }
        }

        public void HandleMouseInput(float deltaX, float deltaY)
        {
            if (_isRightMouseDown)
            {
                Rotate(deltaX, deltaY);
            }
        }

        public void SetRightMouseDown(bool isDown)
        {
            _isRightMouseDown = isDown;
        }
    }
}