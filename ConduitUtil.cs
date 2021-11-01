using ConduitLib.APIs;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;

namespace ConduitLib
{
    public static class ConduitUtil
    {
        /// <summary> Left, Up, Right, Down </summary>
        public static Point16[] Dirs => new Point16[4] { new Point16(-1, 0), new Point16(0, -1), new Point16(1, 0), new Point16(0, 1) };

        public static bool PlaceConduit(ModConduit conduit)
        {
            return PlaceConduit(conduit.Position.X, conduit.Position.Y, conduit.GetType(), out _);
        }

        public static bool PlaceConduit(int i, int j, Type conduitType, out ModConduit conduit)
        {
            ref var list = ref ConduitWorld.Conduits[i, j];
            if (list is null)
                list = new();

            if (!ContaisConduit(i, j, conduitType))
            {
                conduit = (ModConduit)Activator.CreateInstance(conduitType);
                conduit.Position = new Point16(i, j);
                conduit.Direction = new bool[] { true, true, true, true };
                list.Add(conduit);
                if (TryGetConduit(i - 1, j, conduitType, out var conduitL))
                    conduitL.Direction[2] = true;
                if (TryGetConduit(i, j - 1, conduitType, out var conduitU))
                    conduitU.Direction[3] = true;
                if (TryGetConduit(i + 1, j, conduitType, out var conduitR))
                    conduitR.Direction[0] = true;
                if (TryGetConduit(i, j + 1, conduitType, out var conduitD))
                    conduitD.Direction[1] = true;
                conduit.IsConnector = conduit.ValidForConnector();
                UpdateNetwork(i, j, conduitType);
                conduit.OnPlace();
                return true;
            }
            conduit = default;
            return false;
        }

        public static bool RemoveConduit(int i, int j, Type conduitType)
        {
            ref var list = ref ConduitWorld.Conduits[i, j];
            if (list is null)
                return false;

            int removeCount = list.RemoveAll(c =>
            {
                if (c.GetType() == conduitType)
                {
                    c.IsConnector = false;
                    c.OnRemove();
                    return true;
                }
                return false;
            });

            if (removeCount > 0)
            {
                UpdateNetworkNearby(i, j, conduitType);
                return true;
            }

            return false;
        }

        public static bool ContaisConduit(int i, int j, Type conduitType)
        {
            ref var list = ref ConduitWorld.Conduits[i, j];
            return list is not null && list.Any(c => c.GetType() == conduitType);
        }

        public static bool ContaisConduit(int i, int j)
        {
            ref var list = ref ConduitWorld.Conduits[i, j];
            return list is not null && list.Count > 0;
        }

        public static bool TryGetConduit<T>(int i, int j, out T conduit) where T : ModConduit
        {
            var flag = TryGetConduit(i, j, typeof(T), out var c);
            conduit = flag ? (T)c : default;
            return flag;
        }

        public static bool TryGetConduit(int i, int j, Type conduitType, out ModConduit conduit)
        {
            conduit = ConduitWorld.Conduits[i, j]?.SingleOrDefault(t => t.GetType() == conduitType);
            return conduit is not null;
        }

        public static bool TryGetConduit(int i, int j, out List<ModConduit> conduit)
        {
            conduit = ConduitWorld.Conduits[i, j];
            return conduit is not null && conduit.Count > 0;
        }

        public static void UpdateNetworkNearby(int i, int j)
        {
            foreach (var dir in Dirs)
                UpdateNetwork(dir.X + i, dir.Y + j);
        }

        public static void UpdateNetwork(int i, int j)
        {
            if (TryGetConduit(i, j, out var conduits))
                foreach (var conduit in conduits)
                    UpdateNetwork(i, j, conduit.GetType());
        }

        public static void UpdateNetworkNearby(int i, int j, Type conduitType)
        {
            foreach (var dir in Dirs)
                UpdateNetwork(dir.X + i, dir.Y + j, conduitType);
        }

        public static void UpdateNetwork(int i, int j, Type conduitType)
        {
            if (!ContaisConduit(i, j, conduitType))
                return;

            var conduits = new List<ModConduit>();
            FindNetwork(new Point16(i, j), ref conduits, new HashSet<Point16>(), conduitType);

            foreach (var conduit in conduits)
                conduit.Network = conduits.Count > 1 ? conduits : null;
        }

        public static void FindNetwork(Point16 start, ref List<ModConduit> conduits, HashSet<Point16> check, Type conduitType)
        {
            check.Add(start);

            if (TryGetConduit(start.X, start.Y, conduitType, out var conduit))
            {
                if (conduit.IsConnector)
                    conduits.Add(conduit);

                for (int i = 0; i < 4; i++)
                {
                    var pos = start + Dirs[i];

                    if (!check.Contains(pos) && HasConnection(conduit, i))
                        FindNetwork(pos, ref conduits, check, conduitType);
                }
            }
        }



        public static bool HasConnection(ModConduit conduit, int side)
        {
            var pos = conduit.Position + Dirs[side];
            return TryGetConduit(pos.X, pos.Y, conduit.GetType(), out var conduit2) && conduit.Direction[side] && conduit2.Direction[(side + 2) % 4];
        }

        public static void SetConnection(ModConduit conduit, int side, bool value)
        {
            var pos = conduit.Position + Dirs[side];
            if (TryGetConduit(pos.X, pos.Y, conduit.GetType(), out var conduit2))
                conduit.Direction[side] = conduit2.Direction[(side + 2) % 4] = value;
            UpdateNetwork(conduit.Position.X, conduit.Position.Y);
            UpdateNetwork(pos.X, pos.Y);
        }

        public static void DrawConduit(ModConduit conduit, SpriteBatch spriteBatch, float? alpha)
        {
            int frameX = 0;
            int frameY = 0;

            if (HasConnection(conduit, 0)) //Left
                frameX += 18;
            if (HasConnection(conduit, 1)) //Up
                frameY += 18;
            if (HasConnection(conduit, 2)) //Right
                frameX += 36;
            if (HasConnection(conduit, 3)) //Down
                frameY += 36;

            conduit.OnDraw(spriteBatch, ref frameX, ref frameY, ref alpha);
        }

        
    }
}
