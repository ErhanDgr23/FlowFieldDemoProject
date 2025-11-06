using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FlowFieldController : MonoBehaviour
{
    public static FlowFieldController Instance { get; private set; }
    public event Action OnFlowFieldCreated;

    [Header("Grid Settings")]
    public int width = 20, height = 20;
    public float cellSize = 1f;
    public Vector3 origin = Vector3.zero;

    [SerializeField] private float recalcInterval = 0.5f;

    [Header("Target")]
    public Transform target;

    [Header("Tile/Obstacle Settings")]
    public Tilemap tilemap;
    public TileCostData tileCostData;

    [Header("Debug")]
    public bool drawDebug = true;
    public Color gridColor = new Color(0f, 1f, 0f, 0.1f);
    public Color flowColor = new Color(0f, 1f, 1f, 0.1f);
    public Color blockedColor = new Color(1f, 0f, 0f, 0.1f);

    [HideInInspector] public FlowFieldGrid grid;

    private Vector3 lastTargetPos;
    private float nextRecalcTime = 0f;
    private bool costsCalculated = false;

    private void Awake() => Instance = this;

    public void CreateFlowField(FlowAgent agent = null)
    {
        if (target == null)
        {
            Debug.LogWarning("Target missing");
            return;
        }
        // Delte Old Grid
        ClearFlowField();
        grid = new FlowFieldGrid(width, height, cellSize, origin);
        float radius = agent != null ? agent.radius : 0f;
        SetCellCosts(radius);
        costsCalculated = true;
        GenerateFlowField(target.position);
        lastTargetPos = target.position;
        OnFlowFieldCreated?.Invoke();
    }

    public void ClearFlowField()
    {
        grid = null;
        costsCalculated = false;
        //agents.Clear();
    }

    // Harita güncellendiğinde çağır (örneğin tilemap değiştiğinde, flowfield taşındığında)
    public void RecreateFlowField()
    {
        CreateFlowField();
    }

    void LateUpdate()
    {
        if (grid == null || target == null) return;
        if (Time.time >= nextRecalcTime)
        {
            Vector3 diff = target.position - lastTargetPos;
            if (diff.sqrMagnitude > 0.01f)
            {
                UpdateTargetCell(target.position);
                lastTargetPos = target.position;
            }
            nextRecalcTime = Time.time + recalcInterval;
        }
    }

    private void UpdateTargetCell(Vector3 targetPos)
    {
        if (grid == null) return;

        foreach (var kvp in grid.grid)
        {
            Cell c = kvp.Value;
            c.distance = Mathf.Infinity;
            c.parent = null;
        }

        Cell targetCell = grid.GetCell(targetPos);
        if (targetCell == null || !targetCell.isWalkable) return;

        targetCell.distance = 0;

        Queue<Cell> queue = new Queue<Cell>();
        queue.Enqueue(targetCell);

        while (queue.Count > 0)
        {
            Cell current = queue.Dequeue();

            foreach (Cell n in grid.GetNeighbours(current))
            {
                if (!n.isWalkable) continue;

                bool isDiagonal = n.gridIndex.x != current.gridIndex.x && n.gridIndex.y != current.gridIndex.y;
                if (isDiagonal)
                {
                    Cell nX = grid.GetCell(n.gridIndex.x, current.gridIndex.y);
                    Cell nY = grid.GetCell(current.gridIndex.x, n.gridIndex.y);
                    if ((nX != null && !nX.isWalkable) || (nY != null && !nY.isWalkable))
                        continue;
                }

                float newDist = current.distance + n.cost;
                if (newDist < n.distance)
                {
                    n.distance = newDist;
                    n.parent = current;
                    queue.Enqueue(n);
                }
            }
        }

        CalculateFlowDirections();
    }

    private void GenerateFlowField(Vector3 targetPos)
    {
        if (!costsCalculated)
        {
            SetCellCosts();
            costsCalculated = true;
        }

        foreach (var kvp in grid.grid)
        {
            Cell c = kvp.Value;
            c.distance = Mathf.Infinity;
            c.parent = null;
        }

        Cell targetCell = grid.GetCell(targetPos);
        if (targetCell == null || !targetCell.isWalkable) return;
        targetCell.distance = 0;

        Queue<Cell> queue = new Queue<Cell>();
        queue.Enqueue(targetCell);

        while (queue.Count > 0)
        {
            Cell current = queue.Dequeue();

            foreach (Cell n in grid.GetNeighbours(current))
            {
                if (!n.isWalkable) continue;

                bool isDiagonal = n.gridIndex.x != current.gridIndex.x && n.gridIndex.y != current.gridIndex.y;
                if (isDiagonal)
                {
                    Cell nX = grid.GetCell(n.gridIndex.x, current.gridIndex.y);
                    Cell nY = grid.GetCell(current.gridIndex.x, n.gridIndex.y);
                    if ((nX != null && !nX.isWalkable) || (nY != null && !nY.isWalkable))
                        continue;
                }

                float newDist = current.distance + n.cost;
                if (newDist < n.distance)
                {
                    n.distance = newDist;
                    n.parent = current;
                    queue.Enqueue(n);
                }
            }
        }

        CalculateFlowDirections();
    }

    private void SetCellCosts(float agentRadius = 0f)
    {
        foreach (var kvp in grid.grid)
        {
            Cell cell = kvp.Value;
            Vector3 pos = cell.worldPosition;

            cell.isWalkable = true;
            cell.cost = 1f;

            if (tilemap != null && tileCostData != null)
            {
                Vector3Int tilePos = tilemap.WorldToCell(pos);
                TileBase tile = tilemap.GetTile(tilePos);

                if (tile != null)
                {
                    foreach (var tc in tileCostData.tileCosts)
                    {
                        if (tc.tile == tile)
                        {
                            cell.cost = tc.cost;
                            cell.isWalkable = tc.cost <= 1000f;
                            break;
                        }
                    }
                }
            }

            if (agentRadius > 0f && cell.isWalkable)
            {
                // Radiusun 2 katı kadar çevre kontrolü (dar geçitleri önlemek için) !dar geçitlerden agentlar geçiyor şu anlık
                int radiusInCells = Mathf.CeilToInt(agentRadius * 2f / grid.cellSize);  // 2*radius, daha geniş
                for (int x = -radiusInCells; x <= radiusInCells; x++)
                {
                    for (int y = -radiusInCells; y <= radiusInCells; y++)
                    {
                        if (x == 0 && y == 0) continue;

                        Cell n = grid.GetCell(cell.gridIndex.x + x, cell.gridIndex.y + y);
                        if (n != null && !n.isWalkable)
                        {
                            cell.isWalkable = false;
                            cell.cost = 1000f;
                            break;
                        }
                    }
                    if (!cell.isWalkable)
                        break;
                }
            }
        }
    }

    private void CalculateFlowDirections()
    {
        if (grid == null) return;

        foreach (var kvp in grid.grid)
        {
            Cell c = kvp.Value;

            if (!c.isWalkable || c.parent == null)
            {
                c.flowDirection = Vector3.zero;
                continue;
            }

            c.flowDirection = (c.parent.worldPosition - c.worldPosition).normalized;
        }
    }

    public Vector3 GetFlowDirectionAtPosition(Vector3 pos)
    {
        if (grid == null) return Vector3.zero;
        Cell c = grid.GetCell(pos);

        if (c == null) return Vector3.zero;

        if (!c.isWalkable)
        {
            foreach (var n in grid.GetNeighbours(c))
            {
                if (n.isWalkable)
                    return (n.worldPosition - pos).normalized;
            }
            return Vector3.zero;
        }

        return c.flowDirection;
    }

    private void OnDrawGizmos()
    {
        if (grid == null || !drawDebug) return;
        foreach (var kvp in grid.grid)
        {
            Cell c = kvp.Value;
            if (c == null) continue;
            Gizmos.color = c.isWalkable ? gridColor : blockedColor;
            Gizmos.DrawCube(c.worldPosition, Vector3.one * grid.cellSize * 0.9f);
            if (c.flowDirection != Vector3.zero && c.isWalkable)
            {
                Gizmos.color = flowColor;
                Gizmos.DrawRay(c.worldPosition, c.flowDirection * grid.cellSize * 0.4f);
            }
        }
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(target.position, grid.cellSize * 0.3f);
        }
    }
}