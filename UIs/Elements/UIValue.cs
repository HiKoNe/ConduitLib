using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace ConduitLib.UIs.Elements
{
    public class UIValue : UIElement, IUIDescription
    {
        public long Value { get; set; }
        public string Name { get; set; }

        public delegate void onChange(long value);
        public onChange OnChange { get; set; }
        public Func<string> Description { get; set; }

        public UIValue(long value = default, string name = default, int size = 22)
        {
            this.Width.Set(size, 0);
            this.Height.Set(size, 0);
            this.Value = value;
            this.Name = name;
            this.Description = null;

            var plusButton = new UIButton(ConduitAsset.ButtonMini[1]) { Outline = ConduitAsset.ButtonMini[0] };
            plusButton.OnClick = () => OnChange?.Invoke(++Value);
            Append(plusButton);

            var minusButton = new UIButton(ConduitAsset.ButtonMini[2]) { Outline = ConduitAsset.ButtonMini[0] };
            minusButton.Top.Set(0, 0.5f);
            minusButton.OnClick = () => OnChange?.Invoke(--Value);
            Append(minusButton);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var pos = GetDimensions().Position();

            var font = FontAssets.MouseText.Value;
            spriteBatch.DrawString(font, Value.ToString(), pos + new Vector2(Width.GetValue(0) * 0.5f + 2, 0), Color.White);

            if (Name != null)
                spriteBatch.DrawString(font, Name, pos + new Vector2(Width.GetValue(0) + font.MeasureString(Value.ToString()).X, 0), Color.White);

            base.DrawSelf(spriteBatch);
        }

        public override void ScrollWheel(UIScrollWheelEvent evt)
        {
            Value += evt.ScrollWheelValue / 120;
            OnChange?.Invoke(Value);
            SoundEngine.PlaySound(SoundID.MenuTick);

            base.ScrollWheel(evt);
        }
    }
}
