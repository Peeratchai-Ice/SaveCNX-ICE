using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryPageController : MonoBehaviour
{
    [Header("1. Story Pages Group")]
    public GameObject[] storyPages = new GameObject[4]; // ล็อกไว้ 4 หน้าวัตถุ

    [Header("Navigation Buttons")]
    public Button prevButton;  // 5. ปุ่มลูกศรซ้าย
    public Button nextButton;  // 4. ปุ่มลูกศรขวา

    private int currentIndex = 0;

    private void Start()
    {
        // เริ่มเกมมาให้เปิดหน้าแรก และปิดหน้าอื่นๆ ทั้งหมดซะ
        currentIndex = 0;
        UpdatePageVisibility();
    }

    // ฟังก์ชันจัดการเปิด-ปิดหน้าวัตถุ และเช็กปุ่ม
    public void UpdatePageVisibility()
    {
        for (int i = 0; i < storyPages.Length; i++)
        {
            if (storyPages[i] != null)
            {
                // เปิดเฉพาะหน้าปัจจุบัน (currentIndex) หน้าอื่นปิดให้หมด
                storyPages[i].SetActive(i == currentIndex);
            }
        }

        // 5. ปุ่ม Prev: ถ้าอยู่หน้าแรก (index 0) จะกดถอยหลังไม่ได้
        if (prevButton != null)
        {
            prevButton.interactable = (currentIndex > 0);
        }
    }

    // 4. ปุ่ม Next (ทางขวา): กดเพื่อไปหน้าถัดไป
    public void NextPage()
    {
        if (currentIndex < storyPages.Length - 1)
        {
            currentIndex++;
            UpdatePageVisibility();
        }
        else
        {
            // ถ้าอยู่หน้าสุดท้าย (หน้า 4) แล้วกดถัดไปอีก ให้ข้ามไปหน้าเลือกด่านเลย
            GoToStageSelection();
        }
    }

    // 5. ปุ่ม Prev (ทางซ้าย): กดเพื่อถอยกลับหน้าก่อนหน้า
    public void PreviousPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdatePageVisibility();
        }
    }

    // 2. ปุ่ม Btn_Back (ซ้ายบน): กลับหน้า MainMenu
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // 3. ปุ่ม Btn_Skip (ขวาบน): ข้ามไปหน้า StageSelection
    public void GoToStageSelection()
    {
        SceneManager.LoadScene("StageSelection");
    }
}