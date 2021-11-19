using ConduitLib.UIs;
using ItemConduits.ConduitLib.APIs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.UI;

namespace ItemConduits.ConduitLib.UIs.Elements
{
    public class UIFilterSlot : UIElement, IUIDescription
    {
        public delegate void onSlotClick(UIFilterSlot element, Item item);

        public Action<Item> ItemSet { get; set; }
        public Func<Item> ItemGet { get; set; }
        public Func<string> Description { get; set; }

        public event onSlotClick OnSlotClick;

        public UIFilterSlot(Func<Item> itemGet, Action<Item> itemSet)
        {
            ItemSet = itemSet;
            ItemGet = itemGet;
            Description = () => ItemGet().IsAir && Main.mouseItem.IsAir ? Language.GetTextValue("Mods.ConduitLib.UI.FilterSlot") : null;
            Width.Set(26, 0);
            Height.Set(26, 0);
        }

        public override void Click(UIMouseEvent evt)
        {
            base.Click(evt);

            Main.mouseLeft = Main.mouseLeftRelease = true;

            if (Main.mouseItem.IsAir || Main.mouseItem?.ModItem is IFilter)
            {
                var item = ItemGet();
                ItemSlot.LeftClick(ref item);
                ItemSet(item);
                OnSlotClick(this, item);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            var item = ItemGet();

            if (IsMouseHovering)
                ItemSlot.MouseHover(ref item);

            var oldScale = Main.inventoryScale;
            Main.inventoryScale = 0.5f;
            ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.ChestItem, GetDimensions().Position(), Color.White);
            Main.inventoryScale = oldScale;
        }
    }
}
