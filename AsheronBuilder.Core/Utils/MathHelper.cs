// File: AsheronBuilder.Core/Utils/MathHelper.cs

using System;

namespace AsheronBuilder.Core.Utils
{
    public static class MathHelper
    {
        public static float DegreesToRadians(float degrees)
        {
            return degrees * (float)(Math.PI / 180.0);
        }

        public static float RadiansToDegrees(float radians)
        {
            return radians * (float)(180.0 / Math.PI);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}