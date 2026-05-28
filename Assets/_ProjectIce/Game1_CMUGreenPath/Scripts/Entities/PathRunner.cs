using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathRunner : MonoBehaviour
{
    public BuildingNode startNode;
    public List<PathData> plannedRoute = new List<PathData>(); 
    private LineRenderer pathLine;

    [Header("Movement Settings (ตั้งค่าการเดิน)")]
    // ✨ ช่องใหม่: กำหนดความเร็วคงที่ (ยิ่งค่ายิ่งเยอะ ยิ่งวิ่งเร็ว)
    public float constantMoveSpeed = 15f; 

    [Header("Line Settings (ตั้งค่าเส้นทาง)")]
    // ✨ ช่องใหม่: จิ้มเลือกสีเส้นทางที่ลากบนแผนที่ได้เลย
    public Color overridePathColor = Color.magenta; 
    public float arrowScrollSpeed = -2f; 

    [Header("Follower Settings")]
    public GameObject followerFriend; 
    
    private bool isAnimatingArrows = false;
    private float currentArrowOffset = 0f;

    void Awake() { pathLine = GetComponent<LineRenderer>(); }

    void Start()
    {
        if (pathLine != null) 
        {
            pathLine.positionCount = 1;
            if (startNode != null) {
                transform.position = startNode.transform.position;
                pathLine.SetPosition(0, transform.position);
            }
        }
        
        if (followerFriend != null) followerFriend.SetActive(false);
    }

    void Update()
    {
        if (isAnimatingArrows && pathLine != null && pathLine.positionCount > 1 && pathLine.material != null)
        {
            currentArrowOffset += Time.deltaTime * arrowScrollSpeed;
            pathLine.material.mainTextureOffset = new Vector2(currentArrowOffset, 0);
        }
    }

    public void AddSegmentToPlan(PathData pathData)
    {
        if (pathData == null) return; 
        plannedRoute.Add(pathData);
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        if (startNode == null) return;
        if (pathLine == null) return;

        pathLine.positionCount = plannedRoute.Count + 1;
        pathLine.SetPosition(0, startNode.transform.position);

        if (plannedRoute.Count == 0) return;

        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[plannedRoute.Count + 1];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

        // ✨ บังคับใช้สีที่ตั้งไว้ใน overridePathColor สีเดียวทั้งเส้น
        colorKeys[0] = new GradientColorKey(overridePathColor, 0f);

        for (int i = 0; i < plannedRoute.Count; i++)
        {
            if (plannedRoute[i] != null && plannedRoute[i].targetNode != null)
            {
                pathLine.SetPosition(i + 1, plannedRoute[i].targetNode.transform.position);
                float time = (float)(i + 1) / plannedRoute.Count;
                colorKeys[i + 1] = new GradientColorKey(overridePathColor, time);
            }
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        pathLine.colorGradient = gradient;
    }

    public void ExecutePlan()
    {
        isAnimatingArrows = true; 
        StartCoroutine(TravelRoutine());
    }

    private IEnumerator TravelRoutine()
    {
        bool hasPickedUpFriend = false;
        BuildingNode requiredPickupNode = null;
        bool hitDeathNode = false;
        string deathReasonMsg = "";

        foreach (BuildingNode node in FindObjectsOfType<BuildingNode>())
        {
            if (node.nodeRole == BuildingNode.NodeRole.PickupNode)
            {
                requiredPickupNode = node;
                if (node.friendGameObject != null) node.friendGameObject.SetActive(true); 
                break;
            }
        }

        if (followerFriend != null) followerFriend.SetActive(false);

        foreach (PathData path in plannedRoute)
        {
            if (path == null || path.targetNode == null) continue; 

            BuildingNode target = path.targetNode;
            
            // ✨ ใช้ความเร็วคงที่เสมอ (ตัดสมการเวลาและระยะทางทิ้งไปเลย)
            float moveSpeed = constantMoveSpeed;

            while ((Vector2)transform.position != (Vector2)target.transform.position)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
                yield return null; 
            }

            if (target.nodeRole == BuildingNode.NodeRole.DeathNode && !hitDeathNode)
            {
                hitDeathNode = true;
                deathReasonMsg = target.deathReason;
            }

            if (target.nodeRole == BuildingNode.NodeRole.PickupNode)
            {
                hasPickedUpFriend = true;
                if (target.friendGameObject != null) target.friendGameObject.SetActive(false); 
                if (followerFriend != null) followerFriend.SetActive(true);
            }
        }
        
        isAnimatingArrows = false; 

        if (plannedRoute.Count > 0)
        {
            BuildingNode finalNode = plannedRoute[plannedRoute.Count - 1].targetNode;
            string missReason = (requiredPickupNode != null && !hasPickedUpFriend) ? requiredPickupNode.missingFriendReason : "";

            if (RouteEvaluator.Instance != null && startNode != null && finalNode != null)
            {
                RouteEvaluator.Instance.EvaluatePlayerRoute(startNode, finalNode, plannedRoute, hasPickedUpFriend, missReason, hitDeathNode, deathReasonMsg);
            }
        }
    }

    public BuildingNode UndoLastSegment()
    {
        if (plannedRoute.Count > 0)
        {
            plannedRoute.RemoveAt(plannedRoute.Count - 1);
            UpdateLineRenderer();
            return plannedRoute.Count > 0 ? plannedRoute[plannedRoute.Count - 1].targetNode : startNode;
        }
        return startNode;
    }
}