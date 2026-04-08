using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem; // Dùng Input System mới
using UnityEngine.SceneManagement;
public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Data")]
    public int hpPotions = 0;
    public int manaPotions = 0;

    [Header("Settings")]
    public float healAmount = 30f; // Hồi 30 HP
    public float manaRestoreAmount = 20f; // Hồi 20 Mana

    [Header("UI References")]
    public TextMeshProUGUI hpPotionText;
    public TextMeshProUGUI manaPotionText;

    [Header("References")]
    public PlayerHealth playerHealth;
    public PlayerMana playerMana;

    [Header("Effects")]
    public GameObject healHPEffect;
    public GameObject healManaEffect;
    private AudioSource audioSource;
    public AudioClip usePotionSound;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        UpdateUI();
    }

    private void Update()
    {
        // Kiểm tra phím Q (Hồi máu)
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            UseHpPotion();
        }

        // Kiểm tra phím E (Hồi Mana)
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            UseManaPotion();
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // [ADD] Hàm này chạy mỗi khi load lại màn (Rebirth)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Tìm lại UI Text trong scene mới 
        // Ví dụ: GameObject tên là "Text_HP_Potion" và "Text_Mana_Potion" trong Canvas
        GameObject hpTextObj = GameObject.Find("Text_HP_Potion"); 
        GameObject manaTextObj = GameObject.Find("Text_Mana_Potion"); 

        if (hpTextObj != null) hpPotionText = hpTextObj.GetComponent<TextMeshProUGUI>();
        if (manaTextObj != null) manaPotionText = manaTextObj.GetComponent<TextMeshProUGUI>();

        // 2. Cập nhật lại UI
        UpdateUI();
    }

    // [ADD] Hàm Reset dùng cho Rebirth
    public void ResetInventory()
    {
        hpPotions = 0;
        manaPotions = 0;
        UpdateUI();
    }

    // ... (Giữ nguyên các hàm AddPotion, UseHpPotion... bên dưới) ...

public void AddPotion(PotionType type, int amount)
    {
        if (type == PotionType.HpPotion)
        {
            hpPotions += amount;
        }
        else
        {
            manaPotions += amount;
        }
        UpdateUI();
    }

    private void UseHpPotion()
    {
        // Điều kiện: Có thuốc VÀ máu chưa đầy VÀ chưa chết
        if (hpPotions > 0 && playerHealth.currentHealth < playerHealth.stats.MaxHealth && !playerHealth.isDead)
        {
            hpPotions--;

            // Gọi hàm hồi máu bên PlayerHealth (Sếp cần thêm hàm Heal public nhé)
            AudioSource.PlayClipAtPoint(usePotionSound, Camera.main.transform.position, 1f);
            playerHealth.Heal(healAmount);
            Instantiate(healHPEffect, transform.position, Quaternion.identity);
            
            UpdateUI();
            Debug.Log("Used HP Potion");
        }
        else
        {
            Debug.Log("Cannot use HP Potion (Empty, Full HP, or Dead)");
        }
    }

    private void UseManaPotion()
    {
        if (manaPotions > 0 && playerMana.currentMana < playerMana.stats.MaxMana)
        {
            manaPotions--;
            AudioSource.PlayClipAtPoint(usePotionSound, Camera.main.transform.position, 1f);
            playerMana.RestoreMana(manaRestoreAmount);
            Instantiate(healManaEffect, transform.position, Quaternion.identity);
            UpdateUI();
            Debug.Log("Used Mana Potion");
        }
    }

    public void UpdateUI()
    {
        if (hpPotionText != null) hpPotionText.text = hpPotions.ToString();
        if (manaPotionText != null) manaPotionText.text = manaPotions.ToString();
    }
}

public enum PotionType
{
    HpPotion,
    ManaPotion
}