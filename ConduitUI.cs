using ConduitLib.UIs;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ConduitLib
{
    public class ConduitUI : ModSystem
    {
        public static UIWrench UIWrench { get; private set; }
        public static UIProbe UIProbe { get; private set; }
        public static UserInterface UI { get; private set; }

        public override void Load()
        {
            if (!Main.dedServ)
            {
                UIWrench = new UIWrench();
                UIProbe = new UIProbe();
                UI = new UserInterface();
            }
        }

        public override void Unload()
        {
            UIWrench = null;
            UIProbe = null;
            UI = null;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name == "Vanilla: Wire Selection");
            if (index != -1)
            {
                layers.Insert(index, UIWrench);
                layers.Insert(index, UIProbe);
            }

            index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
            if (index != -1)
                layers.Insert(index, new LegacyGameInterfaceLayer("ConduitLib: UI", () => { UI.Draw(Main.spriteBatch, null); return true; }, InterfaceScaleType.UI));
            
        }

        public override void UpdateUI(GameTime gameTime)
        {
            UIWrench.Update();
            UIProbe.Update();
            UI.Update(gameTime);
        }
    }
}
