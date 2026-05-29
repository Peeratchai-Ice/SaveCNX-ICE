using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // ✨ 1. เพิ่มบรรทัดนี้เพื่อให้รู้จักระบบ UI

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
    public enum NodeRole { Normal, StartNode, EndNode, DeathNode, PickupNode }

    [Header("Node Info")]
    public string buildingName;

    [Header("Node Settings (ตั้งค่าสถานะตึก)")]
    public NodeRole nodeRole = NodeRole.Normal; 

    [Header("Death Node Settings")]
    [TextArea]
    public string deathReason = "คุณพยายามว่ายน้ำข้ามอ่างแก้ว... จมน้ำตาย!"; 

    [Header("Pickup Node Settings")]
    public GameObject friendGameObject; 
    [TextArea]
    public string missingFriendReason = "คุณลืมแวะรับเพื่อนไปโรงพยาบาลด้วย!"; 

    [Header("Visuals")]
    public Sprite buildingIcon; 

    [Header("Paths from this Node")]
    public List<PathData> connectedPaths = new List<PathData>();

    private Vector3 originalScale;
    private float hoverScaleMultiplier = 1.2f; 

    private void Start()
    {
        originalScale = transform.localScale;

        SpriteRenderer iconRenderer = transform.Find("BuildingIcon")?.GetComponent<SpriteRenderer>();
        if (iconRenderer != null && buildingIcon != null)
        {
            iconRenderer.sprite = buildingIcon;
        }
    }

    private void OnMouseEnter()
    {
        // ✨ 2. ดักไว้! ถ้าเมาส์ชี้อยู่บน UI ห้ามให้ตึกขยายตัวเด็ดขาด
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }

        transform.localScale = originalScale * hoverScaleMultiplier;
    }

    private void OnMouseExit()
    {
        transform.localScale = originalScale;
    }

    private void OnDrawGizmos()
    {
        switch (nodeRole)
        {
            case NodeRole.StartNode: Gizmos.color = Color.yellow; break;
            case NodeRole.EndNode: Gizmos.color = Color.green; break;
            case NodeRole.DeathNode: Gizmos.color = Color.red; break; 
            case NodeRole.PickupNode: Gizmos.color = Color.magenta; break; 
            default: Gizmos.color = Color.cyan; break;
        }

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