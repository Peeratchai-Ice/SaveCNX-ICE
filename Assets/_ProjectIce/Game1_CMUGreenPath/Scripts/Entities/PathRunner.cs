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
    public float constantMoveSpeed = 15f; 

    [Header("Line Settings (ตั้งค่าเส้นทาง)")]
    public Color overridePathColor = Color.magenta; 
    public float arrowScrollSpeed = -2f; 

    [Header("Follower Settings")]
    public GameObject followerFriend; 

    // ✨ ช่องสำหรับใส่เสียงเดินของตัวละคร
    [Header("Audio Settings (ระบบเสียงเดิน)")]
    public AudioSource movementAudioSource; 
    public AudioClip moveSoundClip;         

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

        // 1. กำหนดพิกัดจุดหักเลี้ยวของเส้น
        for (int i = 0; i < plannedRoute.Count; i++)
        {
            if (plannedRoute[i] != null && plannedRoute[i].targetNode != null)
            {
                pathLine.SetPosition(i + 1, plannedRoute[i].targetNode.transform.position);
            }
        }

        // ✨ 2. แก้บัค Color Keys เกิน 8 สี: สร้างแค่ 2 จุดหัว-ท้ายก็พอ เพราะเป็นสีเดียวกันทั้งเส้น
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(overridePathColor, 0f);
        colorKeys[1] = new GradientColorKey(overridePathColor, 1f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

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

        // ✨ [เล่นเสียงเดิน] 
        if (movementAudioSource != null && moveSoundClip != null)
        {
            movementAudioSource.clip = moveSoundClip;
            movementAudioSource.loop = true; 
            movementAudioSource.Play();
        }

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
                break; 
            }

            if (target.nodeRole == BuildingNode.NodeRole.PickupNode)
            {
                hasPickedUpFriend = true;
                if (target.friendGameObject != null) target.friendGameObject.SetActive(false); 
                if (followerFriend != null) followerFriend.SetActive(true);
            }
        }
        
        isAnimatingArrows = false; 

        // ✨ [หยุดเสียงเดิน]
        if (movementAudioSource != null)
        {
            movementAudioSource.Stop();
        }

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