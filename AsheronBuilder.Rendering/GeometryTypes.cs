// AsheronBuilder.Rendering/GeometryTypes.cs

using System;
using OpenTK.Mathematics;

namespace AsheronBuilder.Rendering
{
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
}