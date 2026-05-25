using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathData
{
    public BuildingNode targetNode;
    public string vehicleName; 
    public Sprite vehicleIcon;     
    
    [Header("Manual Setup")]
    public float distanceKm;        
    public float timeMinutes;       
    public float carbonGrams;       
    
    public Color pathColor = Color.white; 
}

public class BuildingNode : MonoBehaviour
{
    [Header("Node Info")]
    public string buildingName;
    public bool isStartNode = false; 
    public bool isGoalNode = false;

    [Header("Visuals")]
    public Sprite buildingIcon; 

    [Header("Paths from this Node")]
    public List<PathData> connectedPaths = new List<PathData>();

    // ตัวแปรสำหรับทำ Hover Effect
    private Vector3 originalScale;
    private float hoverScaleMultiplier = 1.2f; // ขยายขนาดขึ้น 20% ตอนเมาส์ชี้

    private void Start()
    {
        // จำขนาดดั้งเดิมของตึกไว้ก่อน
        originalScale = transform.localScale;

        // ระบบดึงรูปตึกมาโชว์ (ถ้ามี)
        SpriteRenderer iconRenderer = transform.Find("BuildingIcon")?.GetComponent<SpriteRenderer>();
        if (iconRenderer != null && buildingIcon != null)
        {
            iconRenderer.sprite = buildingIcon;
        }
    }

    // 🌟 เมื่อเมาส์ชี้เข้ามาในเขต Collider
    private void OnMouseEnter()
    {
        transform.localScale = originalScale * hoverScaleMultiplier;
    }

    // 🌟 เมื่อเมาส์ลากออกนอกเขต Collider
    private void OnMouseExit()
    {
        transform.localScale = originalScale;
    }

    private void OnDrawGizmos()
    {
        if (isStartNode) Gizmos.color = Color.yellow;
        else if (isGoalNode) Gizmos.color = Color.red;
        else Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(transform.position, 0.4f);

        foreach (var path in connectedPaths)
        {
            if (path.targetNode != null)
            {
                Gizmos.color = path.pathColor;
                Gizmos.DrawLine(transform.position, path.targetNode.transform.position);
            }
        }
    }
}