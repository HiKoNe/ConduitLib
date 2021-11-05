using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConduitLib.Contents.Items
{
    public class ConduitShell : ModItem
    {
        public override void SetDefaults()
        {
            Item.maxStack = 999;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 0, 10, 0);
        }
    }
}
