using ConduitLib.APIs;
using ConduitLib.Test.APIs;
using ConduitLib.UIs.Elements;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace ConduitLib.Test
{
    public class ItemConduit : ModConduit
    {
        protected int cururrentRoundRobin;
        private int priority;

        public IItemContainer ItemContainer { get; set; }
        public bool RoundRobin { get; set; }
        public int Priority { get => priority; set { priority = value; UpdateNetwork(); } }
        public bool WireMode { get; set; } = true;
        public int MaxTransfer => 4;
        public override List<ModConduit> Network
        {
            get => base.Network; set
            {
                base.Network = value;

                if (network is not null)
                    network.Sort((c, c2) => ((ItemConduit)c2).Priority.CompareTo(((ItemConduit)c).Priority));
            }
        }

        public override int? UpdateDelay => WireMode ? null : 60;
        public override Asset<Texture2D> Texture => ConduitAsset.Test[0];
        public override Asset<Texture2D> WrenchIcon => TextureAssets.Item[ModContent.ItemType<ItemItemConduit>()];

        public override bool ValidForConnector()
        {
            if (ItemContainerUtil.TryGetItemContainer(Position.X, Position.Y, out var itemContainer))
            {
                this.ItemContainer = itemContainer;
                return true;
            }
            this.ItemContainer = null;
            return false;
        }

        public override void OnUpdate()
        {
            if (Output && Network is not null && ItemContainer is not null && !ItemContainer.IsEmpty())
            {
                for (int i = 0; i < ItemContainer.ContainerSize; i++)
                {
                    var item = ItemContainer[i];
                    if (item.stack < 1)
                        continue;

                    int toTransfer = item.stack = Math.Min(item.stack, MaxTransfer);

                    for (int c = 0; c < Network.Count; c++)
                    {
                        var itemConduit = (ItemConduit)Network[c];

                        if (RoundRobin)
                        {
                            if (cururrentRoundRobin > c)
                                continue;

                            cururrentRoundRobin = c + 1;

                            if (cururrentRoundRobin >= Network.Count)
                                cururrentRoundRobin = 0;
                        }

                        item = itemConduit.ItemContainer.AddItem(item);

                        if (item.stack < 1)
                            break;
                    }

                    if (ItemContainer.DecreaseItem(i, toTransfer - item.stack).stack == 0)
                        continue;
                    break;
                }
            }

            base.OnUpdate();
        }

        public override void OnPlace()
        {
            SoundEngine.PlaySound(SoundID.Dig, Position.ToVector2() * 16);
            base.OnPlace();
        }

        public override void OnRemove()
        {
            var pos = Position.ToVector2() * 16;
            SoundEngine.PlaySound(SoundID.Dig, pos);
            Item.NewItem(pos, ModContent.ItemType<ItemItemConduit>(), 1, false, 0, true);
            base.OnRemove();
        }

        public override void OnDraw(in SpriteBatch spriteBatch, ref int frameX, ref int frameY, ref float? alpha)
        {
            if (IsConnector)
            {
                frameX = 72;
                frameY = 0;
            }
            base.OnDraw(spriteBatch, ref frameX, ref frameY, ref alpha);
        }

        public override void OnInitializeUI(ref UIPanel panel, StyleDimension rightDim, StyleDimension topDim)
        {
            var priority = new UIValue(Priority, "Priority")
            {
                Top = topDim,
                OnChange = (value) => Priority = (int)value,
            };
            panel.Append(priority);

            var roundRobin = new UIToggle(ConduitAsset.Button[3], ConduitAsset.Button[4], RoundRobin)
            {
                Outline = ConduitAsset.Button[0],
                Top = topDim,
                Left = rightDim,
                Description = () => "Round-Robin: " + (RoundRobin ? "Enabled" : "Disabled"),
                OnToggle = (toggle) => RoundRobin = toggle,
            };
            panel.Append(roundRobin);

            var wireMode = new UIToggle(ConduitAsset.Button[5], ConduitAsset.Button[6], WireMode)
            {
                Outline = ConduitAsset.Button[0],
                Top = topDim,
                Left = rightDim,
                Description = () => "Wire Mode: " + (WireMode ? "Enabled" : "Disabled") + (WireMode ? "\nActive by wire hit" : "\nActive every second"),
                OnToggle = (toggle) => WireMode = toggle,
            };
            wireMode.Left.Pixels += roundRobin.Width.Pixels + panel.PaddingLeft;
            panel.Append(wireMode);

            base.OnInitializeUI(ref panel, rightDim, topDim);
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag["roundRobin"] = RoundRobin;
            tag["priority"] = Priority;
            tag["wireMode"] = WireMode;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            RoundRobin = tag.GetBool("roundRobin");
            Priority = tag.GetInt("priority");
            WireMode = tag.GetBool("wireMode");
        }
    }
}
