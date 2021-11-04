using ConduitLib.APIs;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ConduitLib
{
    public enum PacketID : byte
    {
        SendWorld,
        PlaceConduit,
        RemoveConduit,
        SetConnection,
        SyncConduit,
        PlaceInWorld,
    }
    public static class ConduitNet
    {
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            var id = (PacketID)reader.ReadByte();
            var i = reader.ReadInt16();
            var j = reader.ReadInt16();
            switch (id)
            {
                case PacketID.SendWorld:
                    var tag = reader.ReadTag();
                    ModContent.GetInstance<ConduitWorld>().LoadWorldData(tag);
                    break;
                case PacketID.PlaceConduit:
                    var conduitType = reader.ReadType();
                    ConduitUtil.PlaceConduit(i, j, conduitType, out _);
                    break;
                case PacketID.RemoveConduit:
                    conduitType = reader.ReadType();
                    ConduitUtil.RemoveConduit(i, j, conduitType);
                    break;
                case PacketID.SetConnection:
                    ConduitUtil.SetConnection(i, j, reader.ReadType(), reader.ReadByte(), reader.ReadBoolean());
                    break;
                case PacketID.SyncConduit:
                    conduitType = reader.ReadType();
                    var isNew = reader.ReadBoolean();
                    ref var list = ref ConduitWorld.Conduits[i, j];
                    if (list is null)
                        list = new();
                    if (reader.ReadBoolean())
                    {
                        tag = reader.ReadTag();

                        var conduit = list.Find(c => c.GetType() == conduitType);
                        bool flag = conduit is null;
                        if (flag)
                            conduit = (ModConduit)Activator.CreateInstance(conduitType);

                        conduit.LoadData(tag, false);

                        if (flag)
                            list.Add(conduit);
                        if (isNew)
                            conduit.OnPlace();
                    }
                    else
                    {
                        list.RemoveAll(c =>
                        {
                            if (c.GetType() == conduitType)
                            {
                                c.OnRemove();
                                return true;
                            }
                            return false;
                        });
                    }
                    if (Main.netMode == 2)
                    {
                        ConduitUtil.UpdateNetwork(i, j, conduitType);
                        SendPacket(PacketID.SyncConduit, -1, i, j, conduitType, isNew);
                    }
                    break;
                case PacketID.PlaceInWorld:
                    ConduitGlobal.PlaceInWorld(i, j, reader.ReadInt32());
                    break;
            }
        }

        public static bool SendPacket(PacketID id, int toClient = -1, int i = -1, int j = -1, params dynamic[] objs)
        {
            if (Main.netMode == 0)
                return false;

            bool flag = false;
            var packet = ConduitLib.Instance.GetPacket();
            packet.Write((byte)id);
            packet.Write((short)i);
            packet.Write((short)j);
            switch (id)
            {
                case PacketID.SendWorld:
                    var tag = new TagCompound();
                    ModContent.GetInstance<ConduitWorld>().SaveWorldData(tag);
                    packet.Write(tag);
                    break;
                case PacketID.PlaceConduit:
                    var conduitType = (Type)objs[0];
                    packet.Write(conduitType);
                    flag = !ConduitUtil.ContaisConduit(i, j, conduitType);
                    break;
                case PacketID.RemoveConduit:
                    conduitType = (Type)objs[0];
                    packet.Write(conduitType);
                    break;
                case PacketID.SetConnection:
                    packet.Write((Type)objs[0]);
                    packet.Write((byte)objs[1]);
                    packet.Write((bool)objs[2]);
                    break;
                case PacketID.SyncConduit:
                    conduitType = (Type)objs[0];
                    var isNew = (bool)objs[1];
                    packet.Write(conduitType);
                    packet.Write(isNew);
                    if (ConduitUtil.TryGetConduit(i, j, conduitType, out var conduit))
                    {
                        packet.Write(true);
                        tag = new TagCompound();
                        conduit.SaveData(tag);
                        packet.Write(tag);
                    }
                    else
                        packet.Write(false);
                    break;
                case PacketID.PlaceInWorld:
                    packet.Write((int)objs[0]);
                    break;
            }
            packet.Send(toClient);
            return flag;
        }
    }
}
