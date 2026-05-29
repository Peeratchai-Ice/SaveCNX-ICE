using UnityEngine;
using UnityEngine.SceneManagement;

public class RewardSceneManager : MonoBehaviour
{
    // ฟังก์ชันนี้เอาไว้ผูกกับปุ่ม Back
    public void GoToStageSelection()
    {
        SceneManager.LoadScene("StageSelection"); // เปลี่ยนกลับไปหน้าเลือกด่าน
    }
}