using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    // Tạo Singleton để Boss  gọi
    public static BossHealthBar Instance;

    [Header("UI References")]
    public GameObject healthBarPanel; 
    public Slider healthSlider;
    public TextMeshProUGUI bossNameText;

    private void Awake()
    {

        if (Instance == null) Instance = this;
        // Mặc định ẩn đi khi vào game
        Hide();
    }

    public void Initialize(string name, float maxHealth)
    {
        if (healthBarPanel != null) healthBarPanel.SetActive(true);
        if (bossNameText != null) bossNameText.text = name;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void UpdateHealth(float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    public void Hide()
    {
        if (healthBarPanel != null) healthBarPanel.SetActive(false);
    }
}