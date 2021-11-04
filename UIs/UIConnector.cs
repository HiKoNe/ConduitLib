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

namespace ConduitLib.UIs
{
    public class UIConnector : UIState
    {
        public List<ModConduit> Conduits { get; set; }
        public UITab[] Tabs { get; set; }
        public ModConduit Current { get; set; }

        public UIConnector(params ModConduit[] conduits)
        {
            this.Conduits = conduits.ToList();
            this.Tabs = new UITab[conduits.Length];
            this.Current = conduits[0];
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
            this.Left.Set(73f, 0);
            this.Top.Set(Main.instance.invBottom, 0);
            this.Width.Set(419f, 0);
            this.Height.Set(165f, 0);

            var panel = new UIPanel();
            panel.Width = this.Width;
            panel.Height = this.Height;
            this.Append(panel);

            for (int i = 0; i < Tabs.Length; i++)
            {
                this.Append(Tabs[i] = new UITab(i, i == 0)
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
            panel.Append(verticalLine);

            var input = new UIToggle(ConduitAsset.Button[1], ConduitAsset.Button[2], Current.Input, "Input") { Outline = ConduitAsset.Button[0] };
            input.Description = () => (Current.Input ? "Allowed" : "Disallowed") + " for insertion";
            input.OnToggle = (toggle) => Current.Input = toggle;
            panel.Append(input);

            var rightDim = new StyleDimension(panel.PaddingLeft, 0.5f);
            var topDim = new StyleDimension(22 + panel.PaddingTop, 0);
            var output = new UIToggle(ConduitAsset.Button[1], ConduitAsset.Button[2], Current.Output, "Output") { Outline = ConduitAsset.Button[0] };
            output.Left = rightDim;
            output.Description = () => (Current.Output ? "Allowed" : "Disallowed") + " for extraction";
            output.OnToggle = (toggle) => Current.Output = toggle;
            panel.Append(output);

            Current.OnInitializeUI(ref panel, rightDim, topDim);
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

                var element = Main.hasFocus ? this.GetElementAt(Main.MouseScreen) : null;
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
