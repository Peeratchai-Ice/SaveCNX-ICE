using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stage", menuName = "CMU Green Path/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Stage Info")]
    public int stageIndex = 1;           // ลำดับด่าน (1 - 5)
    public string stageName = "ชื่อด่าน";
    
    [TextArea(3, 5)]
    public string storyText = "เนื้อเรื่องบรีฟภารกิจ...";

    [TextArea(2, 4)]
    public string goalText = "เป้าหมายด่านนี้...";

    [TextArea(2, 4)]
    public string learningText = "สิ่งที่จะได้เรียนรู้ทางด้าน Algorithm...";

    [Header("Mission Buildings")]
    public BuildingNode startBuilding;   // ตึกจุดเริ่ม
    public BuildingNode goalBuilding;    // ตึกเป้าหมาย
    
    [Header("Star Requirements")]
    public float targetTimeMinutes = 15f; 
    public float targetCarbonGrams = 50f; 

    [Header("Map Layout Prefab")]
    public GameObject mapLayoutPrefab;  // แผนที่โหนดเฉพาะของด่านนี้
}