using ConduitLib.APIs;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ConduitLib
{
    public class ConduitWorld : ModSystem
    {
        public const string TAG_CONDUITS = "conduits";
        public const string TAG_CONNECTORS = "connectors";

        public static List<ModConduit>[,] Conduits { get; private set; }
        public static List<ModConduit> ConduitsToCheck { get; private set; } = new();
        public static List<ModConduit> ConduitsToUpdate { get; private set; } = new();
        public static Dictionary<Type, float?> ConduitsVisibilities { get; private set; } = new();

        public override void Unload()
        {
            Conduits = null;
            ConduitsToCheck.Clear();
            ConduitsToUpdate.Clear();
            ConduitsVisibilities.Clear();
        }

        public override void OnWorldLoad()
        {
            Conduits = new List<ModConduit>[Main.maxTilesX, Main.maxTilesY];
            ConduitsToCheck.Clear();
            ConduitsToUpdate.Clear();
        }
        public override void PreWorldGen()
        {
            OnWorldLoad();
        }
        public override void PostUpdateWorld()
        {
            foreach (var conduit in ConduitsToCheck)
                conduit.IsConnector = conduit.ValidForConnector();
            ConduitsToCheck.Clear();

            foreach (var conduit in ConduitsToUpdate)
                if (conduit.IsConnector)
                    if (conduit.UpdateDelay.HasValue && ++conduit.updateDelay >= conduit.UpdateDelay)
                    {
                        conduit.OnUpdate();
                        conduit.updateDelay = 0;
                    }
        }
        public override void PostDrawTiles()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.TransformationMatrix);
            DrawConduits(true);
            Main.spriteBatch.End();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var dict = new Dictionary<Type, List<TagCompound>>();
            for (short i = 0; i < Main.maxTilesX; i++)
            {
                for (short j = 0; j < Main.maxTilesY; j++)
                {
                    if (Conduits[i, j] == null)
                        continue;

                    foreach (var conduit in Conduits[i, j])
                    {
                        var conduitType = conduit.GetType();
                        dict.TryAdd(conduitType, new List<TagCompound>());

                        var conduitTag = new TagCompound();
                        conduit.InternalSaveData(conduitTag);
                        dict[conduitType].Add(conduitTag);
                    }
                }
            }

            foreach (var modConduit in ConduitLoader.Conduits)
            {
                var modConduitTag = new TagCompound();
                if (dict.TryGetValue(modConduit.GetType(), out var list))
                    modConduitTag[TAG_CONDUITS] = list;
                tag[modConduit.FullName] = modConduitTag;
            }
        }
        public override void LoadWorldData(TagCompound tag)
        {
            OnWorldLoad();

            foreach (var modConduit in ConduitLoader.Conduits)
            {
                if (!tag.ContainsKey(modConduit.FullName))
                    continue;

                var modConduitTag = tag.GetCompound(modConduit.FullName);
                if (modConduitTag.ContainsKey(TAG_CONDUITS))
                {
                    foreach (var conduitTag in modConduitTag.GetList<TagCompound>(TAG_CONDUITS))
                    {
                        var conduit = (ModConduit)Activator.CreateInstance(modConduit.GetType());
                        conduit.InternalLoadData(conduitTag);

                        ref var list = ref Conduits[conduit.Position.X, conduit.Position.Y];
                        if (list is null)
                            list = new();
                        list.Add(conduit);
                        ConduitUtil.UpdateNetwork(conduit.Position.X, conduit.Position.Y, conduit.GetType());
                    }
                }
            }
        }

        internal static void DrawConduits(bool postDraw = false)
        {
            if (Conduits == null)
                return;

            int screenX = Math.Max((int)(Main.screenPosition.X / 16f - 1f), 0);
            int screenY = Math.Max((int)(Main.screenPosition.Y / 16f - 1f), 0);
            int screenWidth = Math.Min((int)((Main.screenPosition.X + Main.screenWidth) / 16f) + 2, Main.maxTilesX);
            int screenHeight = Math.Min((int)((Main.screenPosition.Y + Main.screenHeight) / 16f) + 5, Main.maxTilesY);

            var screenOverdrawOffset = Main.GetScreenOverdrawOffset();
            for (int j = screenY + screenOverdrawOffset.Y; j < screenHeight - screenOverdrawOffset.Y; j++)
            {
                for (int i = screenX + screenOverdrawOffset.X; i < screenWidth - screenOverdrawOffset.X; i++)
                {
                    ref var conduits = ref Conduits[i, j];

                    if (conduits is null)
                        continue;

                    foreach (var conduit in conduits)
                    {
                        if (!postDraw)
                            ConduitUtil.DrawConduit(conduit, Main.spriteBatch, null);
                        else if (ConduitsVisibilities.TryGetValue(conduit.GetType(), out var visible))
                                ConduitUtil.DrawConduit(conduit, Main.spriteBatch, visible);
                    }
                }
            }

            if (postDraw)
                ConduitsVisibilities.Clear();
        }
    }
}
