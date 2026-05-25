using System.Collections.Generic;
using UnityEngine;

public class CMU_GraphManager : MonoBehaviour
{
    public static CMU_GraphManager Instance;
    public List<BuildingNode> allBuildings = new List<BuildingNode>();

    private void Awake() 
    { 
        Instance = this; 
        
        // 🌟 เพิ่มบรรทัดนี้: ให้ระบบค้นหาตึกทั้งหมดบนฉากมารวมไว้ให้อัตโนมัติ
        allBuildings = new List<BuildingNode>(FindObjectsByType<BuildingNode>(FindObjectsSortMode.None));
    }
}