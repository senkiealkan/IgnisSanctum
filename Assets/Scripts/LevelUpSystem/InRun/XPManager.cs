using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class XPManager : MonoBehaviour
{
    public TextMeshProUGUI xpText;
    public GameObject cardSelectMenuPanel;
    public Slider xpBar;

    public int currentLevel = 1;
    public float currentXP = 0f; 
    public float requiredXP = 600f;
    void Update()
    {
        // Nếu menu đang bật mà game lỡ chạy (do thằng khác bật TimeScale), thì ép nó dừng lại ngay
        if (cardSelectMenuPanel != null && cardSelectMenuPanel.activeSelf)
        {
            if (Time.timeScale != 0f)
            {
                Time.timeScale = 0f;
            }
        }
    }

    // --- Phương thức để nhận XP từ sát thương ---
    public void GainXP(float amount)
    {
        currentXP += amount;
        UpdateXPBar();

        if (currentXP >= requiredXP)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;

        // 1. Reset XP, tính toán XP cần thiết cho cấp tiếp theo
        currentXP -= requiredXP;
        requiredXP *= 1.08f;

        UpdateXPBar();

   
        OpenCardSelection();
    }

    private void UpdateXPBar()
    {
        float xpRatio = currentXP / requiredXP;

        xpBar.value = xpRatio;
        if (xpText != null)
        {
            xpText.text = $"Lv {currentLevel} XP: {Mathf.Ceil(currentXP)} / {Mathf.Ceil(requiredXP)}";
        }
    }
    public void ResetXP()
    {
        currentLevel = 1;
        currentXP = 0f;
        requiredXP = 600f;
        UpdateXPBar();
    }
    public void LoadXP(int level, float xp, float reqXP)
    {
        currentLevel = level;
        currentXP = xp;
        requiredXP = reqXP;
        UpdateXPBar();
    }

    // --- Quản lý Card ---
    private void OpenCardSelection()
    {
        Time.timeScale = 0f;
        if (cardSelectMenuPanel != null)
        {
            cardSelectMenuPanel.SetActive(true);

            CardMenuManager cardManager = cardSelectMenuPanel.GetComponent<CardMenuManager>();
            if (cardManager != null)
            {
                cardManager.DisplayCards();
            }
        }
    }

    public void CardSelected()
    {
        cardSelectMenuPanel.SetActive(false);

        // Kiểm tra xem XP hiện tại có đủ để lên cấp tiếp không?
        if (currentXP >= requiredXP)
        {
            // Nếu đủ thì Level Up tiếp (Menu sẽ tự bật lại, TimeScale lại về 0)
            LevelUp();
        }
        else
        {
            // Nếu không thì mới cho game chạy tiếp
            Time.timeScale = 1f;
        }
    }
}