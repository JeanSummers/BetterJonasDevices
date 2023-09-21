using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;


namespace BetterJonasDevices
{
    [HarmonyPatch(typeof(BlockEntityBaseReturnTeleporter), methodName: "OnServerGameTick")]
    internal class TeleportationPatch
    {
        public static bool Prefix(float dt,
            BlockEntityBaseReturnTeleporter __instance,
            ICoreAPI ___Api,
            BlockPos ___Pos,
            ref bool ___activated,
            ref float ___spinupAccum)
        {
            var Api = ___Api;
            var Pos = ___Pos;

            if (!___activated) return false;

            ___spinupAccum += dt;

            if (___spinupAccum > 5)
            {
                ___activated = false;
                var plr = Api.World.NearestPlayer(Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5) as IServerPlayer;
                if (plr.Entity.Pos.DistanceTo(Pos.ToVec3d().Add(0.5, 0, 0.5)) < 5)
                {
                    var pos = plr.GetSpawnPosition(false);
                    plr.Entity.TeleportToDouble(pos.X, pos.Y, pos.Z);

                    // onTeleported callback gets called when chunk you are teleported from is loaded. Not the one you teleporting to
                    var sapi = (ICoreServerAPI)Api;
                    var startPos = Pos.Copy();
                    sapi.WorldManager.LoadChunkColumnPriority((int)pos.X / sapi.WorldManager.ChunkSize, (int)pos.Z / sapi.WorldManager.ChunkSize, new ChunkLoadOptions
                    {
                        OnLoaded = delegate
                        {
                            OnTeleported(Api, plr, pos.AsBlockPos, startPos);
                        }
                    });

                    Api.World.PlaySoundAt(new AssetLocation("sounds/effect/translocate-breakdimension"), plr.Entity.Pos.X, plr.Entity.Pos.Y, plr.Entity.Pos.Z, null, false, 16);
                }
                else
                {
                    // When no players in range, stop and reset spinup
                    __instance.MarkDirty(true);
                    ___spinupAccum = 0;
                    return false;
                }

                Api.World.PlaySoundAt(new AssetLocation("sounds/effect/translocate-breakdimension"), Pos.X + 0.5f, Pos.Y + 0.5f, Pos.Z + 0.5f, null, false, 16);

                int color = ColorUtil.ToRgba(100, 220, 220, 220);
                Api.World.SpawnParticles(120, color, Pos.ToVec3d(), Pos.ToVec3d().Add(1, 1, 1), new Vec3f(-1, -1, -1), new Vec3f(1, 1, 1), 2, 0, 1);

                color = ColorUtil.ToRgba(255, 53, 221, 172);
                Api.World.SpawnParticles(100, color, Pos.ToVec3d().Add(0, 0.25, 0), Pos.ToVec3d().Add(1, 1.25, 1), new Vec3f(-4, 0, -4), new Vec3f(4, 4, 4), 2, 0.6f, 0.8f, EnumParticleModel.Cube);

                Api.World.BlockAccessor.SetBlock(0, Pos);
            }

            return false;
        }

        private static void OnTeleported(ICoreAPI Api, IServerPlayer plr, BlockPos respPosition, BlockPos startPos)
        {
            var dischargedTeleport = Api.World.GetBlock(new AssetLocation("betterjonasdevices", "basereturnteleporter-discharged"));

            // Search blocks around for better position
            var pos = FindSpaceToPlace(Api, respPosition);
            if (pos != null)
            {
                Api.World.BlockAccessor.SetBlock(dischargedTeleport.Id, pos);
                Api.World.BlockAccessor.MarkBlockDirty(pos);
                return;
            }

            // Try to give teleporter item to player
            var itemStack = new ItemStack(dischargedTeleport);
            var stackGiven = plr.InventoryManager.TryGiveItemstack(itemStack);
            if (stackGiven) return;

            // Try to spawn item in the world
            var itemPosition = respPosition.UpCopy().ToVec3d().Add(0.5, 0.5, 0.5);
            var spawned = Api.World.SpawnItemEntity(itemStack, itemPosition);
            if (spawned != null) return;

            // In the worst case try to place teleporter back into starting location
            Api.World.BlockAccessor.SetBlock(dischargedTeleport.Id, startPos);
        }

        static BlockPos FindSpaceToPlace(ICoreAPI Api, BlockPos pos)
        {
            try
            {
                var sideSpace = FindSpaceAround(Api, pos);
                if (sideSpace.HasValue) return sideSpace.Value.Item2;

                var upperSpace = FindUpperEmptySpace(Api, pos);
                if (!upperSpace.HasValue) return null;

                var lowerSpace = FindLowestEmptySpace(Api, upperSpace.Value.Item2);
                if (!lowerSpace.HasValue) return null;

                return lowerSpace.Value.Item2;
            } catch (Exception ex)
            {
                Api.Logger.Error(ex);
                return null;
            }
        }

        static (Block, BlockPos)? FindSpaceAround(ICoreAPI Api, BlockPos pos)
        {
            foreach (var position in SearchPositions(pos))
            {
                var result = FindLowestEmptySpace(Api, position);
                if (result != null) return result;
            }
            return default;
        }

        static (Block, BlockPos)? FindLowestEmptySpace(ICoreAPI Api, BlockPos pos)
        {
            var currentPos = pos;
            var currentBlock = GetBlock(currentPos, Api);
            if (!CanReplace(currentBlock)) return default;

            var lowerPos = currentPos.DownCopy();
            var lowerBlock = GetBlock(lowerPos, Api);
            while (Math.Abs(currentPos.Y - pos.Y) < 20)
            {
                if (!CanReplace(lowerBlock)) break;

                currentPos = lowerPos;
                currentBlock = lowerBlock;
                lowerPos = currentPos.DownCopy();
                lowerBlock = GetBlock(lowerPos, Api);
            }

            if (!lowerBlock.SideSolid[BlockFacing.UP.Index]) return default;

            return (currentBlock, currentPos);
        }

        static (Block, BlockPos)? FindUpperEmptySpace(ICoreAPI Api, BlockPos pos)
        {
            var currentPos = pos;
            var currentBlock = GetBlock(currentPos, Api);
            while (Math.Abs(currentPos.Y - pos.Y) < 20)
            {
                if (CanReplace(currentBlock)) return (currentBlock, currentPos);
                currentPos = currentPos.UpCopy();
                currentBlock = GetBlock(currentPos, Api);
            }

            return default;
        }

        static bool CanReplace(Block block) => block.Id == 0 || block.Code.Path.Contains("tallgrass");
        static Block GetBlock(BlockPos pos, ICoreAPI Api) => Api.World.BlockAccessor.GetBlock(pos, BlockLayersAccess.Solid);

        static IEnumerable<BlockPos> SearchPositions(BlockPos pos)
        {
            var p = pos.NorthCopy();
            yield return p;
            yield return p.East();
            yield return p.South();
            yield return p.South();
            yield return p.West();
            yield return p.West();
            yield return p.North();
            yield return p.North();
        }
    }

}
