using HarmonyLib;
using System.Collections.Generic;
using Vintagestory.GameContent;


namespace BetterJonasDevices
{
    [HarmonyPatch(typeof(BlockEntityRiftWard), methodName: "BlockEntityRiftWard_OnRiftSpawned")]
    internal class ImproveRangePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.operand?.Equals(0.95) == true)
                {
                    instruction.operand = 1.0;
                }
                if (instruction.operand?.Equals(30f) == true)
                {
                    instruction.operand = 128f;
                }
                yield return instruction;
            }
        }
    }
}