using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;


namespace BetterJonasDevices;

public class BetterJonasDevicesModSystem : ModSystem
{
    ICoreAPI api;
    NightVisionRenderer nightVisionRenderer;
    Harmony harmony;

    string modId;

    public override void StartPre(ICoreAPI api)
    {
        this.api = api;

        modId = Mod.Info.ModID;
        Config.Update(api, $"{modId}.json");
    }

    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockClass("BlockBetterRiftWard", typeof(BetterRiftWardBlock));
        api.RegisterBlockClass("BlockDischargedBaseReturnTeleporter", typeof(BlockDischargedBaseReturnTeleporter));
        api.RegisterBlockEntityClass("DischargedBaseReturnTeleporter", typeof(BlockEntityDischargedBaseReturnTeleporter));
        api.RegisterBlockEntityClass("BetterRiftWard", typeof(BetterRiftWardEntity));

        harmony = new Harmony(modId);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        nightVisionRenderer = new NightVisionRenderer(api);
        api.Event.RegisterRenderer(nightVisionRenderer, EnumRenderStage.AfterFinalComposition, modId);
    }

    public override void Dispose()
    {
        if (api.Side == EnumAppSide.Client)
        {
            var capi = (ICoreClientAPI)api;
            capi.Event.UnregisterRenderer(nightVisionRenderer, EnumRenderStage.AfterFinalComposition);
            nightVisionRenderer.Dispose();
        }
        harmony.UnpatchAll(modId);
        base.Dispose();
    }
}
