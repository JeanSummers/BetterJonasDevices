using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace BetterJonasDevices.RiftWard
{
    internal class BlockRiftWard : Block
    {
        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            BlockEntityRiftWard FindBlockEntity(BlockSelection blockSelection)
            {
                var pos = blockSelection.Position;
                var entity = GetBlockEntity<BlockEntityRiftWard>(pos);
                if (entity != null) return entity;
                entity = GetBlockEntity<BlockEntityRiftWard>(pos.Down());
                if (entity != null) return entity;
                return GetBlockEntity<BlockEntityRiftWard>(pos.Down());
            }

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
                    ActionLangCode = "blockhelp-basereturn-deactivate",
                    MouseButton = EnumMouseButton.Right,
                    ShouldApply = (WorldInteraction wi, BlockSelection blockSelection, EntitySelection entitySelection) => FindBlockEntity(blockSelection)?.On == true
                },
                new()
                {
                    ActionLangCode = "blockhelp-basereturn-activate",
                    MouseButton = EnumMouseButton.Right,
                    ShouldApply = (WorldInteraction wi, BlockSelection blockSelection, EntitySelection entitySelection) => FindBlockEntity(blockSelection)?.On == false
                }
            };


            base.OnLoaded(api);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            if (byItemStack != null && byItemStack.Attributes != null)
            {
                var be = GetBlockEntity<BlockEntityRiftWard>(blockPos);

                var tree = byItemStack.Attributes;

                tree.SetInt("posx", blockPos.X);
                tree.SetInt("posy", blockPos.Y);
                tree.SetInt("posz", blockPos.Z);

                be.FromTreeAttributes(tree, world);
            }
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            var be = GetBlockEntity<BlockEntityRiftWard>(pos);
            TreeAttribute attr = new();
            be.ToTreeAttributes(attr);
            ItemStack stack = new(Id, EnumItemClass.Block, 1, attr, world);
            return new ItemStack[] { stack };
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            var be = GetBlockEntity<BlockEntityRiftWard>(blockSel);

            ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (byPlayer.Entity.Controls.ShiftKey && activeSlot.Empty)
            {
                ItemStack stack = GetDrops(world, blockSel.Position, byPlayer)[0];
                if (!byPlayer.InventoryManager.TryGiveItemstack(stack, true))
                {
                    world.SpawnItemEntity(stack, blockSel.Position.ToVec3d().AddCopy(0.5, 0.1, 0.5));
                }
                world.BlockAccessor.SetBlock(0, blockSel.Position);
            }

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
    }
}
