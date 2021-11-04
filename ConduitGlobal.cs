using ConduitLib.Test;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ConduitLib
{
    public class ConduitGlobal : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);

            if (Main.netMode == 1)
                return;

            //Fix Vanilla Food Platter
            if (type == TileID.FoodPlatter && !fail)
            {
                foreach (var item in TileEntity.ByID.Where(te => te.Value is TEFoodPlatter && te.Value.Position.X == i && te.Value.Position.Y == j))
                    TileEntity.ByID.Remove(item.Key);
                foreach (var item in TileEntity.ByPosition.Where(te => te.Value is TEFoodPlatter && te.Value.Position.X == i && te.Value.Position.Y == j))
                    TileEntity.ByPosition.Remove(item.Key);
            }

            if (ConduitWorld.Conduits[i, j] is not null)
                foreach (var conduit in ConduitWorld.Conduits[i, j])
                    ConduitWorld.ConduitsToCheck.Add(conduit);
        }

        public override void PlaceInWorld(int i, int j, int type, Item item)
        {
            base.PlaceInWorld(i, j, type, item);

            if (Main.netMode == 1)
            {
                ConduitNet.SendPacket(PacketID.PlaceInWorld, -1, i, j, type);
                return;
            }

            PlaceInWorld(i, j, type);
        }

        public static void PlaceInWorld(int i, int j, int type)
        {
            int style = 0;
            int alternate = 0;
            var baseCoords = new Point16(i, j);
            TileObjectData.GetTileInfo(Main.tile[i, j], ref style, ref alternate);
            TileObjectData.OriginToTopLeft(type, style, ref baseCoords);

            var tileData = TileObjectData.GetTileData(type, style, alternate);
            var size = tileData is not null ? new Point16(tileData.Width, tileData.Height) : new();

            for (int x = baseCoords.X; x < baseCoords.X + size.X; x++)
                for (int y = baseCoords.Y; y < baseCoords.Y + size.Y; y++)
                    if (ConduitWorld.Conduits[x, y] is not null)
                        foreach (var conduit in ConduitWorld.Conduits[x, y])
                            ConduitWorld.ConduitsToCheck.Add(conduit);
        }

        public override bool PreHitWire(int i, int j, int type)
        {
            bool flag = base.PreHitWire(i, j, type);
            if (Main.netMode == 1)
                return flag;

            if (ConduitUtil.TryGetConduit<ItemConduit>(i, j, out var conduit)
                && conduit.WireMode && conduit.IsConnector && conduit.Output)
                conduit.OnUpdate();

            return flag;
        }
    }
}