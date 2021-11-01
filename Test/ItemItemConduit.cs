using ConduitLib.APIs;

namespace ConduitLib.Test
{
    public class ItemItemConduit : AbstractItemConduit<ItemConduit>
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Item Conduit");
            Tooltip.SetDefault("For transfer Items");

            base.SetStaticDefaults();
        }
    }
}
