using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio; // 🌟 เพิ่มเข้ามาเพื่อใช้ควบคุม Audio Mixer

public class MenuController : MonoBehaviour
{
    [Header("Stage Selection References")]
    public Button stage2Button;    
    public GameObject lockImage;   

    [Header("UI Panels")]
    public GameObject settingPanel; // 🌟 ช่องสำหรับลาก Panel_Setting มาใส่

    [Header("Audio Control")]
    public AudioMixer audioMixer;   // 🌟 ช่องสำหรับลาก MainAudioMixer มาใส่
    public Slider bgmSlider;        // 🌟 ช่องสำหรับลาก Slider_BGM มาใส่
    public Slider sfxSlider;        // 🌟 ช่องสำหรับลาก Slider_SFX มาใส่

    [Header("SFX Click")]
    public AudioSource sfxAudioSource; // 🌟 ช่องสำหรับลาก Audio Source ตัวที่สองมาใส่
    public AudioClip clickSound;       // 🌟 ช่องสำหรับลากไฟล์เสียงคลิกมาใส่

    private void Start()
    {
        // === 1. ตรวจสอบและโหลดสถานะด่านล็อก/ปลดล็อก ===
        if (stage2Button != null)
        {
            CheckStageUnlock();
        }

        // === 2. ตั้งค่าระบบเสียงเริ่มต้นตอนเปิดเมนู ===
        if (settingPanel != null) 
        {
            settingPanel.SetActive(false); // เริ่มเกมมาให้ปิดหน้า Setting ไว้ก่อนเสมอ
        }

        // โหลดค่าความดังเดิมที่ผู้เล่นเคยปรับไว้จากเครื่อง (ถ้าไม่มีให้ตั้งค่าเริ่มต้นที่ 0.75f)
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        if (bgmSlider != null) bgmSlider.value = savedBGM;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);
    }

    private void CheckStageUnlock()
    {
        int isUnlocked = PlayerPrefs.GetInt("Stage2_Unlocked", 0);

        if (isUnlocked == 1)
        {
            stage2Button.interactable = true; 
            if (lockImage != null) lockImage.SetActive(false); 
        }
        else
        {
            stage2Button.interactable = false; 
            if (lockImage != null) lockImage.SetActive(true); 
        }
    }

    // ==========================================
    // 🌟 ส่วนของปุ่มกดเมนูหลัก (Main Menu Buttons)
    // ==========================================

    // ปุ่มที่ 1: New Game (สำหรับผู้เล่นใหม่)
    public void NewGame()
    {
        PlayClickSound();
        
        // ล้างข้อมูลการปลดล็อกด่านเดิม และจำนวนดาวเก่าทั้งหมดเพื่อเริ่มใหม่
        PlayerPrefs.DeleteAll(); // แนะนำให้ล้างหมดเลยเพื่อให้เล่นใหม่จริงจากด่าน 1 ที่ได้ 0 ดาว
        PlayerPrefs.Save();
        
        // ✨ เปลี่ยนจาก "StageSelection" ให้แก้เป็นคำว่า "StoryScene" ตรงนี้เลยครับ!
        SceneManager.LoadScene("StoryScene");
    }
    // ปุ่มที่ 2: Load Game (สำหรับคนที่เคยเล่นแล้ว มีข้อมูลเซฟด่าน)
    public void LoadGame()
    {
        PlayClickSound();
        Debug.Log("โหลดข้อมูลเกมเดิมและเข้าสู่หน้าเลือกด่าน...");
        
        // โหลดเข้าซีนเลือกด่านโดยไม่ล้างค่าเซฟเดิม
        SceneManager.LoadScene("StageSelection");
    }

    // ปุ่มที่ 3: Open Setting (เปิดหน้าต่างตั้งค่า)
    public void OpenSetting()
    {
        PlayClickSound();
        if (settingPanel != null) settingPanel.SetActive(true);
    }

    // ปุ่มปิดหน้าต่างตั้งค่า (ปุ่ม Back หรือ กากบาท)
    public void CloseSetting()
    {
        PlayClickSound();
        if (settingPanel != null) settingPanel.SetActive(false);
        PlayerPrefs.Save(); // บันทึกค่าความดังเสียงทั้งหมดลงเครื่องอย่างปลอดภัย
    }

    // ปุ่มที่ 4: Exit Game (ออกจากเกม)
    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("Game is exiting...");
        Application.Quit();
    }

    // ==========================================
    // 🌟 ส่วนของหน้าเลือกด่าน (Stage Selection)
    // ==========================================

    public void OpenStage1()
    {
        PlayClickSound();
        SceneManager.LoadScene("Gameplay_Level"); 
    }

    public void OpenStage2()
    {
        PlayClickSound();
        SceneManager.LoadScene("Stage2_Scene"); 
    }

    // ==========================================
    // 🌟 ส่วนของระบบควบคุมเสียง (Audio Functions)
    // ==========================================

    public void SetBGMVolume(float value)
    {
        if (audioMixer != null)
        {
            // คำนวณค่าจากระดับ 0-1 ในสไลเดอร์ให้เป็นหน่วยเดซิเบล (dB) แบบสมจริง
            audioMixer.SetFloat("BGMVol", Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat("BGMVolume", value);
        }
    }

    public void SetSFXVolume(float value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat("SFXVolume", value);
        }
    }

    private void PlayClickSound()
    {
        if (sfxAudioSource != null && clickSound != null)
        {
            sfxAudioSource.PlayOneShot(clickSound);
        }
    }

    // ฟังก์ชันพิเศษสำหรับคลิกขวาเคลียร์ดาต้าเพื่อเทสใน Unity Editor
    [ContextMenu("Reset Game Data")]
    public void ResetGameData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=red>=== Game Data Reset! ===</color>");
    }
}