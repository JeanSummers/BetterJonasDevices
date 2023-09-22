using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;

namespace BetterJonasDevices;

internal class GlowingEntities
{
    static readonly int scanDelay = 1000;
    static readonly byte glowLevel = 255;

    long lastScan = 0;
    readonly Dictionary<long, WeakReference<Entity>> glowingEntities = new();
    readonly Dictionary<long, int> glowLevels = new();

    public int Count => glowingEntities.Count;

    public void Scan(IClientWorldAccessor world)
    {
        if (world.ElapsedMilliseconds - lastScan < scanDelay) return;

        static bool Filter(Entity entity)
        {
            if (entity.Properties?.Client?.GlowLevel == glowLevel) return false;
            var animations = entity.Properties?.Client?.Animations;
            return animations != null && animations.Length > 0;
        }

        var r = Config.NightVision.HiglightRange;
        var entities = world.GetEntitiesAround(world.Player.Entity.Pos.XYZ, r, r, Filter);
        foreach (var entity in entities)
        {
            var id = entity.EntityId;
            if (glowingEntities.ContainsKey(id))
            {
                RestoreEntity(id, glowingEntities[id]);
            }
            glowingEntities.Add(id, new(entity));
            glowLevels.Add(id, entity.Properties.Client.GlowLevel);
            entity.Properties.Client.GlowLevel = glowLevel;
        };

        lastScan = world.ElapsedMilliseconds;
    }

    public void Clear()
    {
        foreach (var (id, weakRef) in glowingEntities)
        {
            RestoreEntity(id, weakRef);
        }
    }

    private void RestoreEntity(long id, WeakReference<Entity> weakRef)
    {
        if (weakRef.TryGetTarget(out Entity entity))
        {
            entity.Properties.Client.GlowLevel = glowLevels[id];
        }
        glowingEntities.Remove(id);
        glowLevels.Remove(id);
    }
}
