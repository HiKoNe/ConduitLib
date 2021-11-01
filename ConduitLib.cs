using ConduitLib.APIs;
using Terraria;
using Terraria.ModLoader;

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

        void Main_DoDraw_WallsAndBlacks(On.Terraria.Main.orig_DoDraw_WallsAndBlacks orig, Main self)
        {
            orig(self);
            ConduitWorld.DrawConduits();
        }
    }
}