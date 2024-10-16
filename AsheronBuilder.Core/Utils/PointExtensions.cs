// AsheronBuilder.UI/Utils/PointExtensions.cs

using System.Drawing;
using OpenTK.Mathematics;
using System.Windows.Input;

namespace AsheronBuilder.UI.Utils
{
    public static class PointExtensions
    {
        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
    }
}