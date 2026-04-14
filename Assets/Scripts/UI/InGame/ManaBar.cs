using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class ManaBar : MonoBehaviour
{
    public Slider manaSlider;
    public TextMeshProUGUI manaText;

    public RectTransform rootRectTransform; 

    [Header("Scaling Settings")]
    public float baseMaxMana = 100f; // Mana cơ bản ban đầu
    private float maxScaleFactor = 3f; // Giới hạn độ dài
    private Vector3 initialScale;

    private void Awake()
    {
        // Lưu scale gốc
        if (rootRectTransform != null)
        {
            initialScale = rootRectTransform.localScale;
        }
    }
    private void Start()
    {
        // Dùng PlayerHealth.Instance làm neo để tìm PlayerMana 
        if (PlayerHealth.Instance != null)
        {
            PlayerMana pMana = PlayerHealth.Instance.GetComponent<PlayerMana>();
            if (pMana != null)
            {
          
                SetMaxMana(pMana.stats.MaxMana); // Lấy Max từ Stats
                SetMana(pMana.currentMana);      // Lấy Current từ PlayerMana
            }
        }
    }
    public void SetMaxMana(float maxMana)
    {
        if (manaSlider != null)
        {
            manaSlider.maxValue = maxMana;

            // Gọi hàm Scale khi Max Mana thay đổi
            UpdateScale(maxMana);

            UpdateText(manaSlider.value, maxMana);
        }
    }

    public void SetMana(float currentMana)
    {
        if (manaSlider != null)
        {
            manaSlider.value = currentMana;
            UpdateText(currentMana, manaSlider.maxValue);
        }
    }

    private void UpdateText(float current, float max)
    {
        if (manaText != null)
        {
            manaText.text = $"{Mathf.FloorToInt(current)} / {Mathf.FloorToInt(max)}";
        }
    }

    // Logic Scale tương tự HealthBar
    private void UpdateScale(float newMaxMana)
    {
        if (rootRectTransform == null) return;
        if (initialScale == Vector3.zero)
        {
            initialScale = rootRectTransform.localScale;
        }
        float manaRatio = newMaxMana / baseMaxMana;
        float scaleFactor = Mathf.Min(manaRatio, maxScaleFactor);

        Vector3 targetScale = new Vector3(
            initialScale.x * scaleFactor,
            initialScale.y,
            initialScale.z
        );

        StopAllCoroutines();
        if (rootRectTransform != null)
        {
            StartCoroutine(SmoothScale(targetScale, 0.5f));
        }
    }

    IEnumerator SmoothScale(Vector3 targetScale, float duration)
    {
        Vector3 startScale = rootRectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            rootRectTransform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rootRectTransform.localScale = targetScale;
    }
}