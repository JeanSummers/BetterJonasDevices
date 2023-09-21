using BetterJonasDevices.RiftWard;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;


namespace BetterJonasDevices
{

    public class BetterJonasDevicesModSystem : ModSystem
    {
        ICoreAPI api;
        NightVisionRenderer nightVisionRenderer;
        Harmony harmony;

        string modId;

        public override bool AllowRuntimeReload => true;

        public override void Start(ICoreAPI api)
        {
            modId = Mod.Info.ModID;
            this.api = api;

            api.RegisterBlockClass("BetterRiftWard", typeof(BlockRiftWard));
            api.RegisterBlockClass("BlockDischargedBaseReturnTeleporter", typeof(BlockDischargedBaseReturnTeleporter));
            api.RegisterBlockEntityClass("DischargedBaseReturnTeleporter", typeof(BlockEntityDischargedBaseReturnTeleporter));

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
}
