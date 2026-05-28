using UnityEngine;
using TMPro;

public class StageTimer : MonoBehaviour
{
    [Header("ตั้งค่าเวลา")]
    public float timeLimitSeconds = 60f; // ตั้งเวลาด่านนี้ (วินาที)
    
    [Header("หน้าจอ UI")]
    public TextMeshProUGUI timerText;    // ลาก Text ของเวลามาใส่ช่องนี้

    private float currentTime;
    private bool isRunning = false;

    void Start()
    {
        currentTime = timeLimitSeconds;
        UpdateUI();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    void Update()
    {
        if (isRunning && currentTime > 0)
        {
            currentTime -= Time.deltaTime; 
            
            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                TimeOut(); 
            }
            
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = $"เวลาหมดใน: <color=#FFD166><b>{Mathf.CeilToInt(currentTime)}</b></color> วิ.";
        }
    }

    private void TimeOut()
    {
        Debug.LogWarning("⚠️ หมดเวลา! (Time Out)");
        
        // หยุดเวลาโลกในเกม
        Time.timeScale = 0f; 
        
        // ✨ สั่งให้ระบบประเมินผลโชว์หน้าต่างจบเกมแบบ 0 ดาวทันที
        if (RouteEvaluator.Instance != null)
        {
            RouteEvaluator.Instance.HandleTimeOut();
        }
    }
}