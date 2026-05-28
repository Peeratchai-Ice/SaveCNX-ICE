using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathRunner : MonoBehaviour
{
    public BuildingNode startNode;
    public List<PathData> plannedRoute = new List<PathData>(); 
    private LineRenderer pathLine;
    
    public float visualSpeedMultiplier = 10f; 
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

        colorKeys[0] = new GradientColorKey(plannedRoute[0].pathColor, 0f);

        for (int i = 0; i < plannedRoute.Count; i++)
        {
            if (plannedRoute[i] != null && plannedRoute[i].targetNode != null)
            {
                pathLine.SetPosition(i + 1, plannedRoute[i].targetNode.transform.position);
                float time = (float)(i + 1) / plannedRoute.Count;
                colorKeys[i + 1] = new GradientColorKey(plannedRoute[i].pathColor, time);
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
        
        // ✨ ตัวแปรใหม่: เก็บประวัติว่าเหยียบกับดักไหม
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
            float moveSpeed = (path.distanceKm / (path.timeMinutes + 0.1f)) * visualSpeedMultiplier;
            if (moveSpeed <= 0.5f) moveSpeed = 5f;

            while ((Vector2)transform.position != (Vector2)target.transform.position)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
                yield return null; 
            }

            // ✨ ถ้าเหยียบจุดตาย จะบันทึกไว้ แต่ "ไม่หยุดวิ่ง" ปล่อยให้วิ่งจนจบ
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
                // ✨ ส่งข้อมูลทั้งเรื่องลืมเพื่อน และเรื่องเหยียบกับดัก ไปหักคะแนน
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