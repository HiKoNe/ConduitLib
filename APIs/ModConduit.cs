using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.ID;

namespace ConduitLib.APIs
{
    public abstract class ModConduit : IModType, IEquatable<ModConduit>
    {
        public Mod Mod { get; internal set; }
        public string Name => GetType().Name;
        public string FullName => $"{Mod.Name}/{Name}";

        bool ILoadable.IsLoadingEnabled(Mod mod) => true;
        void ILoadable.Unload() { }
        void ILoadable.Load(Mod mod)
        {
            Mod = mod;
            ConduitLoader.Register(this);
        }

        internal int updateDelay;
        protected List<ModConduit> network;
        protected bool input;
        protected bool output = true;
        protected bool isConnector;
        protected private Point16 position;
        protected bool[] direction;

        public virtual List<ModConduit> Network
        {
            get => network;
            set
            {
                if (value is null)
                {
                    network = null;
                    return;
                }

                var list = new List<ModConduit>(value);
                list.RemoveAll(c => c.Position == Position || !c.Input);
                network = list;
            }
        }
        public bool IsConnector
        {
            get => isConnector;
            set
            {
                if (Main.netMode == 1)
                {
                    isConnector = value;
                    return;
                }

                if (value)
                {
                    if (ValidForConnector() && !ConduitWorld.ConduitsToUpdate.Contains(this))
                    {
                        ConduitWorld.ConduitsToUpdate.Add(this);
                        isConnector = true;
                    }
                }
                else
                {
                    ConduitWorld.ConduitsToUpdate.Remove(this);
                    isConnector = false;
                }
                UpdateConduit();
            }
        }
        public Point16 Position { get => position; set => position = value; }
        public bool[] Direction { get => direction; set => direction = value; } //Left, Up, Right, Down
        public bool Input { get => input; set { input = value; UpdateConduit(); } }
        public bool Output { get => output; set { output = value; UpdateConduit(); } }
        public virtual int? UpdateDelay { get; }
        public abstract Asset<Texture2D> Texture { get; }
        public abstract Asset<Texture2D> WrenchIcon { get; }

        public void UpdateConduit()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                ConduitUtil.UpdateNetwork(Position.X, Position.Y, GetType());
            ConduitNet.SendPacket(PacketID.SyncConduit, -1, Position.X, Position.Y, GetType(), false);
        }

        public abstract bool ValidForConnector();
        public virtual void OnPlace() { }
        public virtual void OnRemove() { }
        public virtual void OnUpdate() { }
        public virtual void OnInitializeUI(ref UIPanel panel, StyleDimension rightDim, StyleDimension topDim) { }
        public virtual bool OnDraw(in SpriteBatch spriteBatch, ref int frameX, ref int frameY, ref float? alpha) => true;
        public virtual void SaveData(TagCompound tag) { }
        public virtual void LoadData(TagCompound tag) { }

        public virtual void InternalSaveData(TagCompound tag)
        {
            tag["x"] = position.X;
            tag["y"] = position.Y;
            tag["dir0"] = direction[0];
            tag["dir1"] = direction[1];
            tag["dir2"] = direction[2];
            tag["dir3"] = direction[3];
            tag["input"] = input;
            tag["output"] = output;
            tag["connector"] = isConnector;
            SaveData(tag);
        }
        public virtual void InternalLoadData(TagCompound tag, bool needUpdate = true)
        {
            position = new Point16(tag.GetShort("x"), tag.GetShort("y"));
            direction = new bool[] { tag.GetBool("dir0"), tag.GetBool("dir1"), tag.GetBool("dir2"), tag.GetBool("dir3") };
            input = tag.GetBool("input");
            output = tag.GetBool("output");
            isConnector = tag.GetBool("connector");
            LoadData(tag);
            if (needUpdate)
                IsConnector = isConnector;
        }

        public override string ToString() => $"Type: {GetType()}, Pos: {Position}";
        public override bool Equals(object obj) => (obj is ModConduit other) && Equals(other);
        public override int GetHashCode() => Position.X + (Position.Y << 16);
        public bool Equals(ModConduit other) => GetHashCode() == other.GetHashCode() && GetType() == other.GetType();
    }
}
