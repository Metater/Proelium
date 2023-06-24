namespace Proelium.Server.Physics;

public struct Cell
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
        return (y << 16) | x;
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