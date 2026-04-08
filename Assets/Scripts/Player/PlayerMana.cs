using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("References")]
    public PlayerStats stats;
    public ManaBar manaBar;

    [Header("Settings")]
    public float manaRegenRate = 1.5f;

    // Biến nội bộ
    public float currentMana;
    private float localMaxMana; 

    private void Start()
    {
        if (stats == null) stats = GetComponent<PlayerStats>();
        if (manaBar == null)
        {
            manaBar = GetComponentInChildren<ManaBar>();
        }
        // 1. Khởi tạo giá trị ban đầu
        localMaxMana = stats.MaxMana;
        currentMana = localMaxMana;

        // 2. Cài đặt UI ban đầu
        InitUI();

        // 3. Đăng ký sự kiện
        stats.OnStatsChanged += UpdateManaStats;
    }

    private void OnDestroy()
    {
        if (stats != null) stats.OnStatsChanged -= UpdateManaStats;
    }

    private void Update()
    {
        // Logic hồi Mana tự động
        if (currentMana < stats.MaxMana)
        {
            float finalRegenRate = manaRegenRate * stats.ManaRegenMultiplier;
            currentMana += finalRegenRate * Time.deltaTime;

            if (currentMana > stats.MaxMana) currentMana = stats.MaxMana;
            if (manaBar != null) manaBar.SetMana(currentMana);
        }
    }

    private void InitUI()
    {
        if (manaBar != null)
        {
            manaBar.SetMaxMana(stats.MaxMana);
            manaBar.SetMana(currentMana);
        }
    }

    // [FIX] Hàm này xử lý logic khi có nâng cấp
    private void UpdateManaStats()
    {
        float newMaxMana = stats.MaxMana;

        // Tính lượng chênh lệch (Ví dụ: 50 -> 60, diff = 10)
        float diff = newMaxMana - localMaxMana;

        // Cập nhật lại biến theo dõi cục bộ
        localMaxMana = newMaxMana;

        // Cập nhật Slider Max Value
        if (manaBar != null)
        {
            manaBar.SetMaxMana(localMaxMana);
        }

        // Logic: Tăng bao nhiêu Max Mana thì hồi bấy nhiêu Current Mana
        if (diff > 0)
        {
            RestoreMana(diff);
            Debug.Log($"Upgrade MaxMana: +{diff} Mana restored.");
        }
    }

    public bool TryUseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            if (manaBar != null) manaBar.SetMana(currentMana);
            return true;
        }
        return false;
    }

    public void RestoreMana(float amount)
    {
        currentMana += amount;
        // Đảm bảo không vượt quá Max hiện tại
        if (currentMana > stats.MaxMana) currentMana = stats.MaxMana;

        if (manaBar != null) manaBar.SetMana(currentMana);
    }
    public void ResetMana()
    {
        // Đồng bộ lại Max Mana (phòng trường hợp vừa nâng cấp xong)
        localMaxMana = stats.MaxMana;

        // Hồi đầy mana
        currentMana = localMaxMana;

        // Cập nhật UI nếu đã kết nối
        if (manaBar != null)
        {
            manaBar.SetMaxMana(localMaxMana);
            manaBar.SetMana(currentMana);
        }
    }
}