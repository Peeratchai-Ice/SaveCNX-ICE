using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StageManager : MonoBehaviour
{
    [Header("Stage Database")]
    [Tooltip("ใส่ไฟล์ Stage_01 ถึง Stage_05 ไว้ที่นี่")]
    public List<StageData> allStages; 
    
    [HideInInspector]
    public StageData currentStage; // ข้อมูลด่านปัจจุบันที่ระบบสุ่มมาใช้

    // ✨ 1. สิ่งที่เพิ่มเข้ามา: ช่องสำหรับรับตัวละคร Player
    [Header("Player Setup")]
    public GameObject playerObject; 

    [Header("UI Elements")]
    public GameObject briefingPanel;
    public TextMeshProUGUI stageNameText;
    public TextMeshProUGUI storyText;
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI starConditionText;
    public TextMeshProUGUI learningText;

   void Start()
    {
        // 1. จำลองการรับค่าจากหน้าเลือกด่าน
        int selectedStageNumber = PlayerPrefs.GetInt("SelectedStage", 1); 
        int stageIndex = selectedStageNumber - 1; 

        // 2. ดึงข้อมูลด่านที่เลือกมาใช้งาน
        if (stageIndex >= 0 && stageIndex < allStages.Count)
        {
            currentStage = allStages[stageIndex];
        }

        // 3. เสกแผนที่ (Map Prefab) ของด่านนั้นๆ ลงมาในฉาก
        if (currentStage != null && currentStage.mapLayoutPrefab != null)
        {
            GameObject spawnedMap = Instantiate(currentStage.mapLayoutPrefab);
            
            // ค้นหาตึกเริ่มต้น
            Transform startNodeTransform = spawnedMap.transform.Find("Campus_Building/Node_Start");
            
            if (startNodeTransform != null && playerObject != null)
            {
                // ✨ 3.1 วาร์ปตัวละครไปยืนที่ตึก
                playerObject.transform.position = startNodeTransform.position;
                
                // ✨ 3.2 กระซิบบอกสคริปต์ PathRunner ว่าตึกนี้คือ startNode!
                PathRunner runnerScript = playerObject.GetComponent<PathRunner>();
                if (runnerScript != null)
                {
                    runnerScript.startNode = startNodeTransform.GetComponent<BuildingNode>();
                }
            }
        }

        // 4. ตั้งค่า UI และหยุดเวลารอผู้เล่นกด Start
        SetupBriefingUI();
        briefingPanel.SetActive(true);
        Time.timeScale = 0f; 
    }

    void SetupBriefingUI()
    {
        if (currentStage == null) return;

        stageNameText.text = currentStage.stageName;
        storyText.text = currentStage.storyText;
        goalText.text = currentStage.goalText;
        learningText.text = currentStage.learningText;
        starConditionText.text = $" เดินทางถึงเป้าหมายสำเร็จ\n ใช้เวลา \u2264 {currentStage.targetTimeMinutes} นาที\n ปล่อยคาร์บอน \u2264 {currentStage.targetCarbonGrams} กรัม";
    }

    public void StartMission()
    {
        briefingPanel.SetActive(false);
        Time.timeScale = 1f; 
    }
}