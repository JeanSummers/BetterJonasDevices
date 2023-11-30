using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace BetterJonasDevices;

internal class BetterRiftWardEntity : BlockEntityRiftWard
{
    public void FromItem(ItemStack item)
    {
        if (item == null || item.Attributes == null) return;

        var tree = item.Attributes;

        fuelDays = tree.GetDouble("fuelUntilTotalDays", fuelDays);
        On = tree.GetBool("on", On);
    }

    public TreeAttribute GetItemData()
    {
        TreeAttribute tree = new();
        tree.SetDouble("fuelUntilTotalDays", fuelDays);
        tree.SetBool("on", On);
        return tree;
    }
}
