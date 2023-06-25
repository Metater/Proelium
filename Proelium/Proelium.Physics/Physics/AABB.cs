using System.Numerics;

namespace Proelium.Server.Physics;

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
