using System.Numerics;

namespace Proelium.Server.Physics;

public class Entity
{
    public EntityFlags flags = EntityFlags.None;
    public AABB bounds = AABB.All;
    public AABB collider = new();
    public float dragForce = 0;
    public float repulsionRadius = 0;
    public float repulsionForce = 0;
    public float repulsionMaxMagnitude = 0;
    public float velocityEpsilon = 1f / 256f;

    public Cell cell;
    public Vector2 position;
    public Vector2 velocity;
    public ulong layerMask = ulong.MaxValue;

    internal Entity(Vector2 position)
    {
        this.position = position;
    }

    public void EnableFlags(EntityFlags flags)
    {
        this.flags |= flags;
    }
    public void DisableFlags(EntityFlags flags)
    {
        this.flags &= ~flags;
    }

    public void EnableLayers(ulong layers)
    {
        layerMask |= layers;
    }
    public void DisableLayers(ulong layers)
    {
        layerMask &= ~layers;
    }

    public static bool LayersOverlap(Entity a, Entity b)
    {
        return (a.layerMask & b.layerMask) != 0;
    }
}