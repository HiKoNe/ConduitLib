﻿using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace ConduitLib
{
    internal static class ExtensionUtil
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

        public static void Write(this BinaryWriter writer, Type type) =>
            writer.Write(type.FullName);
        public static Type ReadType(this BinaryReader reader) =>
            Type.GetType(reader.ReadString());

        public static void Write(this BinaryWriter writer, TagCompound tag) =>
            TagIO.Write(tag, writer);
        public static TagCompound ReadTag(this BinaryReader reader) =>
            TagIO.Read(reader);
    }
}