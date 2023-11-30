using HarmonyLib;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BetterJonasDevices;

internal class BetterRiftWardBlock : Block
{
    WorldInteraction[] interactions;

    public override void OnLoaded(ICoreAPI api)
    {
        interactions = new WorldInteraction[]
        {
            new()
            {
                ActionLangCode = "blockhelp-behavior-rightclickpickup",
                HotKeyCode = "shift",
                MouseButton = EnumMouseButton.Right,
                RequireFreeHand = true
            },
            new()
            {
                ActionLangCode = "blockhelp-forge-fuel",
                Itemstacks = new ItemStack[] { new ItemStack(api.World.GetItem(new AssetLocation("gear-temporal"))) },
                MouseButton = EnumMouseButton.Right,
            },
            new()
            {
                ActionLangCode = "blockhelp-basereturn-activate",
                MouseButton = EnumMouseButton.Right,
            }
        };


        base.OnLoaded(api);
    }

    public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(world, blockPos, byItemStack);

        var be = GetBlockEntity<BetterRiftWardEntity>(blockPos);
        be.FromItem(byItemStack);
    }

    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        var be = GetBlockEntity<BetterRiftWardEntity>(pos);
        var data = be == null ? TryGetNonBetter(pos) : be.GetItemData();

        return new ItemStack[] { new(Id, EnumItemClass.Block, 1, data, world) };
    }

    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {

        ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
        if (byPlayer.Entity.Controls.ShiftKey && activeSlot.Empty)
        {
            ItemStack stack = GetDrops(world, blockSel.Position, byPlayer)[0];
            if (!byPlayer.InventoryManager.TryGiveItemstack(stack, true))
            {
                world.SpawnItemEntity(stack, blockSel.Position.ToVec3d().AddCopy(0.5, 0.1, 0.5));
            }

            world.BlockAccessor.SetBlock(0, blockSel.Position);
            return true;
        }

        var be = GetBlockEntity<BetterRiftWardEntity>(blockSel);
        if (be != null && be.OnInteract(blockSel, byPlayer))
        {
            return true;
        }

        return base.OnBlockInteractStart(world, byPlayer, blockSel);
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
    {
        return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
    }

    /// <summary>
    /// In case when player tries to pick up riftward which was placed before enabling the mod
    /// </summary>
    private TreeAttribute TryGetNonBetter(BlockPos pos)
    {
        var be = GetBlockEntity<BlockEntityRiftWard>(pos);
        if (be == null) return null;

        var fuelDays = Traverse.Create(be).Field("fuelDays").GetValue() as double?;

        TreeAttribute tree = new();
        tree.SetDouble("fuelUntilTotalDays", fuelDays ?? 0);
        tree.SetBool("on", be.On);
        return tree;
    }
}
