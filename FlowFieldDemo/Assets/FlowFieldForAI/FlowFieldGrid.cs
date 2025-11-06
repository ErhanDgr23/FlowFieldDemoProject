using System.Collections.Generic;
using UnityEngine;

public class FlowFieldGrid
{
    public int width, height;
    public float cellSize;
    public Vector3 origin;

    public Dictionary<Vector2Int, Cell> grid = new Dictionary<Vector2Int, Cell>();

    public FlowFieldGrid(int width, int height, float cellSize, Vector3 origin)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int key = new Vector2Int(x, y);
                Vector3 worldPos = GetWorldPosition(x, y);
                grid[key] = new Cell(worldPos, new Vector3Int(x, y, 0));
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return origin + new Vector3(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2, 0);
    }

    public Cell GetCell(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - origin.y) / cellSize);
        Vector2Int key = new Vector2Int(x, y);
        return grid.TryGetValue(key, out var cell) ? cell : null;
    }

    public Cell GetCell(int x, int y)
    {
        Vector2Int key = new Vector2Int(x, y);
        return grid.TryGetValue(key, out var cell) ? cell : null;
    }

    public List<Cell> GetNeighbours(Cell cell)
    {
        List<Cell> neighbours = new List<Cell>();
        // Tüm yönler: kardinal + diagonal
        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        foreach (var dir in dirs)
        {
            Vector2Int key = new Vector2Int(cell.gridIndex.x + dir.x, cell.gridIndex.y + dir.y);
            if (grid.TryGetValue(key, out var neighbour))
                neighbours.Add(neighbour);
        }

        return neighbours;
    }
}