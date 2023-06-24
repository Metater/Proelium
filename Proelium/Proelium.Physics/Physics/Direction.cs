namespace Proelium.Server.Physics;

[Flags]
public enum Direction : byte
{
    None    = 0b_0000,
    North   = 0b_0001,
    East    = 0b_0010,
    South   = 0b_0100,
    West    = 0b_1000
}

public static class DirectionExtension
{
    public static bool Has(this Direction a, Direction b)
    {
        return (a & b) != 0;
    }
}