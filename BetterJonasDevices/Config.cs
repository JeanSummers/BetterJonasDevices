using Vintagestory.API.Common;

namespace BetterJonasDevices;

class NightVisionData
{
    public bool OverlayEnabled = true;
    public int HiglightRange = 64;
}

class RiftWardData
{
    public int SupressionRange = 128;
    public int DestructionRange = 64;
    public int RestorationRange = 64;
}

class ConfigData
{
    public NightVisionData NightVision = new();
    public RiftWardData RiftWard = new();
}

internal static class Config
{
    private static readonly ConfigData data = new();

    public static NightVisionData NightVision => data.NightVision;
    public static RiftWardData RiftWard => data.RiftWard;

    public static void Update(ICoreAPI api, string filename)
    {
        try
        {
            var data = api.LoadModConfig<ConfigData>(filename);
            if (data == null) return;

            Merge(data);
        } catch
        {
            api.Logger.Error($"Unable to load mod config from {filename}");
        }

        api.StoreModConfig(data, filename);
    }

    private static void Merge(ConfigData input)
    {
        if (input.NightVision != null)
        {
            data.NightVision = input.NightVision;
        }
        if (input.RiftWard != null)
        {
            data.RiftWard = input.RiftWard;
        }
    }
}
