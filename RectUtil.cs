using Microsoft.Xna.Framework;
using Terraria;

namespace ConduitLib
{
    internal static class RectUtil
    {
        public static Rectangle TopLeft(this Rectangle rect, int x, int y, int width, int height)
        {
            var pos = rect.TopLeft();
            return new Rectangle((int)pos.X + x, (int)pos.Y + y, width, height);
        }

        public static Rectangle TopRight(this Rectangle rect, int x, int y, int width, int height)
        {
            var pos = rect.TopRight();
            return new Rectangle((int)pos.X + x, (int)pos.Y + y, width, height);
        }

        public static Rectangle BottomRight(this Rectangle rect, int x, int y, int width, int height)
        {
            var pos = rect.BottomRight();
            return new Rectangle((int)pos.X + x, (int)pos.Y + y, width, height);
        }

        public static Rectangle BottomLeft(this Rectangle rect, int x, int y, int width, int height)
        {
            var pos = rect.BottomLeft();
            return new Rectangle((int)pos.X + x, (int)pos.Y + y, width, height);
        }
    }
}
