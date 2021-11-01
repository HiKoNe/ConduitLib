using ConduitLib.APIs;
using System.Collections.Generic;

namespace ConduitLib
{
    public static class ConduitLoader
    {
        public static readonly List<ModConduit> Conduits = new();

        internal static void Unload()
        {
            Conduits.Clear();
        }

        public static void Register(ModConduit conduit)
        {
            Conduits.Add(conduit);
        }
    }
}
