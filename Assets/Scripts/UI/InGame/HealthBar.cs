using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI hpText;
    public RectTransform rootRectTransform;

    [Header("Scaling Settings")]
    public float baseMaxHealth = 100f;
    private float maxScaleFactor = 3f;
    private Vector3 initialScale;

    void Awake()
    {
        if (rootRectTransform != null)
        {
            initialScale = rootRectTransform.localScale;
        }
    }
    void Start()
    {
        if (PlayerHealth.Instance != null)
        {

            // 2. Cập nhật ngay hiển thị theo chỉ số hiện tại của Player
            SetMaxHealth(PlayerHealth.Instance.maxHealth);
            SetHealth(PlayerHealth.Instance.currentHealth);
        }
    }
    public void SetMaxHealth(float health)
    {
        healthBar.maxValue = health;
        UpdateScale(health);

        // Cập nhật text với giá trị hiện tại của slider và max health mới
        UpdateText(healthBar.value, health);
    }

    public void SetHealth(float health)
    {
        healthBar.value = health;

        // [FIXED] Lỗi ở đây: Trước đây sếp truyền (health, health)
        // Bây giờ sửa thành (health, healthBar.maxValue) để lấy MaxHealth từ Slider
        UpdateText(health, healthBar.maxValue);
    }

    private void UpdateText(float current, float max)
    {
        if (hpText != null)
        {
            hpText.text = $"{Mathf.Ceil(current)} / {Mathf.Ceil(max)}";
        }
    }

    private void UpdateScale(float newMaxHealth)
    {
        if (rootRectTransform == null) return;
        if (initialScale == Vector3.zero)
        {
            initialScale = rootRectTransform.localScale;
        }
        float healthRatio = newMaxHealth / baseMaxHealth;
        float scaleFactor = Mathf.Min(healthRatio, maxScaleFactor);

        Vector3 targetScale = new Vector3(
            initialScale.x * scaleFactor,
            initialScale.y,
            initialScale.z
        );

        // [OPTIMIZED] Dừng Coroutine cũ nếu đang chạy để tránh xung đột
        StopAllCoroutines();
        
        StartCoroutine(SmoothScale(targetScale, 0.5f));
    }

    IEnumerator SmoothScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = rootRectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Sếp nhớ dùng Time.unscaledDeltaTime nếu muốn animation chạy kể cả khi pause game (tùy chọn)
            rootRectTransform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rootRectTransform.localScale = targetScale;
    }
}