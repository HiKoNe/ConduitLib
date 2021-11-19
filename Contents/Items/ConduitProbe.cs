using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ConduitLib.Contents.Items
{
    public class ConduitProbe : ModItem
    {
        public enum EProbeMode : byte { Copy = 0, Paste = 1 }

        public static Type ConduitMode { get; set; }
        public static EProbeMode Mode { get; set; }

        public TagCompound SavedConduit { get; set; } = new();
        public string SavedName { get; set; } = "";

        public override void SetStaticDefaults()
        {
            ConduitMode = ConduitLoader.Conduits.FirstOrDefault()?.GetType();
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) =>
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.EverythingElse;

        public override void SetDefaults()
        {
            Item.maxStack = 1;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.tileBoost = 10;

            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(0, 15, 0, 0);
            Item.useStyle = ItemUseStyleID.Swing;

            Item.useTurn = true;
            Item.autoReuse = false;
            Item.consumable = false;
            Item.noMelee = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            if (!player.IsTargetTileInItemRange(Item))
                return true;

            int i = Player.tileTargetX;
            int j = Player.tileTargetY;
            if (player.altFunctionUse != 2)
            {
                var pos = new Vector2(i, j) * 16;

                if (Mode == EProbeMode.Copy)
                {
                    if (ConduitUtil.TryGetConduit(i, j, ConduitMode, out var conduit))
                    {
                        SavedName = conduit.Name;
                        SavedConduit = new();
                        conduit.InternalSaveData(SavedConduit, true);
                        Language.GetTextValue("Mods.ConduitLib.UI.DataCopy").CreatePopup(pos, 45, Color.Green);
                    }
                }
                else
                {
                    if (SavedConduit.Count > 0 && ConduitUtil.TryGetConduit(i, j, ConduitMode, out var conduit))
                    {
                        conduit.InternalLoadData(SavedConduit, true, true);
                        Language.GetTextValue("Mods.ConduitLib.UI.DataPaste").CreatePopup(pos, 45, Color.Yellow);
                    }
                }
            }
            else if (Main.mouseRight && Main.mouseRightRelease)
            {
                ConduitUI.UIProbe.Active ^= true;
                return null;
            }

            return true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SavedConduit", SavedConduit.Count == 0 ? Language.GetTextValue("Mods.ConduitLib.UI.DataStoreEmpty") : Language.GetTextValue("Mods.ConduitLib.UI.DataStore", SavedName)));
        }

        public override void HoldItem(Player player)
        {
            ConduitWorld.ConduitsVisibilities[ConduitMode] = 1f;

            if (player.whoAmI == Main.myPlayer && player.IsTargetTileInItemRange(Item))
            {
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = Type;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["savedConduit"] = SavedConduit;
            tag["savedName"] = SavedName;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            if (tag.ContainsKey("savedConduit"))
                SavedConduit = tag.GetCompound("savedConduit");
            if (tag.ContainsKey("savedName"))
                SavedName = tag.GetString("savedName");
        }

        public override void NetSend(BinaryWriter writer)
        {
            base.NetSend(writer);
            TagIO.Write(SavedConduit, writer);
            writer.Write(SavedName);
        }

        public override void NetReceive(BinaryReader reader)
        {
            base.NetReceive(reader);
            SavedConduit = TagIO.Read(reader);
            SavedName = reader.ReadString();
        }
    }
}
