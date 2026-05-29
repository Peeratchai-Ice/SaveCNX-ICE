using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RouteEvaluator : MonoBehaviour
{
    public static RouteEvaluator Instance;

    // ✨ 1. เพิ่มตัวแปรสำหรับระบุว่านี่คือด่านที่เท่าไหร่ (เพื่อเซฟเกมให้ถูกช่อง)
    [Header("Stage Progression (ระบบเซฟด่าน)")]
    [Tooltip("ระบุหมายเลขด่าน: 1, 2, 3, 4, 5")]
    public int currentStageIndex = 1; 

    [Header("UI Objects")]
    public GameObject resultPanel;         
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
            allPossibleRoutes.Sort((routeA, routeB) => 
            {
                float cA = 0, tA = 0, dA = 0;
                foreach(var p in routeA) { cA += p.carbonGrams; tA += p.timeMinutes; dA += p.distanceKm; }
                
                float cB = 0, tB = 0, dB = 0;
                foreach(var p in routeB) { cB += p.carbonGrams; tB += p.timeMinutes; dB += p.distanceKm; }

                if (Mathf.Abs(cA - cB) > 0.001f) return cA.CompareTo(cB); 
                if (Mathf.Abs(tA - tB) > 0.001f) return tA.CompareTo(tB);
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

        bool earnStar2 = playerTotalTime <= targetTimeMinutes;
        bool earnStar3 = playerTotalCarbon <= targetCarbonGrams;

        if (star1_Yellow != null) star1_Yellow.SetActive(earnStar1);
        if (star2_Yellow != null) star2_Yellow.SetActive(earnStar2);
        if (star3_Yellow != null) star3_Yellow.SetActive(earnStar3);

        // ✨ 2. ระบบเซฟข้อมูลส่งไปให้หน้า Stage Selection ของเพื่อน!
        int totalStars = 0;
        if (earnStar1) totalStars++;
        if (earnStar2) totalStars++;
        if (earnStar3) totalStars++;

        if (earnStar1) // ถ้าเป้าหมายสำเร็จ (เข้าเส้นชัยได้ ไม่ตาย ไม่พลาดจุดแวะ)
        {
            // โค้ดเพื่อนใช้ Index 0 สำหรับด่าน 1
            int friendStageIndex = currentStageIndex - 1; 

            // เซฟดาวสูงสุดที่ทำได้ (เซฟเสมอแม้ได้ 1 หรือ 2 ดาว เพื่อให้อัปเดตหน้าเมนู)
            string starKey = "Stage_" + friendStageIndex + "_Stars";
            int oldStars = PlayerPrefs.GetInt(starKey, 0);
            
            Debug.Log($"[SaveSystem] กำลังเซฟด่านที่: {currentStageIndex} | ดาวที่ได้: {totalStars} | ดาวเดิมที่มี: {oldStars}");

            if (totalStars > oldStars)
            {
                PlayerPrefs.SetInt(starKey, totalStars);
                Debug.Log($"✅ [SaveSystem] อัปเดตดาวสำเร็จ! เซฟ {totalStars} ดาว ลงในด่าน {currentStageIndex}");
            }

            // ✨ สั่งปลดล็อกด่านถัดไป "เฉพาะตอนที่ได้ 3 ดาวเป๊ะๆ"
            if (totalStars == 3)
            {
                string unlockKey = "Stage_" + friendStageIndex + "_Unlocked";
                PlayerPrefs.SetInt(unlockKey, 1);
                Debug.Log($"🔓 [SaveSystem] ได้ 3 ดาว! ปลดล็อกด่านถัดไปสำเร็จ!");
            }
            
            PlayerPrefs.Save();
        }

        if (resultPanel != null) resultPanel.SetActive(true);
        
        // ✨ แก้ไขปุ่ม Next Stage ให้โชว์เฉพาะตอนที่ได้ครบ 3 ดาวเท่านั้น!
        if (nextStageButton != null) 
        {
            nextStageButton.SetActive(totalStars == 3);
        }

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