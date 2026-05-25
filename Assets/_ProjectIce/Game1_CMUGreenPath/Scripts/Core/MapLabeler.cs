using UnityEngine;
using TMPro;

public class MapLabeler : MonoBehaviour
{
    public GameObject textLabelPrefab; 
    
    void Start()
    {
        // รอให้ระบบ Map ลงทะเบียนตึกเสร็จเรียบร้อยก่อน 0.5 วินาทีจึงค่อยวาดป้าย
        Invoke("DrawLabels", 0.5f); 
    }

    void DrawLabels()
    {
        if (textLabelPrefab == null || CMU_GraphManager.Instance == null) return;

        foreach (BuildingNode node in CMU_GraphManager.Instance.allBuildings)
        {
            foreach (PathData path in node.connectedPaths)
            {
                // วาดป้ายเฉพาะเส้นขาไป เพื่อไม่ให้ป้ายขาไป-ขากลับวางซ้อนทับกันเอง
                if (node.GetInstanceID() < path.targetNode.GetInstanceID())
                {
                    // คำนวณจุดกึ่งกลางระหว่าง 2 ตึก และเยื้องขึ้นด้านบนเล็กน้อยไม่ให้บังเส้นถนน
                    Vector3 midPoint = (node.transform.position + path.targetNode.transform.position) / 2f;
                    midPoint.y += 0.4f; 

                    // เสกป้ายขึ้นมาบนแผนที่
                    GameObject labelObj = Instantiate(textLabelPrefab, midPoint, Quaternion.identity, transform);
                    
                    // 1. จัดการเปลี่ยนข้อความ Text ด้วยระบบ Rich Text คมชัด
                    TextMeshPro textMesh = labelObj.GetComponentInChildren<TextMeshPro>();
                    if (textMesh != null)
                    {
                        textMesh.text = $"⏱️ <b>{path.timeMinutes}</b> <size=75%>นาที</size>\n💨 <size=90%>{path.carbonGrams}g</size>";
                    }

                    // 2. ย้อมสีพื้นหลัง หรือดึงค่าสีเส้น (Path Color) มาช่วยเน้นให้สวยงาม
                    SpriteRenderer bgRenderer = labelObj.transform.Find("Badge_BG")?.GetComponent<SpriteRenderer>();
                    if (bgRenderer != null)
                    {
                        // ทริคพรีเมียม: นำสีของพาหนะนั้นๆ มาทำเป็นสีไฮไลต์จางๆ รอบป้าย
                        Color highlightColor = path.pathColor;
                        highlightColor.a = 0.15f; // ให้ฟุ้งๆ บางๆ 
                        
                        // หรือจะกำหนดให้ Badge_BG เปลี่ยนสีตามความเหมาะสมได้ตรงนี้เลยครับ
                    }
                }
            }
        }
    }
}