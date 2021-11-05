using ConduitLib.Contents.Items;
using Terraria;
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

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (Main.netMode == 2)
                ConduitNet.SendPacket(PacketID.SendWorld, fromWho);
            base.SyncPlayer(toWho, fromWho, newPlayer);
        }
    }
}
