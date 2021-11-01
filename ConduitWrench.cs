using ConduitLib.UIs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConduitLib
{
    public class ConduitWrench : ModItem
    {
        public enum EWrenchMode : byte { Demount = 0, Config = 1, Setting = 2 }

        public static List<Type> ConduitMode { get; set; }
        public static EWrenchMode Mode { get; set; }

        public override string Texture => "ConduitLib/Assets/Wrench";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Conduit Wrench");
            Tooltip.SetDefault("Conduit manipulator.");

            ConduitMode = new();
            foreach (var modConduit in ConduitLoader.Conduits)
                ConduitMode.Add(modConduit.GetType());
            Mode = EWrenchMode.Demount;
        }

        public override void SetDefaults()
        {
            Item.maxStack = 1;
            Item.useTime = 5;
            Item.useAnimation = 15;
            Item.tileBoost = 10;

            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.useTurn = true;
            Item.autoReuse = true;
            Item.consumable = false;
            Item.noMelee = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            if (player.altFunctionUse != 2)
            {
                if (!player.IsTargetTileInItemRange(Item))
                    return true;

                int i = Player.tileTargetX;
                int j = Player.tileTargetY;

                if (Mode == EWrenchMode.Demount)
                {
                    if (Main.mouseLeft)
                    {
                        foreach (var conduitType in ConduitMode)
                            ConduitUtil.RemoveConduit(i, j, conduitType);
                    }
                }
                else if (Mode == EWrenchMode.Config)
                {
                    foreach (var conduitType in ConduitMode)
                    {
                        if (!ConduitUtil.TryGetConduit(i, j, conduitType, out var conduit))
                            continue;

                        var tilePos = new Vector2(i, j) * 16 - Main.screenPosition;
                        var mousePos = Main.MouseScreen;
                        var dirPos = mousePos - tilePos;

                        int side;
                        bool flag = dirPos.X + dirPos.Y > 16;
                        if (dirPos.X > dirPos.Y)
                        {
                            side = 1;
                            if (flag)
                                side = 2;
                        }
                        else
                        {
                            side = 0;
                            if (flag)
                                side = 3;
                        }

                        ConduitUtil.SetConnection(conduit, side, !conduit.Direction[side]);
                    }
                }
                else
                {
                    UIConnector.Open(i, j, ConduitMode.ToArray());
                }
            }
            else if (Main.mouseRight && Main.mouseRightRelease)
            {
                ConduitUI.UIWrench.Active = !ConduitUI.UIWrench.Active;
                return null;
            }

            return true;
        }

        public override float UseTimeMultiplier(Player player)
        {
            if ((player.whoAmI == Main.myPlayer && player.altFunctionUse == 2) || Mode != EWrenchMode.Demount)
                return 3f;

            return base.UseTimeMultiplier(player);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            foreach (var conduitType in ConduitMode)
                ConduitWorld.ConduitsVisibilities[conduitType] = 1f;

            if (player.whoAmI == Main.myPlayer && player.IsTargetTileInItemRange(Item))
            {
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = Type;
            }
        }
    }
}
