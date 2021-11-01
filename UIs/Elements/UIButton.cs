using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace ConduitLib.UIs.Elements
{
    public class UIButton : UIElement, IUIDescription
    {
        public string Name { get; set; }
        public Asset<Texture2D> Texture { get; set; }
        public Asset<Texture2D> Outline { get; set; }
        public Color? OutlineColor { get; set; }

        public delegate void onClick();
        public new onClick OnClick { get; set; }
        public Func<string> Description { get; set; }

        public UIButton(Asset<Texture2D> texture)
        {
            this.Texture = texture;
            this.Width.Set(Math.Max(texture.Width(), texture.Width()), 0);
            this.Height.Set(Math.Max(texture.Height(), texture.Height()), 0);
            this.Description = null;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var pos = GetDimensions().Position();

            spriteBatch.Draw(Texture.Value, pos, Color.White);

            if (IsMouseHovering && Outline != null)
                spriteBatch.Draw(Outline.Value, pos, OutlineColor ?? Color.White);

            if (Name != null)
                spriteBatch.DrawString(FontAssets.MouseText.Value, Name, pos + new Vector2(Width.GetValue(0) + 4, 0), Color.White);

            base.DrawSelf(spriteBatch);
        }

        public override void Click(UIMouseEvent evt)
        {
            OnClick?.Invoke();
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
