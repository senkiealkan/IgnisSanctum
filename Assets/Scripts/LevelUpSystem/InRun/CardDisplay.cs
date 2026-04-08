using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Cần thư viện này để bắt sự kiện chuột

// Thêm Interface IPointerEnterHandler để nhận biết khi chuột lướt qua
public class CardDisplay : MonoBehaviour, IPointerEnterHandler
{
    [Header("UI Components")]
    public TextMeshProUGUI cardTitleText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;

    [Header("Audio")]
    public AudioClip hoverSound; // Kéo file âm thanh vào đây
    public AudioClip clickSound; // Kéo file âm thanh vào đây
    private AudioSource audioSource;

    private UpgradeCardConfig currentConfig;
    private CardMenuManager manager;
    private Button button;

    private void Awake()
    {
        // Tự động thêm loa (AudioSource) nếu quên thêm
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Setup(UpgradeCardConfig config, CardMenuManager menuManager)
    {
        currentConfig = config;
        manager = menuManager;

        // 1. Cấu hình UI Text
        cardTitleText.text = config.cardName;
        descriptionText.text = config.description;
        if (config.icon != null) iconImage.sprite = config.icon;

        // 2. Gán hàm cho Button
        button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnCardSelected);
    }

    // --- Xử lý Hover (Lướt chuột) ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound); // Phát tiếng "bíp" khi lướt qua
        }
    }

    // --- Xử lý Click (Chọn Card) ---
    private void OnCardSelected()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound); // Phát tiếng "cạch" khi chọn
        }

        if (manager != null && currentConfig != null)
        {
            manager.ApplyUpgrade(currentConfig);
        }
    }
}