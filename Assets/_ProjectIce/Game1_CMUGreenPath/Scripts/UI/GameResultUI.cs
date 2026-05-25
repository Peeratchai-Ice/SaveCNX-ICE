using UnityEngine;
using TMPro;

public class GameResultUI : MonoBehaviour
{
    public static GameResultUI Instance;
    public GameObject resultCanvas;

    public TextMeshProUGUI playerTimeText;
    public TextMeshProUGUI playerCarbonText;
    public TextMeshProUGUI ecoCarbonText;
    public TextMeshProUGUI expressTimeText;

    private void Awake() { Instance = this; }

    public void ShowFinalReport(BuildingNode startNode, BuildingNode endNode, float playerTime, float playerCarbon)
    {
        playerTimeText.text = $"{playerTime:F1} นาที";
        playerCarbonText.text = $"{playerCarbon:F0} gCO2";

        float bestCarbon = PathCostCalculate.Instance.FindOptimalCost(startNode, endNode, PathObjective.EcoCarbon);
        ecoCarbonText.text = $"ดีที่สุด: {bestCarbon:F0} gCO2";

        float bestTime = PathCostCalculate.Instance.FindOptimalCost(startNode, endNode, PathObjective.ExpressTime);
        expressTimeText.text = $"เร็วที่สุด: {bestTime:F1} นาที";

        if(resultCanvas != null) resultCanvas.SetActive(true); 
    }
}