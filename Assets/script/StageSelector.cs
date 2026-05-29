using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageSelector : MonoBehaviour
{
    [System.Serializable]
    public class StageButtonUI
    {
        public string stageSceneName; 
        public Button stageButton;    
        public GameObject lockIcon;   
        
        [Header("ใส่เฉพาะดาวเหลือง (YesStar)")]
        public GameObject[] yellowStars = new GameObject[3]; 
    }

    [Header("ตั้งค่าปุ่มทั้ง 5 ด่าน")]
    public StageButtonUI[] stageElements = new StageButtonUI[5];

    private void Start()
    {
        RefreshStageSelectionUI();
    }

    public void RefreshStageSelectionUI()
    {
        for (int i = 0; i < stageElements.Length; i++)
        {
            // ✨ 1. เซฟตี้: ถ้าช่อง Element ของด่านนั้นพังหรือแหว่ง ให้ข้ามไปเลย เกมจะได้ไม่ Error
            if (stageElements[i] == null) continue;

            // ด่านแรก (Index 0) จะเปิดให้เล่นเสมอ
            bool isUnlocked = (i == 0);

            // เช็คการปลดล็อกด่านจาก PlayerPrefs ของด่านก่อนหน้า
            if (i > 0)
            {
                string previousStageUnlockedKey = "Stage_" + (i - 1) + "_Unlocked";
                isUnlocked = PlayerPrefs.GetInt(previousStageUnlockedKey, 0) == 1;
            }

            // 1. เปิด/ปิด ปุ่มและกุญแจ (พร้อมระบบเซฟตี้ป้องกันการลืมลากของมาใส่)
            if (stageElements[i].stageButton != null)
            {
                stageElements[i].stageButton.interactable = isUnlocked;
            }
            if (stageElements[i].lockIcon != null)
            {
                stageElements[i].lockIcon.SetActive(!isUnlocked);
            }

            // 2. ดึงจำนวนดาวที่เคยทำได้มาแสดง
            string starKey = "Stage_" + i + "_Stars";
            int starsEarned = PlayerPrefs.GetInt(starKey, 0); 

            // 3. สั่งเปิด/ปิดดาวเหลือง (YesStar)
            if (stageElements[i].yellowStars != null)
            {
                for (int starIdx = 0; starIdx < stageElements[i].yellowStars.Length; starIdx++)
                {
                    // ✨ 2. เซฟตี้: เช็คให้ชัวร์ว่าช่องดาวดวงนี้มี GameObject ลากมาใส่ไว้จริงๆ
                    if (stageElements[i].yellowStars[starIdx] != null)
                    {
                        bool shouldShowYellowStar = starIdx < starsEarned;
                        stageElements[i].yellowStars[starIdx].SetActive(shouldShowYellowStar);
                    }
                }
            }
        }
    }

    public void LoadStageScene(int stageIndex)
    {
        // เซฟตี้ป้องกันกดปุ่มด่านที่ยังไม่ได้ตั้งค่า
        if (stageIndex < 0 || stageIndex >= stageElements.Length || stageElements[stageIndex] == null) return;

        string sceneName = stageElements[stageIndex].stageSceneName;
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ ยังไม่ได้พิมพ์ชื่อ Scene ของด่านที่ " + (stageIndex + 1) + " ใน Inspector ครับ!");
        }
    }
    
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        RefreshStageSelectionUI();
        Debug.Log("✅ ล้างข้อมูลการเล่นทั้งหมดเรียบร้อยแล้ว!");
    }
}