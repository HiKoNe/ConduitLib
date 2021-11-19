using ConduitLib.Contents.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace ConduitLib.UIs
{
    public class UIProbe : GameInterfaceLayer
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

        public UIProbe() : base("ConduitLib: Probe Selection", InterfaceScaleType.UI)
        {
            Active = false;
        }

        public void Update()
        {
            if (Active)
            {
                if (Main.LocalPlayer.DeadOrGhost || Main.LocalPlayer.HeldItem.ModItem is not ConduitProbe)
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
                    {
                        if (i == 2)
                        {
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            var probe = (ConduitProbe)Main.LocalPlayer.HeldItem.ModItem;
                            if (probe.SavedConduit.Count > 0)
                            {
                                probe.SavedConduit = new();
                                Language.GetTextValue("Mods.ConduitLib.UI.DataClear").CreatePopup(Main.LocalPlayer.position, 30, Color.Red);
                            }
                        }
                        else
                            ConduitProbe.Mode = (ConduitProbe.EProbeMode)i;
                    }

                    if (Main.mouseRight)
                        Active = false;
                }

                if (ConduitProbe.Mode == (ConduitProbe.EProbeMode)i)
                    color = Color.White;

                Draw(ConduitAsset.WrenchGUI[textureID].Value, drawPos, color);
                Draw(ConduitAsset.Probe[i].Value, drawPos, color);
            }

            int conduitsCount = ConduitLoader.Conduits.Count;
            i = 0;
            foreach (var conduit in ConduitLoader.Conduits)
            {
                var conduitType = conduit.GetType();
                var drawPos = cursorPos + (-Vector2.UnitY).RotatedBy(MathHelper.TwoPi / conduitsCount * i) * 70f;

                int textureID = (int)ConduitProbe.Mode * 2;
                var color = new Color(100, 100, 100);

                if (Vector2.Distance(drawPos, Main.MouseScreen) < 19f)
                {
                    color = new Color(200, 200, 200);
                    Main.LocalPlayer.mouseInterface = true;
                    textureID++;

                    if (Main.mouseLeft && Main.mouseLeftRelease)
                        ConduitProbe.ConduitMode = conduitType;

                    if (Main.mouseRight)
                        Active = false;
                }

                if (ConduitProbe.ConduitMode == conduitType)
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
