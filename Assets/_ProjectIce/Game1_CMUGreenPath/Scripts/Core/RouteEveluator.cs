using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RouteEvaluator : MonoBehaviour
{
    public static RouteEvaluator Instance;

    [Header("UI Objects")]
    public GameObject resultPanel;         

    [Header("UI Tiles (โชว์ 2 ช่อง)")]
    public TextMeshProUGUI bestApproachText; 
    public TextMeshProUGUI playerText;       

    [Header("Star Rating UI")]
    public GameObject star1_Yellow; 
    public GameObject star2_Yellow; 
    public GameObject star3_Yellow; 

    [Header("Stage Conditions (เกณฑ์ผ่านด่าน)")]
    public float targetTimeMinutes = 60f;  
    public float targetCarbonGrams = 100f; 
    // ✨ เพิ่มช่องกำหนดระยะทางสูงสุด
    public float targetDistanceKm = 999f; 

    private List<List<PathData>> allPossibleRoutes = new List<List<PathData>>();

    private void Awake()
    {
        Instance = this;
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void EvaluatePlayerRoute(BuildingNode startNode, BuildingNode goalNode, List<PathData> playerRoute, bool hasPickedUpFriend = true, string missingFriendReason = "", bool hitDeathNode = false, string deathReason = "")
    {
        allPossibleRoutes.Clear();
        
        List<PathData> currentRoute = new List<PathData>();
        HashSet<BuildingNode> visited = new HashSet<BuildingNode>() { startNode };
        FindAllRoutes(startNode, goalNode, currentRoute, visited);

        allPossibleRoutes.RemoveAll(route => route.Exists(p => p.targetNode.nodeRole == BuildingNode.NodeRole.DeathNode));

        BuildingNode pickupNode = null;
        foreach (BuildingNode node in FindObjectsOfType<BuildingNode>())
        {
            if (node.nodeRole == BuildingNode.NodeRole.PickupNode) pickupNode = node;
        }

        if (pickupNode != null)
        {
            allPossibleRoutes.RemoveAll(route => !route.Exists(p => p.targetNode == pickupNode));
        }

        if (allPossibleRoutes.Count == 0) return;

        List<PathData> bestRoute = allPossibleRoutes[0];
        float minCarbon = float.MaxValue;
        float minTimeForBest = float.MaxValue;

        foreach (var route in allPossibleRoutes)
        {
            float t = 0, c = 0, d = 0;
            foreach (var p in route) { t += p.timeMinutes; c += p.carbonGrams; d += p.distanceKm; }

            if (c < minCarbon || (c == minCarbon && t < minTimeForBest)) 
            { 
                minCarbon = c; 
                minTimeForBest = t;
                bestRoute = route; 
            }
        }

        float playerTotalTime = 0f;
        float playerTotalCarbon = 0f;
        float playerTotalDistance = 0f; // ✨ ตัวแปรเก็บระยะทางที่ผู้เล่นเดิน

        foreach (var p in playerRoute) 
        {
            playerTotalTime += p.timeMinutes;
            playerTotalCarbon += p.carbonGrams;
            playerTotalDistance += p.distanceKm; // ✨ บวกระยะทางสะสม
        }

        bool earnStar1 = true;
        string penaltyMessage = "";

        if (!string.IsNullOrEmpty(missingFriendReason) && !hasPickedUpFriend) 
        {
            earnStar1 = false; 
            penaltyMessage += $"<color=#FF4D4D><size=85%><b>* {missingFriendReason}</b></size></color>\n";
        }

        if (hitDeathNode)
        {
            earnStar1 = false;
            penaltyMessage += $"<color=#FF4D4D><size=85%><b>* {deathReason}</b></size></color>\n";
        }

        // ✨ เช็คเงื่อนไขระยะทางเกินหรือไม่
        if (playerTotalDistance > targetDistanceKm)
        {
            earnStar1 = false;
            penaltyMessage += $"<color=#FF4D4D><size=85%><b>* ระยะทางเกินกำหนด (ห้ามเกิน {targetDistanceKm} กม.)</b></size></color>\n";
        }

        if (star1_Yellow != null) star1_Yellow.SetActive(earnStar1);
        if (star2_Yellow != null) star2_Yellow.SetActive(playerTotalTime <= targetTimeMinutes);
        if (star3_Yellow != null) star3_Yellow.SetActive(playerTotalCarbon <= targetCarbonGrams);

        if (resultPanel != null) resultPanel.SetActive(true);

        UpdateTile(bestApproachText, "Best Approach", bestRoute, startNode);
        UpdateTile(playerText, "Your Way", playerRoute, startNode);

        if (!earnStar1 && playerText != null && !string.IsNullOrEmpty(penaltyMessage))
        {
            playerText.text += $"\n{penaltyMessage}<color=#FF4D4D><size=85%><b>(ถูกยึดดาว 1 ดวง)</b></size></color>";
        }
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

        uiText.text = $"<color=#FFD166><size=110%><b>{title}</b></size></color>\n" +
                      $"เวลา: <b>{t}</b> นาที\n" +
                      $"ระยะทาง: <b>{d:F1}</b> กม.\n" +
                      $"คาร์บอน: <b>{c}</b> gCO2\n" +
                      $"<size=75%><color=#E0E0E0>{pathStr}</color></size>";
    }

    public void HandleTimeOut()
    {
        if (star1_Yellow != null) star1_Yellow.SetActive(false);
        if (star2_Yellow != null) star2_Yellow.SetActive(false);
        if (star3_Yellow != null) star3_Yellow.SetActive(false);

        if (resultPanel != null) resultPanel.SetActive(true);

        if (playerText != null)
        {
            playerText.text = "<color=#FF4D4D><size=120%><b>GAME OVER</b></size></color>\n\n" +
                              "<color=#FFF>คุณวางแผนเส้นทาง\n<size=110%><b>ไม่ทันเวลา!</b></size></color>";
        }
    }
}