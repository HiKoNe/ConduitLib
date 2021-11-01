using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace ConduitLib.UIs.Elements
{
    public class UITab : UIElement, IUIDescription
    {
        Asset<Texture2D> borderTexture;
        Asset<Texture2D> backgroundTexture;

        int cornerSize;
        int barSize;

        public int ID { get; set; }
        public bool Toggle { get; set; }
        public Asset<Texture2D> IconTexture { get; set; }
        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
        public Color SelectedColor { get; set; }
        public Color OutlineColor { get; set; }

        public delegate void onToggle(int id, bool toggle);
        public onToggle OnToggle { get; set; }
        public Func<string> Description { get; set; }

        public UITab(int id = 0, bool toggle = default)
        {
            this.ID = id;
            this.Toggle = toggle;
            this.BackgroundColor = new Color(63, 82, 151) * 0.7f;
            this.BorderColor = Color.Black;
            this.OutlineColor = Color.Yellow;
            this.SelectedColor = Color.LightYellow * 0.7f;
            this.borderTexture = Main.Assets.Request<Texture2D>("Images/UI/PanelBorder");
            this.backgroundTexture = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
            this.cornerSize = 12;
            this.barSize = 4;
            this.Description = null;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var rect = GetDimensions().ToRectangle();

            Draw(spriteBatch, backgroundTexture.Value, Toggle ? SelectedColor : BackgroundColor);
            Draw(spriteBatch, borderTexture.Value, IsMouseHovering ? OutlineColor : BorderColor);
            if (IconTexture is not null)
            {
                var t = IconTexture.Value;
                spriteBatch.Draw(t, new Vector2(rect.X + rect.Width * 0.5f, rect.Y + rect.Height * 0.5f), null, Color.White, 0f, t.Bounds.Size() * 0.5f, Vector2.One, 0, 0);
            }

            base.DrawSelf(spriteBatch);
        }

        void Draw(SpriteBatch spriteBatch, Texture2D texture, Color color)
        {
            var rect = GetDimensions().ToRectangle();
            var corner = new Rectangle(0, 0, cornerSize, cornerSize);
            spriteBatch.Draw(texture, rect.TopLeft(), corner, color, 0f, Vector2.Zero, Vector2.One, 0, 0);
            spriteBatch.Draw(texture, rect.TopRight(), corner, color, MathHelper.PiOver2, Vector2.Zero, Vector2.One, 0, 0);
            spriteBatch.Draw(texture, rect.BottomRight(), corner, color, MathHelper.Pi, Vector2.Zero, Vector2.One, 0, 0);
            spriteBatch.Draw(texture, rect.BottomLeft(), corner, color, MathHelper.Pi + MathHelper.PiOver2, Vector2.Zero, Vector2.One, 0, 0);
            var bar = new Rectangle(cornerSize, 0, barSize, cornerSize);
            spriteBatch.Draw(texture, rect.TopLeft(cornerSize, 0, rect.Width - cornerSize * 2, cornerSize), bar, color, 0f, Vector2.Zero, 0, 0);
            spriteBatch.Draw(texture, rect.TopRight(0, cornerSize, rect.Height - cornerSize * 2, cornerSize), bar, color, MathHelper.PiOver2, Vector2.Zero, 0, 0);
            spriteBatch.Draw(texture, rect.BottomRight(-cornerSize, 0, rect.Width - cornerSize * 2, cornerSize), bar, color, MathHelper.Pi, Vector2.Zero, 0, 0);
            spriteBatch.Draw(texture, rect.BottomLeft(0, -cornerSize, rect.Height - cornerSize * 2, cornerSize), bar, color, MathHelper.Pi + MathHelper.PiOver2, Vector2.Zero, 0, 0);
            var center = new Rectangle(cornerSize, cornerSize, barSize, barSize);
            spriteBatch.Draw(texture, rect.TopLeft(cornerSize, cornerSize, rect.Width - cornerSize * 2, rect.Height - cornerSize * 2), center, color, 0, Vector2.Zero, 0, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;

            }

            base.Update(gameTime);
        }

        public override void Click(UIMouseEvent evt)
        {
            Toggle = !Toggle;
            OnToggle?.Invoke(ID, Toggle);
            SoundEngine.PlaySound(SoundID.MenuTick);
            base.Click(evt);
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            base.MouseOver(evt);
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            base.MouseOut(evt);
        }
    }
}
