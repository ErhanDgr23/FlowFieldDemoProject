using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlowFieldTerrainData", menuName = "FlowField/Terrain Data")]
public class FlowFieldTerrainData : ScriptableObject
{
    [System.Serializable]
    public class TerrainCost
    {
        public string tileName;
        public float cost = 1f;
    }

    [Header("Tile Costları")]
    public List<TerrainCost> tileCosts = new List<TerrainCost>();

    [Header("Prefab Etkisi")]
    [Tooltip("Objects with this tag make the cell inaccessible.")]
    public string blockingTag = "Enviroment";

    [Tooltip("Cost value of unreachable cells.")]
    public float unwalkableCost = 9999f;

    private Dictionary<string, float> costLookup;

    // Runtime’da hızlı lookup için sözlük oluşturur
    public void Initialize()
    {
        costLookup = new Dictionary<string, float>();
        foreach (var tc in tileCosts)
        {
            if (!costLookup.ContainsKey(tc.tileName))
                costLookup.Add(tc.tileName, tc.cost);
        }
    }

    public float GetCostForTile(string tileName)
    {
        if (costLookup == null) Initialize();
        return costLookup.TryGetValue(tileName, out float c) ? c : 1f;
    }
}
