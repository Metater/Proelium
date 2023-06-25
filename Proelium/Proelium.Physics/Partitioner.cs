using System.Numerics;

namespace Proelium.Physics;

public class Partitioner
{
    public float CellSize { get; init; }
    public float HalfCellSize { get; init; }
    public float DimensionLength { get; init; }
    public Vector2 WorldCenter { get; init; }

    public Partitioner(float cellSize)
    {
        CellSize = cellSize;
        HalfCellSize = cellSize / 2f;
        DimensionLength = ushort.MaxValue + 1f;
        WorldCenter = new(DimensionLength / 2f);
    }

    public Cell GetCell(Vector2 position)
    {
        return new(GetCellDimension(position.X), GetCellDimension(position.Y));
    }
    public ushort GetCellDimension(float position)
    {
        return (ushort)(position / CellSize);
    }

    public Vector2 GetCellCenterPosition(Cell cell)
    {
        Vector2 position = GetCellPosition(cell);
        return new(position.X + HalfCellSize, position.Y + HalfCellSize);
    }
    public Vector2 GetCellPosition(Cell cell)
    {
        return new(cell.x * CellSize, cell.y * CellSize);
    }

    public void GetCellsInAABB(AABB aabb, ICollection<Cell> output)
    {
        ushort north = (ushort)MathF.Ceiling(aabb.North / CellSize);
        ushort east = (ushort)MathF.Ceiling(aabb.East / CellSize);
        ushort south = (ushort)MathF.Floor(aabb.South / CellSize);
        ushort west = (ushort)MathF.Floor(aabb.West / CellSize);

        for (ushort y = south; y <= north; y++)
        {
            for (ushort x = west; x <= east; x++)
            {
                output.Add(new(x, y));
            }
        }
    }
}