using System.Numerics;

namespace Proelium.Server.Physics;

public class Entity
{
    // TODO WHY PROPERTIES??????? DONT NEED MAYBE????
    public EntityFlags Flags { get; set; } = EntityFlags.None;
    public AABB Bounds { get; set; } = AABB.All;
    public AABB Collider { get; set; } = new();
    public float DragForce { get; set; } = 0;
    public float RepulsionRadius { get; set; } = 0;
    public float RepulsionForce { get; set; } = 0;
    public float RepulsionMaxMagnitude { get; set; } = 0;
    public float VelocityEpsilon { get; set; } = 1f / 256f;

    public Cell Cell { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public ulong LayerMask { get; set; } = ulong.MaxValue;

    internal Entity(Vector2 position)
    {
        Position = position;
    }

    public void EnableFlags(EntityFlags flags)
    {
        Flags |= flags;
    }
    public void DisableFlags(EntityFlags flags)
    {
        Flags &= ~flags;
    }

    public void EnableLayers(ulong layers)
    {
        LayerMask |= layers;
    }
    public void DisableLayers(ulong layers)
    {
        LayerMask &= ~layers;
    }

    public static bool LayersOverlap(Entity a, Entity b)
    {
        return (a.LayerMask & b.LayerMask) != 0;
    }
}