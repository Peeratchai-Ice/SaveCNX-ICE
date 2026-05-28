using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelNavigation : MonoBehaviour
{
    [Header("Scene Settings")]
    public string selectStageSceneName = "SelectStage"; // ชื่อ Scene กลับหน้าหลัก
    public string nextStageSceneName = "";              // ✨ ชื่อ Scene ด่านต่อไป (ปล่อยว่างไว้ไปเติมใน Unity)

    // ฟังก์ชันกลับหน้าเลือกด่าน (ของเดิม)
    public void GoBackToSelectStage()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(selectStageSceneName);
    }

    // ✨ ฟังก์ชันใหม่! สำหรับปุ่มไปด่านต่อไป
    public void GoToNextLevel()
    {
        Time.timeScale = 1f; // ปลดล็อกเวลาก่อนเปลี่ยนฉาก
        
        if (!string.IsNullOrEmpty(nextStageSceneName))
        {
            SceneManager.LoadScene(nextStageSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ ยังไม่ได้พิมพ์ชื่อ Scene ด่านต่อไปใน Inspector ครับ!");
        }
    }
}