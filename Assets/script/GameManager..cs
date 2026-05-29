using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Stage Settings (กำหนดใน Inspector ของแต่ละด่าน)")]
    public int currentStageIndex = 0; // ด่านปัจจุบัน (0 = ด่าน 1, 1 = ด่าน 2, ... 4 = ด่าน 5)
    public float matchTimeLimit = 60f; // เวลาจำกัดของด่านนี้
    public float targetTimeForStar = 40f; // เกณฑ์เวลาที่จะได้ดาวดวงที่ 2 (ต้องผ่านภายในกี่วิ)
    public float maxCarbonLimit = 100f; // เกณฑ์คาร์บอนสูงสุด (ด่าน 2 ใช้ 50, ด่าน 5 ใช้ 100)

    [Header("UI Text References")]
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI carbonText; // 🌟 เพิ่ม Text แสดงค่าคาร์บอนปัจจุบันในด่าน
    public Slider timerSlider;        

    [Header("UI Pop-up Panels")] 
    public GameObject tutorialPanel; 
    public GameObject settingPanel;    // 🌟 เพิ่มช่องสำหรับหน้าต่าง Setting
    public GameObject resultPanel;     // 🌟 เปลี่ยนจาก Victory/Defeat เป็น Result เดียวกันแล้วใช้เปิดดาวเอา

    [Header("Result UI Stars (รูปดาวในหน้าสรุปผล)")]
    public GameObject star1_Active; // รูปดาวดวงที่ 1 สีเหลือง
    public GameObject star2_Active; // รูปดาวดวงที่ 2 สีเหลือง
    public GameObject star3_Active; // รูปดาวดวงที่ 3 สีเหลือง
    public TextMeshProUGUI resultStatusText; // ข้อความบอกว่า "ผ่านด่าน!" หรือ "ภารกิจล้มเหลว"

    // ตัวแปรสถานะภายในเกม
    private float timeElapsed = 0f;    // เวลาที่ใช้ไป
    private float timeRemaining;
    private float currentCarbon = 0f;  // ค่าคาร์บอนที่ใช้ไปในปัจจุบัน
    private bool isGameOver = false;
    private bool isTutorialActive = true;
    private bool isGamePaused = false;  // เช็กว่ากดปุ่ม Pause/Setting อยู่ไหม

    // เงื่อนไขในการผ่านด่าน (ให้เพื่อนสั่งเปลี่ยนค่าเมื่อเคลียร์เงื่อนไขของด่าน)
    [HideInInspector] public bool hasReachedGoal = false; 
    [HideInInspector] public bool hasVisitedAllWaypoints = true; // ด่าน 3, 5 ต้องให้เพื่อนเซ็ตเป็น true เมื่อเก็บครบ
    [HideInInspector] public bool isGameOverByObstacle = false; // ด่าน 4 ถ้าชนหมาดุ ให้เพื่อนเซ็ตเป็น true

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ปิด Panel ทั้งหมดตอนเริ่ม
        if (resultPanel != null) resultPanel.SetActive(false);
        if (settingPanel != null) settingPanel.SetActive(false);

        // โชว์ Tutorial ตอนเริ่มเกม
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            isTutorialActive = true; 
            Time.timeScale = 0f; 
        }
        else
        {
            isTutorialActive = false;
            Time.timeScale = 1f; 
        }

        timeRemaining = matchTimeLimit;
        
        if (timerSlider != null)
        {
            timerSlider.maxValue = matchTimeLimit;
            timerSlider.value = matchTimeLimit;
        }

        UpdateTimerUI(); 
        UpdateCarbonUI();
    }

    private void Update()
    {
        if (isGameOver || isTutorialActive || isGamePaused) return;

        // นับเวลาถอยหลัง
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timeElapsed += Time.deltaTime; // นับเวลาที่ใช้ไปจริงเพื่อเอาไปคิดดาวดวงที่ 2
            
            if (timerSlider != null)
            {
                timerSlider.value = timeRemaining; 
            }

            UpdateTimerUI(); 
        }
        else
        {
            timeRemaining = 0;
            UpdateTimerUI();
            FinishGame(false); // หมดเวลา = แพ้
        }
    }

    #region UI Control Functions (ระบบปุ่มกดต่างๆ)

    // ปุ่มกดปิดหน้า Tutorial ตอนเริ่มเกม / หรือปิดจากปุ่มตัว i
    public void CloseTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        isTutorialActive = false;
        
        // ถ้าหน้า Setting ไม่ได้เปิดอยู่ด้วย ให้เกมเดินต่อ
        if (!isGamePaused) Time.timeScale = 1f; 
    }

    // ปุ่มตัว i: กดเพื่อดูวิธีเล่นซ้ำระหว่างเกม
    public void OpenTutorialInGame()
    {
        if (isGameOver) return;
        isTutorialActive = true;
        Time.timeScale = 0f; // หยุดเกมไว้ก่อน
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
    }

    // ปุ่ม Setting / ปุ่มหยุดเกม (Pause)
    public void OpenSettingPanel()
    {
        if (isGameOver) return;
        isGamePaused = true;
        Time.timeScale = 0f;
        if (settingPanel != null) settingPanel.SetActive(true);
    }

    // ปุ่มกด "เล่นต่อ" (Resume) จากหน้า Setting
    public void CloseSettingPanel()
    {
        if (settingPanel != null) settingPanel.SetActive(false);
        isGamePaused = false;
        
        // ถ้าหน้า Tutorial ไม่ได้เปิดอยู่ด้วย ให้เกมเดินต่อ
        if (!isTutorialActive) Time.timeScale = 1f;
    }

    #endregion

    #region Gameplay Logic (ฟังก์ชันสำหรับเพื่อนเรียกใช้)

    // เพื่อนสามารถเรียกฟังก์ชันนี้เพื่อเพิ่มค่าคาร์บอนเมื่อผู้เล่นขับรถม่วง/มอเตอร์ไซค์
    public void AddCarbon(float amount)
    {
        if (isGameOver || isTutorialActive || isGamePaused) return;
        currentCarbon += amount;
        UpdateCarbonUI();

        // เช็กเงื่อนไขด่าน 2: ถ้าปล่อยคาร์บอนเกินเกณฑ์ (50) และด่านนั้นซีเรียสเรื่องห้ามเกิน (สามารถปรับประยุกต์ได้)
        // หรือถ้าต้องการให้คาร์บอนเกินแล้วไม่ Game Over ทันทีแต่ไม่ได้ดาวดวงที่ 3 ก็ย้ายลอจิกไปเช็กตอนท้ายได้ครับ
    }

    private void UpdateCarbonUI()
    {
        if (carbonText != null)
        {
            carbonText.text = "CO2: " + currentCarbon.ToString("F1") + " g";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timeRemaining);
            timerText.text = "TIME: " + seconds.ToString() + "s";
        }
    }

    // ฟังก์ชันจบเกม (เพื่อนจะเรียกฟังก์ชันนี้เมื่อผู้เล่นเดินไปถึงโหนดเป้าหมายสำเร็จ หรือเดินชนหมาดุ)
    public void FinishGame(bool reachedGoalSuccess)
    {
        isGameOver = true;
        Time.timeScale = 0f; // หยุดเกมทันที

        hasReachedGoal = reachedGoalSuccess;

        if (resultPanel != null) resultPanel.SetActive(true);

        // ซ่อนดาวเหลืองไว้ก่อน รอคำนวณเปิด
        if (star1_Active != null) star1_Active.SetActive(false);
        if (star2_Active != null) star2_Active.SetActive(false);
        if (star3_Active != null) star3_Active.SetActive(false);

        int totalStarsEarned = 0;

        // เงื่อนไขด่าน 4: ถ้าโดนหมากัด ถือว่าแพ้ทันที
        if (isGameOverByObstacle)
        {
            if (resultStatusText != null) resultStatusText.text = "GAME OVER!\nโดนหมาอ่างแก้วกัด";
            return; 
        }

        // 🌟 คำนวณดาวระบบใหม่
        if (hasReachedGoal && hasVisitedAllWaypoints)
        {
            // ⭐ ดาวดวงที่ 1: ไปถึงเป้าหมายสำเร็จ และเก็บจุดแวะพักครบ (สำหรับด่าน 3, 5)
            totalStarsEarned++;
            if (star1_Active != null) star1_Active.SetActive(true);

            // ⭐ ดาวดวงที่ 2: ทำเวลาผ่านเกณฑ์ (ใช้เวลาไปน้อยกว่าเกณฑ์ที่กำหนด)
            if (timeElapsed <= targetTimeForStar)
            {
                totalStarsEarned++;
                if (star2_Active != null) star2_Active.SetActive(true);
            }

            // ⭐ ดาวดวงที่ 3: คาร์บอนไม่เกินเกณฑ์ที่กำหนด
            if (currentCarbon <= maxCarbonLimit)
            {
                totalStarsEarned++;
                if (star3_Active != null) star3_Active.SetActive(true);
            }

            if (resultStatusText != null) resultStatusText.text = "ภารกิจสำเร็จ!";
            
            // 💾 บันทึกคะแนนลงเครื่องด้วย PlayerPrefs
            SaveStageProgress(currentStageIndex, totalStarsEarned);
        }
        else
        {
            if (resultStatusText != null) resultStatusText.text = "ภารกิจล้มเหลว!";
        }
    }

    private void SaveStageProgress(int stageIdx, int stars)
    {
        // บันทึกจำนวนดาวที่ทำได้ดีที่สุดในด่านนั้นๆ
        string starKey = "Stage_" + stageIdx + "_Stars";
        int oldStars = PlayerPrefs.GetInt(starKey, 0);
        if (stars > oldStars)
        {
            PlayerPrefs.SetInt(starKey, stars);
        }

        // ลอจิกการปลดล็อกด่านถัดไป: ถ้าได้ 3 ดาว จะเซ็ตให้ด่านถัดไป unlocked = 1
        if (stars == 3 && stageIdx < 4) 
        {
            string nextStageKey = "Stage_" + (stageIdx + 1) + "_Unlocked";
            PlayerPrefs.SetInt(nextStageKey, 1); 
        }
        PlayerPrefs.Save();
    }

    #endregion

    #region Scene Management (จัดการเปลี่ยนหน้า)

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToStageSelection()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StageSelection"); 
    }

    #endregion
}