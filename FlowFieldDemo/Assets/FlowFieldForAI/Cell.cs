using UnityEngine;

public class Cell
{
    public Vector3 worldPosition;
    public Vector3Int gridIndex;

    public float distance = Mathf.Infinity;
    public Cell parent = null;
    public bool isWalkable = true;
    public float cost = 1f;
    public Vector3 flowDirection = Vector3.zero;

    public Cell(Vector3 worldPosition, Vector3Int gridIndex)
    {
        this.worldPosition = worldPosition;
        this.gridIndex = gridIndex;
    }

    public void ResetCell()
    {
        distance = Mathf.Infinity;
        flowDirection = Vector3.zero;
        parent = null;
    }
}