namespace Proelium.Server.Physics;

[Flags]
public enum EntityConfigFlags : byte
{
    None = 0b0000_0000_0000_0000,
    Bounds = 0b0000_0000_0000_0001,
    Collider = 0b0000_0000_0000_0010,
    ColliderTrigger = 0b0000_0000_0000_0100,
    Drag = 0b0000_0000_0000_1000,
    CanRepulseOthers = 0b0000_0000_0001_0000,
    CanBeRepulsed = 0b0000_0000_0010_0000,
    VelocityEpsilon = 0b0000_0000_0100_0000
}