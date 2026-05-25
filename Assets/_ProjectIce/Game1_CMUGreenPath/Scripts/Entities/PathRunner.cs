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
    
    [Header("Animation Settings")]
    public float arrowScrollSpeed = -2f; 
    
    // 🌟 1. ตัวแปรใหม่สำหรับเป็น "สวิตช์" คุมการวิ่งของลูกศร
    private bool isAnimatingArrows = false;
    private float currentArrowOffset = 0f;

    void Start()
    {
        pathLine = GetComponent<LineRenderer>();
        pathLine.positionCount = 1;
        if (startNode != null) {
            transform.position = startNode.transform.position;
            pathLine.SetPosition(0, transform.position);
        }
    }

    void Update()
    {
        // 🌟 2. ลูกศรจะขยับก็ต่อเมื่อสวิตช์ isAnimatingArrows ถูกเปิดเท่านั้น
        if (isAnimatingArrows && pathLine != null && pathLine.positionCount > 1 && pathLine.material != null)
        {
            // ใช้ Time.deltaTime บวกสะสมค่าไปเรื่อยๆ แทน Time.time เพื่อไม่ให้ลูกศรกระตุกตอนเริ่มวิ่ง
            currentArrowOffset += Time.deltaTime * arrowScrollSpeed;
            pathLine.material.mainTextureOffset = new Vector2(currentArrowOffset, 0);
        }
    }

    public void AddSegmentToPlan(PathData pathData)
    {
        plannedRoute.Add(pathData);
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        pathLine.positionCount = plannedRoute.Count + 1;
        pathLine.SetPosition(0, startNode.transform.position);

        if (plannedRoute.Count == 0) return;

        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[plannedRoute.Count + 1];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

        colorKeys[0] = new GradientColorKey(plannedRoute[0].pathColor, 0f);

        for (int i = 0; i < plannedRoute.Count; i++)
        {
            pathLine.SetPosition(i + 1, plannedRoute[i].targetNode.transform.position);
            float time = (float)(i + 1) / plannedRoute.Count;
            colorKeys[i + 1] = new GradientColorKey(plannedRoute[i].pathColor, time);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        pathLine.colorGradient = gradient;
    }

    public void ExecutePlan()
    {
        // 🌟 3. เปิดสวิตช์ให้ลูกศรเริ่มวิ่ง ทันทีที่กดปุ่ม GO
        isAnimatingArrows = true; 
        StartCoroutine(TravelRoutine());
    }

    private IEnumerator TravelRoutine()
    {
        foreach (PathData path in plannedRoute)
        {
            BuildingNode target = path.targetNode;
            float moveSpeed = (path.distanceKm / (path.timeMinutes + 0.1f)) * visualSpeedMultiplier;
            if(moveSpeed <= 0.5f) moveSpeed = 5f;

            while ((Vector2)transform.position != (Vector2)target.transform.position)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
                yield return null; 
            }
        }
        
        // 🌟 4. ปิดสวิตช์ให้ลูกศรหยุดวิ่ง เมื่อตัวละครเดินทางถึงจุดหมายแล้ว
        isAnimatingArrows = false; 

        if (plannedRoute.Count > 0)
        {
            BuildingNode finalNode = plannedRoute[plannedRoute.Count - 1].targetNode;
            if (RouteEvaluator.Instance != null)
                RouteEvaluator.Instance.EvaluatePlayerRoute(startNode, finalNode, plannedRoute);
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