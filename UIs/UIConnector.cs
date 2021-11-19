using ConduitLib.APIs;
using ConduitLib.UIs.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.ID;
using Terraria.Localization;
using ItemConduits.ConduitLib.UIs.Elements;
using ItemConduits.ConduitLib.APIs;

namespace ConduitLib.UIs
{
    public class UIConnector : UIState
    {
        public List<ModConduit> Conduits { get; set; }
        public UIPanel Panel { get; set; }
        public UITab[] Tabs { get; set; }
        public UIElement[] InFiltes { get; set; }
        public UIToggle InWhitelist { get; set; }
        public UIElement[] OutFiltes { get; set; }
        public UIToggle OutWhitelist { get; set; }
        public ModConduit Current { get; set; }

        protected StyleDimension TopDim;

        public UIConnector(params ModConduit[] conduits)
        {
            Conduits = conduits.ToList();
            Tabs = new UITab[conduits.Length];
            Current = conduits[0];
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var player = Main.LocalPlayer;

            var rect = this.GetDimensions().ToRectangle();
            if (!Main.recBigList && rect.Contains(Main.mouseX, Main.mouseY))
            {
                player.mouseInterface = true;
            }

            if (Main.recBigList)
                Main.trashSlotOffset = default;
            else
                Main.trashSlotOffset = new Point16(5, 168);

            if (player.DeadOrGhost
                || !Main.playerInventory
                || player.chest > -1
                || player.talkNPC > -1
                || player.sign > -1
                || !player.IsInTileInteractionRange(Conduits[0].Position.X, Conduits[0].Position.Y))
                Close();
        }

        public override void OnInitialize()
        {
            Left.Set(73f, 0);
            Top.Set(Main.instance.invBottom, 0);
            Width.Set(419f, 0);
            Height.Set(165f, 0);

            Panel = new()
            {
                Width = Width,
                Height = Height
            };
            Append(Panel);

            for (int i = 0; i < Tabs.Length; i++)
            {
                Append(Tabs[i] = new UITab(i, i == 0)
                {
                    Top = new(3f, 1f),
                    Left = new(40f * i, 0),
                    Width = new(40f, 0),
                    Height = new(40f, 0),
                    IconTexture = Conduits[i].WrenchIcon,
                    OnToggle = TabToggle,
                });
            }

            var verticalLine = new UIVerticalSeparator();
            verticalLine.Left.Set(0, 0.5f);
            verticalLine.Top.Set(0, 0f);
            verticalLine.Height.Set(0, 1f);
            verticalLine.Width.Set(2f, 0);
            verticalLine.Color = Color.Black;
            Panel.Append(verticalLine);

            var input = new UIToggle(ConduitAsset.Button[1], ConduitAsset.Button[2], Current.Input, Language.GetTextValue("Mods.ConduitLib.UI.Input")) { Outline = ConduitAsset.Button[0] };
            input.Description = () => Current.Input ? Language.GetTextValue("Mods.ConduitLib.UI.InputAllowed") : Language.GetTextValue("Mods.ConduitLib.UI.InputDisallowed");
            input.OnToggle = (toggle) => Current.Input = toggle;
            Panel.Append(input);

            var rightDim = new StyleDimension(Panel.PaddingLeft, 0.5f);
            TopDim = new StyleDimension(22 + Panel.PaddingTop, 0);
            var output = new UIToggle(ConduitAsset.Button[1], ConduitAsset.Button[2], Current.Output, Language.GetTextValue("Mods.ConduitLib.UI.Output")) { Outline = ConduitAsset.Button[0] };
            output.Left = rightDim;
            output.Description = () => Current.Output ? Language.GetTextValue("Mods.ConduitLib.UI.OutputAllowed") : Language.GetTextValue("Mods.ConduitLib.UI.OutputDisallowed");
            output.OnToggle = (toggle) => Current.Output = toggle;
            Panel.Append(output);

            Current.OnInitializeUI(Panel, rightDim, ref TopDim);

            if (Current.UseFilters)
            {
                InFiltes = Array.Empty<UIElement>();
                var inFilterSlot = new UIFilterSlot(() => Current.InputFilter, (item) => Current.InputFilter = item);
                inFilterSlot.OnSlotClick += (e, item) => UpdateInFilters(item);
                inFilterSlot.Top = TopDim;
                Panel.Append(inFilterSlot);
                InWhitelist = new UIToggle(ConduitAsset.Button[3], ConduitAsset.Button[4], false) { Outline = ConduitAsset.Button[0] };
                InWhitelist.Top = TopDim;
                InWhitelist.Top.Pixels += 2;
                InWhitelist.Left.Pixels += 26 + 2;
                InWhitelist.Description = () => InWhitelist.Toggle ? Language.GetTextValue("Mods.ConduitLib.UI.Whitelist") : Language.GetTextValue("Mods.ConduitLib.UI.Blacklist");
                UpdateInFilters(Current.InputFilter);

                OutFiltes = Array.Empty<UIElement>();
                var outFilterSlot = new UIFilterSlot(() => Current.OutputFilter, (item) => Current.OutputFilter = item);
                outFilterSlot.OnSlotClick += (e, item) => UpdateOutFilters(item);
                outFilterSlot.Top = TopDim;
                outFilterSlot.Left = rightDim;
                Panel.Append(outFilterSlot);
                OutWhitelist = new UIToggle(ConduitAsset.Button[3], ConduitAsset.Button[4], false) { Outline = ConduitAsset.Button[0] };
                OutWhitelist.Top = TopDim;
                OutWhitelist.Top.Pixels += 2;
                OutWhitelist.Left = rightDim;
                OutWhitelist.Left.Pixels += 26 + 2;
                OutWhitelist.Description = () => OutWhitelist.Toggle ? Language.GetTextValue("Mods.ConduitLib.UI.Whitelist") : Language.GetTextValue("Mods.ConduitLib.UI.Blacklist");
                UpdateOutFilters(Current.OutputFilter);
            }
        }

