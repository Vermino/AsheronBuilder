// AsheronBuilder.Core/Utils/VectorExtensions.cs

using OpenTK.Mathematics;

namespace AsheronBuilder.Core.Utils
{
    public static class VectorExtensions
    {
        public static Vector3 Min(this Vector3 a, Vector3 b)
        {
            return new Vector3(
                Math.Min(a.X, b.X),
                Math.Min(a.Y, b.Y),
                Math.Min(a.Z, b.Z)
            );
        }

        public static Vector3 Max(this Vector3 a, Vector3 b)
        {
            return new Vector3(
                Math.Max(a.X, b.X),
                Math.Max(a.Y, b.Y),
                Math.Max(a.Z, b.Z)
            );
        }
        
        public static OpenTK.Mathematics.Vector3 ToOpenTK(this System.Numerics.Vector3 v)
        {
            return new OpenTK.Mathematics.Vector3(v.X, v.Y, v.Z);
        }

        public static System.Numerics.Vector3 ToSystemNumerics(this OpenTK.Mathematics.Vector3 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        public static OpenTK.Mathematics.Vector2 ToOpenTK(this System.Numerics.Vector2 v)
        {
            return new OpenTK.Mathematics.Vector2(v.X, v.Y);
        }

        public static System.Numerics.Vector2 ToSystemNumerics(this OpenTK.Mathematics.Vector2 v)
        {
            return new System.Numerics.Vector2(v.X, v.Y);
        }
    }
}