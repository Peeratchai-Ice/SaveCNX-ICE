using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RouteEvaluator : MonoBehaviour
{
    public static RouteEvaluator Instance;

    [Header("UI Objects")]
    public GameObject resultPanel;         

    [Header("4 Quadrants UI Tiles")]
    public TextMeshProUGUI fastestText;    // ช่อง 1: เร็วที่สุด 
    public TextMeshProUGUI shortestText;   // ช่อง 2: สั้นที่สุด
    public TextMeshProUGUI lowestText;     // ช่อง 3: คาร์บอนต่ำสุด
    public TextMeshProUGUI playerText;     // ช่อง 4: เส้นทางผู้เล่น

    private List<List<PathData>> allPossibleRoutes = new List<List<PathData>>();

    private void Awake()
    {
        Instance = this;
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void EvaluatePlayerRoute(BuildingNode startNode, BuildingNode goalNode, List<PathData> playerRoute)
    {
        allPossibleRoutes.Clear();
        
        List<PathData> currentRoute = new List<PathData>();
        HashSet<BuildingNode> visited = new HashSet<BuildingNode>() { startNode };
        FindAllRoutes(startNode, goalNode, currentRoute, visited);

        if (allPossibleRoutes.Count == 0) return;

        List<PathData> shortestRoute = allPossibleRoutes[0];
        List<PathData> fastestRoute = allPossibleRoutes[0];
        List<PathData> greenestRoute = allPossibleRoutes[0];

        float minDist = float.MaxValue, minTime = float.MaxValue, minCarbon = float.MaxValue;

        foreach (var route in allPossibleRoutes)
        {
            float d = 0, t = 0, c = 0;
            foreach (var p in route) { d += p.distanceKm; t += p.timeMinutes; c += p.carbonGrams; }

            if (d < minDist) { minDist = d; shortestRoute = route; }
            if (t < minTime) { minTime = t; fastestRoute = route; }
            if (c < minCarbon) { minCarbon = c; greenestRoute = route; }
        }

        if (resultPanel != null) resultPanel.SetActive(true);

        // ส่งข้อมูลอัปเดต (ปรับลำดับให้ตรงกับเลย์เอาต์ซ้าย-ขวา)
        UpdateTile(fastestText, "Fastest Way", fastestRoute, startNode);
        UpdateTile(shortestText, "Shortest Distance", shortestRoute, startNode);
        UpdateTile(lowestText, "Lowest Carbon", greenestRoute, startNode);
        UpdateTile(playerText, "Your Way", playerRoute, startNode);
    }

    private void FindAllRoutes(BuildingNode current, BuildingNode goal, List<PathData> currentRoute, HashSet<BuildingNode> visited)
    {
        if (current == goal) { allPossibleRoutes.Add(new List<PathData>(currentRoute)); return; }
        foreach (var path in current.connectedPaths)
        {
            if (path.targetNode != null && !visited.Contains(path.targetNode))
            {
                visited.Add(path.targetNode);
                currentRoute.Add(path);
                FindAllRoutes(path.targetNode, goal, currentRoute, visited);
                currentRoute.RemoveAt(currentRoute.Count - 1);
                visited.Remove(path.targetNode);
            }
        }
    }

    private void UpdateTile(TextMeshProUGUI uiText, string title, List<PathData> route, BuildingNode start)
    {
        if (uiText == null) return;

        float d = 0, t = 0, c = 0;
        string pathStr = start.buildingName;

        foreach (var p in route) 
        {
            d += p.distanceKm;
            t += p.timeMinutes;
            c += p.carbonGrams;
            pathStr += " -> " + p.targetNode.buildingName;
        }

        // 🌟 แก้ไขตรงนี้: จัดฟอร์แมตให้ประหยัดพื้นที่แนวตั้ง และลดขนาดฟอนต์ของชื่อตึก
        uiText.text = $"<color=#FFD166><size=110%><b>{title}</b></size></color>\n" +
                      $"เวลา: <b>{t}</b> นาที\n" +
                      $"ระยะทาง: <b>{d:F1}</b> กม.\n" +
                      $"คาร์บอน: <b>{c}</b> gCO2\n" +
                      $"<size=75%><color=#E0E0E0>{pathStr}</color></size>";
    }
}