using UnityEngine;

public class MissionBriefingManager : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject mainBriefingPanel; 
    public GameObject page1;             
    public GameObject page2;             

    [Header("Gameplay UI")]
    // ✨ เพิ่มตัวแปรสำหรับรับกล่องเวลา
    public GameObject timerPanel;        

    void Start()
    {
        if (mainBriefingPanel != null) mainBriefingPanel.SetActive(true);
        
        // ✨ ซ่อน Timer Panel ไว้ก่อนตอนเริ่มด่าน
        if (timerPanel != null) timerPanel.SetActive(false); 
        
        ShowPage1();
        Time.timeScale = 0f; 
    }

    public void ShowPage1()
    {
        if (page1 != null) page1.SetActive(true);
        if (page2 != null) page2.SetActive(false);
    }

    public void ShowPage2()
    {
        if (page1 != null) page1.SetActive(false);
        if (page2 != null) page2.SetActive(true);
    }

    public void StartMission()
    {
        if (mainBriefingPanel != null) mainBriefingPanel.SetActive(false);
        
        // ✨ โชว์ Timer Panel ขึ้นมาเมื่อผู้เล่นกด "เข้าสู่ภารกิจ"
        if (timerPanel != null) timerPanel.SetActive(true); 
        
        Time.timeScale = 1f; 
    }
}