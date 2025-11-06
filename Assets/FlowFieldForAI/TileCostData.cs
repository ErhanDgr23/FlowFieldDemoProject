using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

[CreateAssetMenu(menuName = "FlowField/Tile Cost Data", fileName = "NewTileCostData")]
public class TileCostData : ScriptableObject
{
    [System.Serializable]
    public class TileCost
    {
        public TileBase tile;
        public float cost = 1f;
    }

    public List<TileCost> tileCosts = new List<TileCost>();
}