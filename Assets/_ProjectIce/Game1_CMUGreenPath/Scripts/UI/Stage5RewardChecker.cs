using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage5RewardChecker : MonoBehaviour
{
    public void CheckRewardAndGoNext()
    {
        int totalStars = 0;
        
        // ดึงข้อมูลดาวที่ผู้เล่นทำได้ตั้งแต่ด่าน 1 ถึงด่าน 5 (Index 0 ถึง 4) มารวมกัน
        for (int i = 0; i < 5; i++)
        {
            totalStars += PlayerPrefs.GetInt("Stage_" + i + "_Stars", 0);
        }

        Debug.Log("รวมดาวทั้งหมดได้: " + totalStars + " ดวง");

        if (totalStars >= 15)
        {
            // ถ้าเล่นครบ 15 ดาวเป๊ะ! ให้โหลดหน้าแจกรางวัล
            SceneManager.LoadScene("RewardScene");
        }
        else
        {
            // ถ้าดาวไม่ครบ ให้กลับไปหน้าเลือกด่านตามปกติ
            SceneManager.LoadScene("StageSelection");
        }
    }
}