        void UpdateInFilters(Item item)
        {
            for (int i = 0; i < InFiltes.Length; i++)
                InFiltes[i].Remove();
            InWhitelist.Remove();

            if (item?.ModItem is IFilter filter)
            {
                InFiltes = new UIElement[filter.FiltersCount];

                InWhitelist.Toggle = Current.GetFilter(true).IsWhitelist;
                InWhitelist.OnToggle = (toggle) => Current.GetFilter(true).IsWhitelist = toggle;
                Panel.Append(InWhitelist);

                var rightDim = new StyleDimension();
                for (int i = 0; i < InFiltes.Length; i++)
                {
                    InFiltes[i] = new UIElement()
                    {
                        Top = TopDim,
                        Left = rightDim,
                        Width = new StyleDimension(26, 0),
                        Height = new StyleDimension(26, 0),
                    };
                    InFiltes[i].Top.Pixels += 26 + 2;
                    rightDim.Pixels += 26 + 2;
                    InFiltes[i].SetPadding(0f);
                    Current.OnInitializeUIFilters(i, true, InFiltes[i]);
                    Panel.Append(InFiltes[i]);
                }
            }
        }

        void UpdateOutFilters(Item item)
        {
            for (int i = 0; i < OutFiltes.Length; i++)
                OutFiltes[i].Remove();
            OutWhitelist.Remove();

            if (item?.ModItem is IFilter filter)
            {
                OutFiltes = new UIElement[filter.FiltersCount];

                OutWhitelist.Toggle = Current.GetFilter(false).IsWhitelist;
                OutWhitelist.OnToggle = (toggle) => Current.GetFilter(false).IsWhitelist = toggle;
                Panel.Append(OutWhitelist);

                var rightDim = new StyleDimension(Panel.PaddingLeft, 0.5f);
                for (int i = 0; i < OutFiltes.Length; i++)
                {
                    OutFiltes[i] = new UIElement()
                    {
                        Top = TopDim,
                        Left = rightDim,
                        Width = new StyleDimension(26, 0),
                        Height = new StyleDimension(26, 0),
                    };
                    OutFiltes[i].Top.Pixels += 26 + 2;
                    rightDim.Pixels += 26 + 2;
                    OutFiltes[i].SetPadding(0f);
                    Current.OnInitializeUIFilters(i, false, OutFiltes[i]);
                    Panel.Append(OutFiltes[i]);
                }
            }
        }

        void TabToggle(int id, bool toggle)
        {
            Current = Conduits[id];
            for (int i = 0; i < Tabs.Length; i++)
                Tabs[i].Toggle = id == i;
        }

        public override void OnActivate()
        {
            Main.trashSlotOffset = new Point16(5, 168);
            Main.recBigList = false;
            Main.LocalPlayer.chest = -1;
            Main.LocalPlayer.sign = -1;
            Main.LocalPlayer.SetTalkNPC(-1);
            Main.playerInventory = true;
        }

        public override void OnDeactivate() =>
            Main.trashSlotOffset = default;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Main.recBigList)
            {
                base.Draw(spriteBatch);

                var element = Main.hasFocus ? GetElementAt(Main.MouseScreen) : null;
                string text = (element as IUIDescription)?.Description?.Invoke() ?? null;
                if (text is not null)
                {
                    var textSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, Vector2.One);
                    Utils.DrawInvBG(spriteBatch, Main.mouseX + 20, Main.mouseY + 20, textSize.X + 15, textSize.Y + 15, new Color(23, 25, 81, 255) * 0.925f);
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(Main.mouseX + 30, Main.mouseY + 30), Color.White, 0f, Vector2.Zero, Vector2.One);
                }
            }
        }

        public static void Open(int i, int j, params Type[] conduitTypes)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            var list = new List<ModConduit>();
            foreach (var conduitType in conduitTypes)
                if (ConduitUtil.TryGetConduit(i, j, conduitType, out var conduit) && conduit.IsConnector)
                    list.Add(conduit);

            if (list.Count > 0 && Main.LocalPlayer.IsInTileInteractionRange(i, j))
                ConduitUI.UI.SetState(new UIConnector(list.ToArray()));
        }

        public static void Close()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            ConduitUI.UI.SetState(null);
        }
    }
}
