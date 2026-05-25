using UnityEngine;
using UnityEngine.UI; // 🌟 ต้องมีบรรทัดนี้เพื่อใช้ UI Image
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI Objects")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI infoText;
    public Image vehicleIconImage; // 🌟 ช่องสำหรับลาก UI Image มาใส่

    private void Awake()
    {
        Instance = this;
        HideTooltip(); 
    }

    // อัปเกรดฟังก์ชันให้รับค่ารูปภาพ (Sprite) เข้ามาด้วย
    public void ShowTooltip(string message, Sprite icon = null)
    {
        if (tooltipPanel != null && infoText != null)
        {
            tooltipPanel.SetActive(true);
            infoText.text = message;

            // ระบบจัดการไอคอน
            if (vehicleIconImage != null)
            {
                if (icon != null) // ถ้าเส้นทางนี้มีรูปตั้งไว้
                {
                    vehicleIconImage.gameObject.SetActive(true); // เปิดรูป
                    vehicleIconImage.sprite = icon; // เปลี่ยนรูป
                }
                else // ถ้าไม่ได้ตั้งรูปไว้
                {
                    vehicleIconImage.gameObject.SetActive(false); // ซ่อนรูป
                }
            }
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}