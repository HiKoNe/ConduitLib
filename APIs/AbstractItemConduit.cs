using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConduitLib.APIs
{
    public abstract class AbstractItemConduit<T> : ModItem where T : ModConduit
    {
        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.useTime = 5;
            Item.useAnimation = 15;
            Item.tileBoost = 5;

            Item.useStyle = ItemUseStyleID.Swing;

            Item.useTurn = true;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.noMelee = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer && Main.mouseLeft && player.IsTargetTileInItemRange(Item))
                return ConduitUtil.PlaceConduit(Player.tileTargetX, Player.tileTargetY, typeof(T), out _);

            return false;
        }

        public override void HoldItem(Player player)
        {
            ConduitWorld.ConduitsVisibilities[typeof(T)] = 1f;
            if (player.whoAmI == Main.myPlayer && player.IsTargetTileInItemRange(Item))
            {
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = Type;
            }
        }
    }
}
