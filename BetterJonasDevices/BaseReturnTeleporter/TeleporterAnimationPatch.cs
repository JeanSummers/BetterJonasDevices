using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.GameContent;


namespace BetterJonasDevices
{
    [HarmonyPatch(typeof(BlockEntityBaseReturnTeleporter), methodName: nameof(BlockEntityBaseReturnTeleporter.FromTreeAttributes))]
    internal class TeleporterAnimationPatch
    {
        public static void Postfix(
            BlockEntityBaseReturnTeleporter __instance,
            ICoreAPI ___Api,
            bool ___activated)
        {
            if (___Api?.Side != EnumAppSide.Client || ___activated) return;

            __instance.GetBehavior<BEBehaviorAnimatable>().animUtil.StopAnimation("active");
        }
    }
}
