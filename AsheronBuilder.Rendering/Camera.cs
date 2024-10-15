// AsheronBuilder.Rendering/Camera.cs

using OpenTK.Mathematics;
using System;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace AsheronBuilder.Rendering
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        
        public float Yaw { get; set; } = -90f;
        public float Pitch { get; set; } = 0f;
        private float _aspectRatio = 1.0f;
        public bool IsRightMouseDown { get; set; }
        public float AspectRatio { get; set; }

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            Up = Vector3.UnitY;
            UpdateVectors();
        }

        public void UpdateVectors()
        {
            Front = new Vector3(
                (float)Math.Cos(MathHelper.DegreesToRadians(Yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(Pitch)),
                (float)Math.Sin(MathHelper.DegreesToRadians(Pitch)),
                (float)Math.Sin(MathHelper.DegreesToRadians(Yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(Pitch))
            ).Normalized();

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45.0f),
                _aspectRatio,
                0.1f,
                100.0f);
        }
        
        public void SetAspectRatio(float aspectRatio)
        {
            _aspectRatio = aspectRatio;
        }

        public void HandleMouseInput(float deltaX, float deltaY)
        {
            Yaw += deltaX * 0.1f;
            Pitch -= deltaY * 0.1f;
            Pitch = MathHelper.Clamp(Pitch, -89f, 89f);
            UpdateVectors();
        }
        
        public struct Ray
        {
            public Vector3 Origin;
            public Vector3 Direction;

            public Ray(Vector3 origin, Vector3 direction)
            {
                Origin = origin;
                Direction = direction.Normalized();
            }

            public bool Intersects(Plane plane, out float distance)
            {
                float denom = Vector3.Dot(plane.Normal, Direction);
                if (Math.Abs(denom) > float.Epsilon)
                {
                    Vector3 p0l0 = plane.Normal * plane.D - Origin;
                    distance = Vector3.Dot(p0l0, plane.Normal) / denom;
                    return distance >= 0;
                }

                distance = 0;
                return false;
            }
        }
        
        public struct Plane
        {
            public Vector3 Normal;
            public float D;

            public Plane(Vector3 normal, float d)
            {
                Normal = normal.Normalized();
                D = d;
            }
        }

        public Ray GetPickingRay(Vector2 mousePosition, float viewportWidth, float viewportHeight)
        {
            Vector4 viewport = new Vector4(0, 0, viewportWidth, viewportHeight);
            Vector3 nearPoint = UnProject(new Vector3(mousePosition.X, viewportHeight - mousePosition.Y, 0), viewport);
            Vector3 farPoint = UnProject(new Vector3(mousePosition.X, viewportHeight - mousePosition.Y, 1), viewport);

            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);
            return new Ray(nearPoint, direction);
        }

        private Vector3 UnProject(Vector3 source, Vector4 viewport)
        {
            Vector4 vec = new Vector4(
                (source.X - viewport.X) / viewport.Z * 2.0f - 1.0f,
                (source.Y - viewport.Y) / viewport.W * 2.0f - 1.0f,
                2.0f * source.Z - 1.0f,
                1.0f
            );

            Matrix4 viewProjectionInverse = (GetProjectionMatrix() * GetViewMatrix()).Inverted();
            vec = viewProjectionInverse * vec;
            vec /= vec.W;

            return new Vector3(vec.X, vec.Y, vec.Z);
        }
    }
}