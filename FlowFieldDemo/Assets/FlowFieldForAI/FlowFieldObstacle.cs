using UnityEngine.Tilemaps;
using UnityEngine;

public class FlowFieldObstacle : MonoBehaviour
{
    [Header("Tile Coverage (supports floats)")]
    public Vector2[] tileCoverage;

    [Header("Tilemap Settings")]
    public Tilemap targetTilemap;
    public TileBase obstacleTile;

    [Header("Gizmo Settings")]
    public Color gizmoColor = new Color(1, 0, 0, 0.25f);
    public float cellSize = 1f;

    bool DontDrawInGame;

    private void Start()
    {
        DontDrawInGame = true;

        if (targetTilemap != null && obstacleTile != null && tileCoverage != null)
        {
            PlaceTilesOnMap();
        }
        else
        {
            Debug.LogWarning("Tilemap veya Tile atanmadı!");
        }
    }

    private void PlaceTilesOnMap()
    {
        Vector3Int baseCellPos = targetTilemap.WorldToCell(transform.position);

        foreach (Vector2 tile in tileCoverage)
        {
            Vector3 worldOffset = new Vector3(tile.x * cellSize, tile.y * cellSize, 0);
            Vector3 worldPos = transform.position + worldOffset;

            Vector3Int tilePosition = targetTilemap.WorldToCell(worldPos);

            targetTilemap.SetTile(tilePosition, obstacleTile);
        }

        targetTilemap.RefreshAllTiles();
    }

    private void OnDrawGizmos()
    {
        if (DontDrawInGame) return;
        if (tileCoverage == null || tileCoverage.Length == 0) return;

        Gizmos.color = gizmoColor;

        foreach (Vector2 tile in tileCoverage)
        {
            Vector3 cellCenter = transform.position + new Vector3(tile.x * cellSize, tile.y * cellSize, 0);

            Gizmos.DrawCube(cellCenter, new Vector3(cellSize, cellSize, 0.01f));
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, cellSize, 0.01f));

            Gizmos.color = gizmoColor;
        }
    }
}
