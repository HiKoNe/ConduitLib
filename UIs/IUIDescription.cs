using System;

namespace ConduitLib.UIs
{
    public interface IUIDescription
    {
        Func<string> Description { get; set; }
    }
}
