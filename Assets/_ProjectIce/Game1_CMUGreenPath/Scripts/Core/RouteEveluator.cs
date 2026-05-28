using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RouteEvaluator : MonoBehaviour
{
    public static RouteEvaluator Instance;

    [Header("UI Objects")]
    public GameObject resultPanel;         

    [Header("Navigation UI")]
    public GameObject nextStageButton;

    [Header("Result Screen Backgrounds")]
    public Image resultBackgroundImage; 
    public Sprite victorySprite;        
    public Sprite gameOverSprite;       

    [Header("UI Tiles - Best Approach")]
    public TextMeshProUGUI bestTimeText;
    public TextMeshProUGUI bestDistText;
    public TextMeshProUGUI bestCarbonText;
    public TextMeshProUGUI bestPathText; 

    [Header("UI Tiles - Your Way")]
    public TextMeshProUGUI playerTimeText;
    public TextMeshProUGUI playerDistText;
    public TextMeshProUGUI playerCarbonText;
    public TextMeshProUGUI playerPathText; 

    [Header("UI Tiles - Warnings / Game Over")]
    public TextMeshProUGUI failReasonText; 

    [Header("Star Rating UI")]
    public GameObject star1_Yellow; 
    public GameObject star2_Yellow; 
    public GameObject star3_Yellow; 

    [Header("Stage Conditions (เกณฑ์ผ่านด่าน)")]
    public float targetTimeMinutes = 60f;  
    public float targetCarbonGrams = 100f; 
    public float targetDistanceKm = 999f; 

    private List<List<PathData>> allPossibleRoutes = new List<List<PathData>>();

    private void Awake()
    {
        Instance = this;
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    public void EvaluatePlayerRoute(BuildingNode startNode, BuildingNode goalNode, List<PathData> playerRoute, bool hasPickedUpFriend = true, string missingFriendReason = "", bool hitDeathNode = false, string deathReason = "")
    {
        if (hitDeathNode)
        {
            ShowGameOverScreen(deathReason);
            return;
        }

        if (resultBackgroundImage != null && victorySprite != null)
        {
            resultBackgroundImage.sprite = victorySprite;
        }

        allPossibleRoutes.Clear();
        List<PathData> currentRoute = new List<PathData>();
        HashSet<BuildingNode> visited = new HashSet<BuildingNode>() { startNode };
        
        FindAllRoutes(startNode, goalNode, currentRoute, visited);

        List<BuildingNode> pickupNodes = new List<BuildingNode>();
        List<BuildingNode> deathNodes = new List<BuildingNode>();

        foreach (BuildingNode node in FindObjectsOfType<BuildingNode>())
        {
            if (node.nodeRole == BuildingNode.NodeRole.PickupNode) pickupNodes.Add(node);
            if (node.nodeRole == BuildingNode.NodeRole.DeathNode) deathNodes.Add(node);
        }

        allPossibleRoutes.RemoveAll(route => 
        {
            foreach(var dNode in deathNodes) {
                if (route.Exists(p => p.targetNode == dNode)) return true; 
            }
            foreach(var pNode in pickupNodes) {
                if (startNode == pNode) continue;
                if (!route.Exists(p => p.targetNode == pNode)) return true; 
            }
            return false; 
        });

        if (allPossibleRoutes.Count == 0)
        {
            if (bestTimeText != null) bestTimeText.text = "-";
            if (bestDistText != null) bestDistText.text = "-";
            if (bestCarbonText != null) bestCarbonText.text = "-";
            if (bestPathText != null) bestPathText.text = "<color=#C0392B>AI ไม่พบเส้นทางที่ปลอดภัยและแวะรับเพื่อนได้</color>";
        }
        else
        {
            // ✨ อัปเกรดความฉลาด AI ให้รู้จักเปรียบเทียบระยะทางด้วย!
            allPossibleRoutes.Sort((routeA, routeB) => 
            {
                float cA = 0, tA = 0, dA = 0;
                foreach(var p in routeA) { cA += p.carbonGrams; tA += p.timeMinutes; dA += p.distanceKm; }
                
                float cB = 0, tB = 0, dB = 0;
                foreach(var p in routeB) { cB += p.carbonGrams; tB += p.timeMinutes; dB += p.distanceKm; }

                // กฎข้อ 1: คาร์บอนน้อยกว่า ชนะ
                if (Mathf.Abs(cA - cB) > 0.001f) return cA.CompareTo(cB); 
                
                // กฎข้อ 2: ถ้าคาร์บอนเท่ากัน ให้เวลาเร็วกว่า ชนะ
                if (Mathf.Abs(tA - tB) > 0.001f) return tA.CompareTo(tB);

                // ✨ กฎข้อ 3: ถ้าเวลาเท่ากันอีก ให้ระยะทางสั้นกว่า ชนะ!
                return dA.CompareTo(dB);
            });

            List<PathData> bestRoute = allPossibleRoutes[0];
            float bestRouteTime = 0f, bestRouteDist = 0f, bestRouteCarbon = 0f;
            foreach (var p in bestRoute) 
            { 
                bestRouteTime += p.timeMinutes; 
                bestRouteDist += p.distanceKm; 
                bestRouteCarbon += p.carbonGrams; 
            }

            if (bestTimeText != null) bestTimeText.text = bestRouteTime.ToString("0.##");
            if (bestDistText != null) bestDistText.text = bestRouteDist.ToString("0.##");
            if (bestCarbonText != null) bestCarbonText.text = bestRouteCarbon.ToString("0.##");
            if (bestPathText != null)
            {
                string bestPathStr = string.IsNullOrEmpty(startNode.buildingName) ? "[ไม่ระบุชื่อ]" : startNode.buildingName;
                foreach (var p in bestRoute) 
                {
                    string targetName = string.IsNullOrEmpty(p.targetNode.buildingName) ? "[ไม่ระบุชื่อ]" : p.targetNode.buildingName;
                    bestPathStr += $" -> {targetName}";
                }
                bestPathText.text = bestPathStr;
            }
        }

        float playerTotalTime = 0f;
        float playerTotalCarbon = 0f;
        float playerTotalDistance = 0f;
        foreach (var p in playerRoute) 
        {
            playerTotalTime += p.timeMinutes;
            playerTotalCarbon += p.carbonGrams;
            playerTotalDistance += p.distanceKm;
        }

        bool earnStar1 = true;
        string penaltyMessage = "";

        if (!string.IsNullOrEmpty(missingFriendReason) && !hasPickedUpFriend) 
        {
            earnStar1 = false; 
            penaltyMessage += $"* {missingFriendReason}\n";
        }

        if (playerTotalDistance > targetDistanceKm)
        {
            earnStar1 = false;
            penaltyMessage += $"* ระยะทางเกิน (ห้ามเกิน {targetDistanceKm} กม.)\n";
        }

        if (star1_Yellow != null) star1_Yellow.SetActive(earnStar1);
        if (star2_Yellow != null) star2_Yellow.SetActive(playerTotalTime <= targetTimeMinutes);
        if (star3_Yellow != null) star3_Yellow.SetActive(playerTotalCarbon <= targetCarbonGrams);

        if (resultPanel != null) resultPanel.SetActive(true);
        if (nextStageButton != null) nextStageButton.SetActive(true);

        if (playerTimeText != null) playerTimeText.text = playerTotalTime.ToString("0.##");
        if (playerDistText != null) playerDistText.text = playerTotalDistance.ToString("0.##");
        if (playerCarbonText != null) playerCarbonText.text = playerTotalCarbon.ToString("0.##");
        
        if (playerPathText != null)
        {
            string playerPathStr = string.IsNullOrEmpty(startNode.buildingName) ? "[ไม่ระบุชื่อ]" : startNode.buildingName;
            foreach (var p in playerRoute) 
            {
                string targetName = string.IsNullOrEmpty(p.targetNode.buildingName) ? "[ไม่ระบุชื่อ]" : p.targetNode.buildingName;
                playerPathStr += $" -> {targetName}";
            }
            playerPathText.text = playerPathStr;
        }

        if (failReasonText != null)
        {
            if (!earnStar1 && !string.IsNullOrEmpty(penaltyMessage))
                failReasonText.text = $"<color=#C0392B><b>{penaltyMessage}(ถูกยึดดาว 1 ดวง)</b></color>";
            else
                failReasonText.text = ""; 
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

    public void HandleTimeOut()
    {
        ShowGameOverScreen("หมดเวลาตัดสินใจ!");
    }

    public void ShowGameOverScreen(string reason)
    {
        if (resultBackgroundImage != null && gameOverSprite != null)
        {
            resultBackgroundImage.sprite = gameOverSprite;
        }

        if (star1_Yellow != null) star1_Yellow.SetActive(false);
        if (star2_Yellow != null) star2_Yellow.SetActive(false);
        if (star3_Yellow != null) star3_Yellow.SetActive(false);

        if (resultPanel != null) resultPanel.SetActive(true);
        if (nextStageButton != null) nextStageButton.SetActive(false);

        if (bestTimeText != null) bestTimeText.text = "-";
        if (bestDistText != null) bestDistText.text = "-";
        if (bestCarbonText != null) bestCarbonText.text = "-";
        if (bestPathText != null) bestPathText.text = "";

        if (playerTimeText != null) playerTimeText.text = "-";
        if (playerDistText != null) playerDistText.text = "-";
        if (playerCarbonText != null) playerCarbonText.text = "-";
        if (playerPathText != null) playerPathText.text = "";

        if (failReasonText != null)
        {
            failReasonText.text = $"<color=#C0392B><size=120%><b>GAME OVER</b></size>\nสาเหตุ: {reason}</color>";
        }
    }

    public void RetryStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}