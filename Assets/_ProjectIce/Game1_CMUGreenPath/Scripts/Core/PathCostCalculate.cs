using System.Collections.Generic;
using UnityEngine;

public enum PathObjective { EcoCarbon, ExpressTime }

public class PathCostCalculate : MonoBehaviour
{
    public static PathCostCalculate Instance;
    private void Awake() { Instance = this; }

    public float FindOptimalCost(BuildingNode start, BuildingNode end, PathObjective objective)
    {
        Dictionary<BuildingNode, float> costTable = new Dictionary<BuildingNode, float>();
        List<BuildingNode> unvisited = new List<BuildingNode>(CMU_GraphManager.Instance.allBuildings);

        foreach (var node in unvisited) costTable[node] = float.MaxValue;
        costTable[start] = 0f;

        while (unvisited.Count > 0)
        {
            unvisited.Sort((x, y) => costTable[x].CompareTo(costTable[y]));
            BuildingNode current = unvisited[0];
            unvisited.Remove(current);

            if (current == end || costTable[current] == float.MaxValue) break;

            foreach (PathData path in current.connectedPaths)
            {
                if (!unvisited.Contains(path.targetNode)) continue;

                // ดึงตัวเลขจากที่คุณกรอกมือมาคำนวณ
                float edgeWeight = (objective == PathObjective.EcoCarbon) ? path.carbonGrams : path.timeMinutes;

                float altCost = costTable[current] + edgeWeight;
                if (altCost < costTable[path.targetNode])
                {
                    costTable[path.targetNode] = altCost;
                }
            }
        }
        return costTable[end];
    }
}