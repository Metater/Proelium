namespace Proelium.Server.Physics;

[Flags]
public enum EntityFlags : byte
{
    None                = 0b_0000_0000,
    Bounds              = 0b_0000_0001,
    Collider            = 0b_0000_0010,
    TriggerCollider     = 0b_0000_0100,
    Drag                = 0b_0000_1000,
    CanRepulseOthers    = 0b_0001_0000,
    CanBeRepulsed       = 0b_0010_0000,
    VelocityEpsilon     = 0b_0100_0000,
    Static      = 0b_1000_0000
}

public static class EntityFlagsExtension
{
    public static bool Has(this EntityFlags a, EntityFlags b)
    {
        return (a & b) != 0;
    }
}