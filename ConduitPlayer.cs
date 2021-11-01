using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ConduitLib
{
    public class ConduitPlayer : ModPlayer
    {
        public const string TAG_WRENCH_MODE = "wrenchMode";

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag[TAG_WRENCH_MODE] = (byte)ConduitWrench.Mode;
        }

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            if (tag.ContainsKey(TAG_WRENCH_MODE))
                ConduitWrench.Mode = (ConduitWrench.EWrenchMode)tag.GetByte(TAG_WRENCH_MODE);
        }
    }
}
