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
        public Point16 Position { get; set; }
        public bool[] Direction { get; set; } //Left, Up, Right, Down
        public bool Input { get => input; set { input = value; UpdateNetwork(); } }
        private bool input;
        public bool Output { get => output; set { output = value; UpdateNetwork(); } }
        private bool output = true;
        public virtual int? UpdateDelay { get; }
        public abstract Asset<Texture2D> Texture { get; }
        public abstract Asset<Texture2D> WrenchIcon { get; }
        public abstract bool ValidForConnector();

        bool isConnector;
        public bool IsConnector
        {
            get => isConnector;
            set
            {
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
                    if (ConduitWorld.ConduitsToUpdate.Remove(this))
                        isConnector = false;
                }
                UpdateNetwork();
            }
        }

        public void UpdateNetwork() => ConduitUtil.UpdateNetwork(Position.X, Position.Y, GetType());

        public virtual void OnPlace() { }
        public virtual void OnRemove() { }
        public virtual void OnUpdate() { }
        public virtual void OnInitializeUI(ref UIPanel panel, StyleDimension rightDim, StyleDimension topDim) { }
        public virtual void OnDraw(in SpriteBatch spriteBatch, ref int frameX, ref int frameY, ref float? alpha)
        {
            spriteBatch.Draw(Texture.Value,
                Position.ToVector2() * 16 - Main.screenPosition,
                new Rectangle(frameX, frameY, 16, 16),
                alpha.HasValue ? Color.White * alpha.Value : Lighting.GetColor(Position.X, Position.Y),
                0f, Vector2.Zero, 1f, 0f, 0f);
        }
        public virtual void SaveData(TagCompound tag)
        {
            tag["x"] = Position.X;
            tag["y"] = Position.Y;
            tag["dir0"] = Direction[0];
            tag["dir1"] = Direction[1];
            tag["dir2"] = Direction[2];
            tag["dir3"] = Direction[3];
            tag["input"] = Input;
            tag["output"] = Output;
            tag["connector"] = IsConnector;
        }
        public virtual void LoadData(TagCompound tag)
        {
            Position = new Point16(tag.GetShort("x"), tag.GetShort("y"));
            Direction = new bool[] { tag.GetBool("dir0"), tag.GetBool("dir1"), tag.GetBool("dir2"), tag.GetBool("dir3") };
            if (tag.GetBool("connector"))
                IsConnector = true;
            Input = tag.GetBool("input");
            Output = tag.GetBool("output");
        }

        public override string ToString() => $"Type: {GetType()}, Pos: {Position}";
        public override bool Equals(object obj) => (obj is ModConduit other) && Equals(other);
        public override int GetHashCode() => Position.X + (Position.Y << 16);
        public bool Equals(ModConduit other) => GetHashCode() == other.GetHashCode() && GetType() == other.GetType();
    }
}
