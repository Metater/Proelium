using System.Numerics;

namespace Proelium.Server.Physics;

public class Entity
{
    public EntityFlags Flags { get; set; } = EntityFlags.None;
    public AABB Bounds { get; set; } = 
    public AABB collider;
    public float dragForce;
    public float repulsionRadius;
    public float repulsionForce;
    public float repulsionMaxMagnitude;
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
        this.Flags |= flags;
    }
    public void DisableFlags(EntityFlags flags)
    {
        this.Flags &= ~flags;
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