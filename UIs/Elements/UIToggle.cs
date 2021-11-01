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
    public class UIToggle : UIElement, IUIDescription
    {
        public bool Toggle { get; set; }
        public string Name { get; set; }
        public Asset<Texture2D> OffTexture { get; set; }
        public Asset<Texture2D> OnTexture { get; set; }
        public Asset<Texture2D> Outline { get; set; }
        public Color? OffColor { get; set; }
        public Color? OnColor { get; set; }
        public Color? OutlineColor { get; set; }

        public delegate void onToggle(bool toggle);
        public onToggle OnToggle { get; set; }
        public Func<string> Description { get; set; }

        public UIToggle(Asset<Texture2D> offTexture, Asset<Texture2D> onTexture, bool toggle = default, string name = default)
        {
            this.OffTexture = offTexture;
            this.OnTexture = onTexture;
            this.Width.Set(Math.Max(offTexture.Width(), onTexture.Width()), 0);
            this.Height.Set(Math.Max(offTexture.Height(), onTexture.Height()), 0);
            this.Toggle = toggle;
            this.Name = name;
            this.Description = null;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var pos = GetDimensions().Position();
            var texture = Toggle ? OnTexture.Value : OffTexture.Value;
            var color = Toggle ? OnColor : OffColor;

            if (texture != null)
                spriteBatch.Draw(texture, pos, color ?? Color.White);
            
            if (IsMouseHovering && Outline != null)
                spriteBatch.Draw(Outline.Value, pos, OutlineColor ?? Color.White);

            if (Name != null)
                spriteBatch.DrawString(FontAssets.MouseText.Value, Name, pos + new Vector2(Width.GetValue(0) + 4, 0), Color.White);

            base.DrawSelf(spriteBatch);
        }

        public override void Click(UIMouseEvent evt)
        {
            Toggle = !Toggle;
            OnToggle?.Invoke(Toggle);
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
