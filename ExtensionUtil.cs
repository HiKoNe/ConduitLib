using ItemConduits.ConduitLib.APIs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace ConduitLib
{
    public static class ExtensionUtil
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

        public static void Write(this BinaryWriter writer, Type type)
        {
            writer.Write(type.FullName);
            writer.Write(type.Assembly.FullName);
        }

        public static Type ReadType(this BinaryReader reader)
        {
            var type = reader.ReadString();
            var ass = reader.ReadString();
            var ConduitLibAss = AppDomain.CurrentDomain.GetAssemblies().Last(a => a.FullName == ass);
            return ConduitLibAss.GetType(type);
        }

        public static void Write(this BinaryWriter writer, TagCompound tag) =>
            TagIO.Write(tag, writer);
        public static TagCompound ReadTag(this BinaryReader reader) =>
            TagIO.Read(reader);

        public static bool AnyConditions(this IFilter filter, object obj)
        {
            for (int f = 0; f < filter.FiltersCount; f++)
                if (filter.Condition(f, obj))
                    return true;
            return false;
        }

        public static int CreatePopup(this string str, Vector2 pos, int lifeTime, Color color)
        {
            if (!Main.showItemText || Main.netMode == 2)
                return -1;

            var p = FindNextItemTextSlot();
            if (p > -1)
            {
                var value = FontAssets.MouseText.Value.MeasureString(str);
                ref var popupText = ref Main.popupText[p];
                popupText.alpha = 1f;
                popupText.alphaDir = -1;
                popupText.active = true;
                popupText.scale = 1f;
                popupText.NoStack = true;
                popupText.rotation = 0f;
                popupText.position = pos - value / 2f;
                popupText.expert = false;
                popupText.master = false;
                popupText.name = str;
                popupText.stack = 1;
                popupText.velocity.Y = -7f;
                popupText.lifeTime = lifeTime;
                popupText.context = PopupTextContext.RegularItemPickup;
                popupText.coinValue = 0;
                popupText.coinText = false;
                popupText.color = color;
            }

            return p;
        }

        private static int FindNextItemTextSlot()
        {
            int num = -1;
            for (int i = 0; i < 20; i++)
            {
                if (!Main.popupText[i].active)
                {
                    num = i;
                    break;
                }
            }

            if (num == -1)
            {
                double num2 = Main.bottomWorld;
                for (int j = 0; j < 20; j++)
                {
                    if (num2 > Main.popupText[j].position.Y)
                    {
                        num = j;
                        num2 = Main.popupText[j].position.Y;
                    }
                }
            }

            return num;
        }
    }
}
