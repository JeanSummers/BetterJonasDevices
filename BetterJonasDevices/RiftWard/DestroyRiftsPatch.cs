using HarmonyLib;
using System;
using Vintagestory.API.Server;
using Vintagestory.GameContent;


namespace BetterJonasDevices;

[HarmonyPatch(typeof(BlockEntityRiftWard), methodName: nameof(BlockEntityRiftWard.Initialize))]
internal class DestroyRiftsPatch
{
    static readonly AccessTools.FieldRef<BlockEntityRiftWard, double> fuelDays = AccessTools.FieldRefAccess<BlockEntityRiftWard, double>("fuelDays");

    public static void Postfix(ICoreServerAPI ___sapi, BlockEntityRiftWard __instance)
    {
        var api = ___sapi;
        var self = __instance;
        if (api == null) return;

        if (Config.RiftWard.DestructionRange > 0)
        {
            self.RegisterGameTickListener((dt) => DestroyRifts(api, self), 30 * 1000);
        }
        if (Config.RiftWard.RestorationRange > 0)
        {
            self.RegisterGameTickListener((dt) => RestoreStability(api, self), 5 * 1000);
        }
    }

    private static void RestoreStability(ICoreServerAPI api, BlockEntityRiftWard self)
    {
        if (!self.On || fuelDays(self) <= 0) return;

        var pos = self.Pos.ToVec3d();

        var r = Config.RiftWard.RestorationRange;
        foreach (var player in api.World.GetPlayersAround(pos, r*r, 2*r))
        {
            var temp = player.Entity.GetBehavior<EntityBehaviorTemporalStabilityAffected>();
            if (temp != null)
            {
                temp.OwnStability = 1;
            }
        }
    }

    private static void DestroyRifts(ICoreServerAPI api, BlockEntityRiftWard self)
    {
        if (!self.On || fuelDays(self) <= 0) return;

        var riftSystem = api.ModLoader.GetModSystem<ModSystemRifts>();
        var rifts = riftSystem?.riftsById;
        if (rifts == null) return;

        var pos = self.Pos.ToVec3d();

        var r = Config.RiftWard.DestructionRange;
        bool destroyed = false;
        foreach (var rift in rifts.Values)
        {
            if (rift.Position.DistanceTo(pos) > r) continue;

            var dieTime = Math.Min(rift.DieAtTotalHours, api.World.Calendar.TotalHours + 0.1);
            if (dieTime == rift.DieAtTotalHours) continue;

            rift.DieAtTotalHours = dieTime;
            destroyed = true;
        }
        if (destroyed)
        {
            riftSystem.BroadCastRifts();
        }
    }
}