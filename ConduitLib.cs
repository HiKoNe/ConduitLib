using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ConduitLib
{
	public class ConduitLib : Mod
	{
		public static ConduitLib Instance { get; private set; }

        public static void Debug(object obj) => Instance.Logger.Debug(obj);

        public override void Load()
        {
            Instance = this;
            ConduitAsset.Load(this);
            On.Terraria.Main.DoDraw_WallsAndBlacks += this.Main_DoDraw_WallsAndBlacks;
        }

        public override void Unload()
        {
            Instance = null;
            ConduitLoader.Unload();
            ConduitAsset.Unload();
            On.Terraria.Main.DoDraw_WallsAndBlacks -= this.Main_DoDraw_WallsAndBlacks;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            base.HandlePacket(reader, whoAmI);
            ConduitNet.HandlePacket(reader, whoAmI);
        }

        void Main_DoDraw_WallsAndBlacks(On.Terraria.Main.orig_DoDraw_WallsAndBlacks orig, Main self)
        {
            orig(self);
            ConduitWorld.DrawConduits();
        }
    }
}