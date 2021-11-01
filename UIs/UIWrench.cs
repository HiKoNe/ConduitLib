using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace ConduitLib.UIs
{
    public class UIWrench : GameInterfaceLayer
    {
        Vector2 cursorPos;

        public new bool Active
        {
            get => base.Active;
            set
            {
                cursorPos = Main.MouseScreen / Main.UIScale;
                base.Active = value;
            }
        }

        public UIWrench() : base("ConduitLib: Wrench Selection", InterfaceScaleType.UI)
        {
            Active = false;
        }

        public void Update()
        {
            if (Active)
            {
                if (Main.LocalPlayer.DeadOrGhost || Main.LocalPlayer.HeldItem.ModItem is not ConduitWrench)
                    Active = false;
            }
        }

        protected override bool DrawSelf()
        {
            int i;
            for (i = 0; i < 3; i++)
            {
                var drawPos = cursorPos + (-Vector2.UnitY).RotatedBy(MathHelper.TwoPi / 3 * i - MathHelper.TwoPi / 6) * 25f;
                int textureID = i * 2;
                var color = new Color(100, 100, 100);

                if (Vector2.Distance(drawPos, Main.MouseScreen) < 19f)
                {
                    color = new Color(200, 200, 200);
                    Main.LocalPlayer.mouseInterface = true;
                    textureID++;

                    if (Main.mouseLeft && Main.mouseLeftRelease)
                        ConduitWrench.Mode = (ConduitWrench.EWrenchMode)i;

                    if (Main.mouseRight)
                        Active = false;
                }

                if (ConduitWrench.Mode == (ConduitWrench.EWrenchMode)i)
                    color = Color.White;

                Draw(ConduitAsset.WrenchGUI[textureID].Value, drawPos, color);
                Draw(ConduitAsset.Wrench[i].Value, drawPos, color);
            }

            int conduitsCount = ConduitLoader.Conduits.Count;
            i = 0;
            foreach (var conduit in ConduitLoader.Conduits)
            {
                var conduitType = conduit.GetType();
                var drawPos = cursorPos + (-Vector2.UnitY).RotatedBy(MathHelper.TwoPi / conduitsCount * i) * 70f;
                
                int textureID = (int)ConduitWrench.Mode * 2;
                var color = new Color(100, 100, 100);

                if (Vector2.Distance(drawPos, Main.MouseScreen) < 19f)
                {
                    color = new Color(200, 200, 200);
                    Main.LocalPlayer.mouseInterface = true;
                    textureID++;

                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        if (ConduitWrench.ConduitMode.Contains(conduitType))
                            ConduitWrench.ConduitMode.Remove(conduitType);
                        else
                            ConduitWrench.ConduitMode.Add(conduitType);
                    }

                    if (Main.mouseRight)
                        Active = false;
                }

                if (ConduitWrench.ConduitMode.Contains(conduitType))
                    color = Color.White;

                Draw(ConduitAsset.WrenchGUI[textureID].Value, drawPos, color);
                Draw(conduit.WrenchIcon.Value, drawPos, color);

                i++;
            }

            return base.DrawSelf();
        }

        static void Draw(Texture2D texture, Vector2 drawPos, Color color) =>
            Main.spriteBatch.Draw(texture, drawPos, texture.Bounds, color, 0f, texture.Bounds.Size() / 2f, Vector2.One, 0f, 0f);
    }
}
