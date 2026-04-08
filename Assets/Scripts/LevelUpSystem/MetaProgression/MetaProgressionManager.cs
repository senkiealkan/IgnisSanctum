using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager Instance;

    [Header("Currency")]
    public int statGems; // Rune of Potency
    public int fireGems; // Rune of Ignis
    [Header("Current Run Loot (Túi tạm thời)")]
    public int currentRunStatGems; // [NEW] Gem nhặt trong màn hiện tại
    public int currentRunFireGems; // [NEW]

    [Header("General Upgrades")]
    public int healthLevel = 0;
    public int damageLevel = 0;
    public int critChanceLevel = 0;
    public int dashCountLevel = 0;

    [Header("Fire Skill Upgrades")]
    public int maxManaLevel = 0;    
    public int manaRegenLevel = 0;
    public int fireDamageLevel = 0;     
    public int tornadoLevel = 0;        
    public int explosionLevel = 0;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi chuyển scene
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- LƯU & TẢI DỮ LIỆU (Dùng PlayerPrefs) ---
    public void SaveData()
    {
        PlayerPrefs.SetInt("StatGems", statGems);
        PlayerPrefs.SetInt("FireGems", fireGems);

        PlayerPrefs.SetInt("HealthLevel", healthLevel);
        PlayerPrefs.SetInt("DamageLevel", damageLevel);
        PlayerPrefs.SetInt("DashCountLevel", dashCountLevel);

        // Save Fire Skills
        PlayerPrefs.SetInt("FireDamageLevel", fireDamageLevel);
        //PlayerPrefs.SetInt("FireManaCostLevel", fireManaCostLevel);
        PlayerPrefs.SetInt("TornadoLevel", tornadoLevel);
        PlayerPrefs.SetInt("ExplosionLevel", explosionLevel);

        PlayerPrefs.SetInt("MaxManaLevel", maxManaLevel);
        PlayerPrefs.SetInt("ManaRegenLevel", manaRegenLevel);
        PlayerPrefs.SetInt("CritChanceLevel", critChanceLevel);
        PlayerPrefs.Save();
    }
    // Xóa toàn bộ dữ liệu
    public void ResetData()
    {
        // 1. Reset tiền tệ
        statGems = 0;
        fireGems = 0;

        // 2. Reset các cấp độ nâng cấp
        healthLevel = 0;
        damageLevel = 0;
        dashCountLevel = 0;

        fireDamageLevel = 0;
        tornadoLevel = 0;
        explosionLevel = 0;

        maxManaLevel = 0;
        manaRegenLevel = 0;
        critChanceLevel = 0;

        // 3. Quan trọng: Lưu lại trạng thái "về mo" này vào máy ngay
        SaveData();

 
        Debug.Log("Data has been reset for New Game!");
    }
    public void LoadData()
    {
        statGems = PlayerPrefs.GetInt("StatGems", 0);
        fireGems = PlayerPrefs.GetInt("FireGems", 0);

        healthLevel = PlayerPrefs.GetInt("HealthLevel", 0);
        damageLevel = PlayerPrefs.GetInt("DamageLevel", 0);
        dashCountLevel = PlayerPrefs.GetInt("DashCountLevel", 0);

        fireDamageLevel = PlayerPrefs.GetInt("FireDamageLevel", 0);
        tornadoLevel = PlayerPrefs.GetInt("TornadoLevel", 0);
        explosionLevel = PlayerPrefs.GetInt("ExplosionLevel", 0);

        maxManaLevel = PlayerPrefs.GetInt("MaxManaLevel", 0);
        manaRegenLevel = PlayerPrefs.GetInt("ManaRegenLevel", 0);
        critChanceLevel = PlayerPrefs.GetInt("CritChanceLevel", 0);
    }

    // --- API để các script khác gọi ---

    public void CommitRunLoot()
    {
        statGems += currentRunStatGems;
        fireGems += currentRunFireGems;

        // Reset túi tạm
        currentRunStatGems = 0;
        currentRunFireGems = 0;

        // Bây giờ mới lưu xuống ổ cứng
        SaveData();
        Debug.Log("Đã chốt sổ Gem vào kho!");
    }
    public void CollectGem(GemType type, int amount)
    {
        if (type == GemType.StatGem)
        {
            currentRunStatGems += amount;
            // Update UI hiển thị tổng (Gốc + Vừa nhặt)
            Debug.Log($"Nhặt {amount} StatGem. Túi tạm: {currentRunStatGems}");
        }
        else if (type == GemType.FireGem)
        {
            currentRunFireGems += amount;
        }

        // [QUAN TRỌNG] KHÔNG GỌI SaveData() Ở ĐÂY NỮA!
    }
    // --- HÀM HỦY SỔ (Gọi khi Give Up) ---
    public void DiscardRunLoot()
    {
        currentRunStatGems = 0;
        currentRunFireGems = 0;
        Debug.Log("Đã hủy toàn bộ Gem nhặt trong Run này!");
    }

    // ... (Giữ nguyên SaveData, LoadData, ResetData cũ) ...

    public int GetDisplayStatGems() => statGems + currentRunStatGems;
    public int GetDisplayFireGems() => fireGems + currentRunFireGems;

    // 2. Tính chỉ số cộng thêm từ Meta
    public float GetMetaHealthBonus() => healthLevel * 10f; // Mỗi level tăng 10 HP
    public float GetMetaDamageBonus() => damageLevel * 2f;  // Mỗi level tăng 2 DMG
    public int GetMetaDashCountBonus() => dashCountLevel;   // Mỗi level thêm 1 lần lướt
    public float GetMetaFireDamageBonus() => fireDamageLevel * 5f;  // Ví dụ: Mỗi cấp tăng 5 damage
    public float GetMetaTornadoBonus() => tornadoLevel * 8f; 
    public float GetMetaExplosionBonus() => explosionLevel * 15f;

    public float GetMetaMaxManaBonus() => maxManaLevel * 10f;         // +10 Mana mỗi cấp
    public float GetMetaManaRegenBonus() => manaRegenLevel * 0.25f;    // +20% tốc độ hồi mỗi cấp
    public float GetMetaCritChanceBonus() => critChanceLevel * 0.025f;
}

public enum GemType
{
    StatGem, // Rune of Potency
    FireGem  // Rune of Ignis
}