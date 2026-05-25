using UnityEngine;
using TMPro; 

public class PlanningUI : MonoBehaviour
{
    public PathRunner pathRunner;
    public GameObject goButton; 
    public TextMeshProUGUI totalStatsText; 

    private BuildingNode currentNode;

    void Start()
    {
        if (goButton != null) goButton.SetActive(false);
        UpdateSummaryStats(); 
    }

    void Update()
    {
        if (Camera.main == null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.TryGetComponent<BuildingNode>(out BuildingNode hoveredNode))
        {
            BuildingNode currentPos = (currentNode != null) ? currentNode : pathRunner.startNode;

            if (currentPos != null)
            {
                if (hoveredNode == currentPos)
                {
                    // 🌟 เพิ่มคำว่า 'ตำแหน่งปัจจุบัน'
                    TooltipManager.Instance.ShowTooltip($"ตำแหน่ง: <color=#FFD166><b>{hoveredNode.buildingName}</b></color>\n<size=85%><color=#64DFDF>[ คุณอยู่ที่นี่ ]</color></size>");
                }
                else
                {
                    PathData path = currentPos.connectedPaths.Find(p => p.targetNode == hoveredNode);
                    
                    if (path != null)
                    {
                        string vehicleColorHex = ColorUtility.ToHtmlStringRGB(path.pathColor);
                        
                        // 🌟 เพิ่มบรรทัด 'มุ่งหน้าไป' ให้แสดงชื่อตึกด้านบนสุด
                        TooltipManager.Instance.ShowTooltip(
                            $"มุ่งหน้าไป: <color=#FFD166><b>{hoveredNode.buildingName}</b></color>\n" +
                            $"<size=90%>พาหนะ: <color=#{vehicleColorHex}><b>{path.vehicleName}</b></color>\n" +
                            $"ระยะทาง: <b>{path.distanceKm}</b> กม.\n" +
                            $"เวลา: <b>{path.timeMinutes}</b> นาที\n" +
                            $"คาร์บอน: <b>{path.carbonGrams}</b> gCO2</size>",
                            path.vehicleIcon 
                        );
                    }
                    else
                    {
                        // 🌟 เพิ่มคำว่า 'เป้าหมาย' สำหรับตึกที่ไปไม่ได้
                        TooltipManager.Instance.ShowTooltip($"เป้าหมาย: <color=#FFD166><b>{hoveredNode.buildingName}</b></color>\n<size=85%><color=#FF595E>[ ไม่มีถนนเชื่อมจากจุดปัจจุบัน ]</color></size>");
                    }
                }
            }
        }
        else
        {
            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
        }

        if (Input.GetMouseButtonDown(0) && goButton != null && !goButton.activeSelf)
        {
            if (hit.collider != null && hit.collider.TryGetComponent<BuildingNode>(out BuildingNode clickedNode))
            {
                TryRouteToNode(clickedNode);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            UndoMove();
        }
    }

    private void TryRouteToNode(BuildingNode targetNode)
    {
        if (currentNode == null) 
        {
            currentNode = pathRunner.startNode;
            if (currentNode == null) return;
        }

        PathData validPath = currentNode.connectedPaths.Find(p => p.targetNode == targetNode);

        if (validPath == null) return; 

        pathRunner.AddSegmentToPlan(validPath);
        currentNode = targetNode; 

        if (currentNode.isGoalNode && goButton != null) goButton.SetActive(true); 
        
        UpdateSummaryStats(); 
    }

    private void UndoMove()
    {
        if (pathRunner.plannedRoute.Count > 0)
        {
            currentNode = pathRunner.UndoLastSegment();

            if (!currentNode.isGoalNode && goButton != null) goButton.SetActive(false);
            
            UpdateSummaryStats(); 
        }
    }

    private void UpdateSummaryStats()
    {
        if (totalStatsText == null) return;

        int stepCount = pathRunner.plannedRoute.Count;

        if (stepCount == 0)
        {
            totalStatsText.text = "<b>เส้นทางของคุณ</b>\nยังไม่ได้เลือกจุดแวะ\n<size=80%><color=#FFD166>คลิกซ้ายเพื่อเดิน | คลิกขวาเพื่อย้อน</color></size>";
            return;
        }

        float totalTime = 0, totalCarbon = 0, totalDist = 0;

        foreach (var path in pathRunner.plannedRoute)
        {
            totalTime += path.timeMinutes;
            totalCarbon += path.carbonGrams;
            totalDist += path.distanceKm;
        }

        totalStatsText.text = $"ผ่านจุดแวะ: <b>{stepCount}</b> จุด\n" +
                              $"รวมระยะทาง: <b>{totalDist:F1}</b> กม.\n" +
                              $"เวลารวม: <b>{totalTime}</b> นาที\n" +
                              $"คาร์บอนรวม: <b>{totalCarbon}</b> gCO2\n" +
                              $"<size=80%><color=#FFD166>คลิกขวาเพื่อย้อนกลับ</color></size>";
    }

    public void OnClickGo()
    {
        if(goButton != null) goButton.SetActive(false);
        pathRunner.ExecutePlan();
    }
}