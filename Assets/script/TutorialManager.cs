using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject tutorialPanel;

    // สามารถลากสคริปต์ตัวจับเวลา (Timer) มาใส่ตรงนี้ได้ ถ้าต้องการสั่งให้มันเริ่มทำงานแบบเจาะจง
    // [SerializeField] private MonoBehaviour timerScript; 

    void Start()
    {
        // 1. เปิดหน้า Tutorial ขึ้นมาตอนเริ่มเกม
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        // 2. หยุดเวลาในเกมทั้งหมดไว้ (ตัวละครจะขยับไม่ได้, เวลาไม่เดิน)
        Time.timeScale = 0f;
    }

    // ฟังก์ชันสำหรับผูกกับปุ่มเพื่อปิดหน้าต่างและเริ่มเกม
    public void CloseTutorialAndStartGame()
    {
        // 1. ปิดหน้าต่าง Tutorial
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // 2. ปล่อยให้เวลาในเกมเดินตามปกติ (ตัวละครขยับได้, เริ่มจับเวลา)
        Time.timeScale = 1f;

        // ถ้ามีสคริปต์เวลาแยกที่ต้องเปิดใช้งาน สามารถสั่งเปิดตรงนี้ได้ เช่น:
        // if (timerScript != null) timerScript.enabled = true;
    }
}