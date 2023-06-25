using System.Numerics;

namespace Proelium.Physics;

public readonly struct AABB
{
    public static readonly AABB All = new(new(float.MinValue), new(float.MaxValue));

    public readonly Vector2 min;
    public readonly Vector2 max;

    public float North => max.Y;
    public float East => max.X;
    public float South => min.Y;
    public float West => min.X;

    public AABB() { }

    public AABB(Vector2 min, Vector2 max)
    {
        this.min = min;
        this.max = max;
    }

    public AABB Offset(Vector2 offset)
    {
        return new(min + offset, max + offset);
    }

    public bool Overlaps(AABB other)
    {
        if (max.X < other.min.X || min.X > other.max.X)
        {
            return false;
        }
        if (max.Y < other.min.Y || min.Y > other.max.Y)
        {
            return false;
        }
        return true;
    }
}

public readonly struct Cell
{
    public readonly ushort x;
    public readonly ushort y;

    public Cell(ushort x, ushort y)
    {
        this.x = x;
        this.y = y;
    }

    public Cell Offset(int x, int y)
    {
        return new((ushort)(this.x + x), (ushort)(this.y + y));
    }

    public void GetSurrounding(ICollection<Cell> output)
    {
        output.Add(this);
        output.Add(Offset(0, 1));
        output.Add(Offset(1, 1));
        output.Add(Offset(1, 0));
        output.Add(Offset(1, -1));
        output.Add(Offset(0, -1));
        output.Add(Offset(-1, -1));
        output.Add(Offset(-1, 0));
        output.Add(Offset(-1, 1));
    }

    public override int GetHashCode()
    {
        return y << 16 | x;
    }

    public static bool operator ==(Cell a, Cell b)
    {
        return a.x == b.x && a.y == b.y;
    }
    public static bool operator !=(Cell a, Cell b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        return this == (Cell)obj;
    }
}

[Flags]
public enum Direction : byte
{
    None = 0b_0000,
    North = 0b_0001,
    East = 0b_0010,
    South = 0b_0100,
    West = 0b_1000
}

public static class DirectionExtension
{
    public static bool Has(this Direction a, Direction b)
    {
        return (a & b) != 0;
    }
}

[Flags]
public enum EntityFlags : byte
{
    None = 0b_0000_0000,
    Bounds = 0b_0000_0001,
    Collider = 0b_0000_0010,
    TriggerCollider = 0b_0000_0100,
    Drag = 0b_0000_1000,
    CanRepulseOthers = 0b_0001_0000,
    CanBeRepulsed = 0b_0010_0000,
    VelocityEpsilon = 0b_0100_0000,
    Static = 0b_1000_0000
}

public static class EntityFlagsExtension
{
    public static bool Has(this EntityFlags a, EntityFlags b)
    {
        return (a & b) != 0;
    }
}