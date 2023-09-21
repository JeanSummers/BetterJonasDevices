using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.ServerMods;

namespace BetterJonasDevices
{
    internal class BlockDischargedBaseReturnTeleporter : Block
    {
        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            interactions = new WorldInteraction[]
            {
                new WorldInteraction()
                {
                    ActionLangCode = "betterjonasdevices:blockhelp-basereturn-recharge",
                    Itemstacks = new ItemStack[] { new ItemStack(api.World.GetItem(new AssetLocation("gear-temporal"))) },
                    MouseButton = EnumMouseButton.Right,
                }
            };


            base.OnLoaded(api);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty || slot.Itemstack.Collectible is not ItemTemporalGear)
            {
                return base.OnBlockInteractStart(world, byPlayer, blockSel);
            }

            slot.TakeOut(1);
            world.PlaySoundAt(new AssetLocation("sounds/effect/latch"), blockSel.Position.X + 0.5, blockSel.Position.Y, blockSel.Position.Z + 0.5, byPlayer, true, 16);
            Block block = world.GetBlock(new AssetLocation("basereturnteleporter"));
            world.BlockAccessor.SetBlock(block.Id, blockSel.Position);

            return true;
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
