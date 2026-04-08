using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // ========================================================================
    // 1. CONFIGURATION (Chỉ số cơ bản cài đặt trong Inspector)
    // ========================================================================
    [Header("Base Config - Survival")]
    [SerializeField] private float configBaseHealth = 100f;
    [SerializeField] private float configBaseMoveSpeed = 12f;
    [SerializeField] private int configBaseDashCount = 1;

    [Header("Base Config - Combat")]
    [SerializeField] private float configBaseDamage = 10f;

    [Header("Base Config - Magic")]
    [SerializeField] private float configBaseMaxMana = 100f;

    // Skill 0: Fireball
    [SerializeField] private float configBaseFireDamage = 15f;
    [SerializeField] private float configBaseFireCost = 5f;

    // Skill 1: Tornado
    [SerializeField] private float configBaseTornadoDamage = 25f;
    [SerializeField] private float configBaseTornadoCost = 15f;

    // Skill 2: Explosion
    [SerializeField] private float configBaseExplosionDamage = 40f;
    [SerializeField] private float configBaseExplosionCost = 30f;

    // ========================================================================
    // 2. REAL BASE STATS (Config + Meta Progression)
    // ========================================================================
    public float RealBaseMaxHealth { get; private set; }
    public float RealBaseDamage { get; private set; }
    public int RealBaseDashCount { get; private set; }
    public float RealBaseMaxMana { get; private set; }
    public float MetaManaRegenBonus { get; private set; }
    public float MetaCritChanceBonus { get; private set; }

    // ========================================================================
    // 3. IN-RUN BONUSES (Chỉ số cộng thêm tạm thời trong màn chơi)
    // ========================================================================
    private float maxHealthBonus_InRun = 0f;
    private float damageMultiplier_InRun = 1f;
    private float moveSpeedBonus_InRun = 0f;
    private float damageResistance_InRun = 0f;

    // 1. KHAI BÁO BIẾN IN-RUN MỚI
    private float cooldownReduction_InRun = 0f;

    // [FIX 2] Sửa mặc định thành 0f. 
    // Vì công thức là Cộng dồn (Add), nên mặc định phải là 0.
    // Nếu để 1, công thức (1 + Meta + InRun) sẽ thành (1 + 0 + 1) = x2 ngay từ đầu game.
    private float manaRegenMultiplier_InRun = 0f;

    private float criticalChance_InRun = 0.01f;
    private float areaScale_InRun = 5f;

    // Magic Bonuses
    private float maxManaBonus_InRun = 0f;
    private float fireDamageMultiplier_InRun = 1f;
    private float fireCostReduction_InRun = 0f;

    // ========================================================================
    // 4. FINAL STATS (GETTERS)
    // ========================================================================
    public float MaxHealth => RealBaseMaxHealth + maxHealthBonus_InRun;
    public float TotalDamage => RealBaseDamage * damageMultiplier_InRun;
    public float TotalMoveSpeed => configBaseMoveSpeed + moveSpeedBonus_InRun;
    public int MaxDashCount => RealBaseDashCount;
    public float CooldownMultiplier => Mathf.Max(0.1f, 1f - cooldownReduction_InRun);

    // Công thức: 1 (Gốc) + Meta (ví dụ 0.5) + InRun (ví dụ 0.5) = 2.0 (x2 Tốc độ)
    public float ManaRegenMultiplier => 1f + MetaManaRegenBonus + manaRegenMultiplier_InRun;

    public float CriticalChance => 0.01f + MetaCritChanceBonus + criticalChance_InRun;
    public float AreaScale => areaScale_InRun;

    // Magic Getters
    public float MaxMana => RealBaseMaxMana + maxManaBonus_InRun;

    public float FireballDamage => (configBaseFireDamage * fireDamageMultiplier_InRun) + MetaProgressionManager.Instance.GetMetaFireDamageBonus();
    public float FireballCost => Mathf.Max(1f, configBaseFireCost - fireCostReduction_InRun);

    public float TornadoDamage => (configBaseTornadoDamage * fireDamageMultiplier_InRun) + MetaProgressionManager.Instance.GetMetaTornadoBonus();
    public float TornadoCost => Mathf.Max(1f, configBaseTornadoCost - fireCostReduction_InRun);

    public float ExplosionDamage => (configBaseExplosionDamage * fireDamageMultiplier_InRun) + MetaProgressionManager.Instance.GetMetaExplosionBonus();
    public float ExplosionCost => Mathf.Max(1f, configBaseExplosionCost - fireCostReduction_InRun);

    public event Action OnStatsChanged;

    // [FIX 1] Đổi từ Awake() sang Start()
    // Lý do: MetaProgressionManager khởi tạo trong Awake. 
    // Nếu PlayerStats cũng chạy Awake và nhanh hơn MetaManager, nó sẽ không tìm thấy Instance và load sai dữ liệu.
    // Chuyển sang Start đảm bảo MetaManager đã sẵn sàng.
    private void Start()
    {
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        if (MetaProgressionManager.Instance != null)
        {
            var meta = MetaProgressionManager.Instance;

            RealBaseMaxHealth = configBaseHealth + meta.GetMetaHealthBonus();
            RealBaseDamage = configBaseDamage + meta.GetMetaDamageBonus();
            RealBaseDashCount = configBaseDashCount + meta.GetMetaDashCountBonus();

            RealBaseMaxMana = configBaseMaxMana + meta.GetMetaMaxManaBonus();
            MetaManaRegenBonus = meta.GetMetaManaRegenBonus();
            MetaCritChanceBonus = meta.GetMetaCritChanceBonus();
            Debug.Log($"PlayerStats Updated! MetaManaRegen: {MetaManaRegenBonus * 100}%");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy MetaProgressionManager! Sử dụng chỉ số mặc định.");
            RealBaseMaxHealth = configBaseHealth;
            RealBaseDamage = configBaseDamage;
            RealBaseDashCount = configBaseDashCount;
            RealBaseMaxMana = configBaseMaxMana;

            // [FIX 3] Sửa fallback về 0. 
            // Nếu không có Meta, bonus phải là 0. Trước đây để 1f là sai (thành +100%).
            MetaManaRegenBonus = 0f;
            MetaCritChanceBonus = 0f;
        }

        OnStatsChanged?.Invoke();
    }

    public void ApplyUpgrade(UpgradeCardConfig config)
    {
        switch (config.type)
        {
            case UpgradeType.MaxHealth:
                maxHealthBonus_InRun += config.value;
                break;
            case UpgradeType.DamageMultiplier:
                damageMultiplier_InRun += config.value;
                break;
            case UpgradeType.MoveSpeed:
                moveSpeedBonus_InRun += config.value;
                break;
            case UpgradeType.MaxMana:
                maxManaBonus_InRun += config.value;
                break;
            case UpgradeType.CooldownReduction:
                cooldownReduction_InRun += config.value;
                break;
            case UpgradeType.ManaRegen:
                manaRegenMultiplier_InRun += config.value;
                break;
            case UpgradeType.CriticalChance:
                criticalChance_InRun += config.value;
                break;
            case UpgradeType.ManaCostReduction:
                fireCostReduction_InRun += config.value;
                break;
            case UpgradeType.AreaScale:
                areaScale_InRun += config.value;
                break;
            default:
                Debug.LogWarning($"Loại nâng cấp chưa được xử lý: {config.type}");
                break;
        }
        OnStatsChanged?.Invoke();
    }

    public void ResetInRunStats()
    {
        maxHealthBonus_InRun = 0f;
        damageMultiplier_InRun = 1f;
        moveSpeedBonus_InRun = 0f;
        damageResistance_InRun = 0f;

        maxManaBonus_InRun = 0f;
        fireDamageMultiplier_InRun = 1f;
        fireCostReduction_InRun = 0f;

        cooldownReduction_InRun = 0f;

        // [FIX 2] Reset về 0
        manaRegenMultiplier_InRun = 0f;

        criticalChance_InRun = 0.1f;
        areaScale_InRun = 5f;

        RecalculateStats();
        Debug.Log("Player Stats (In-Run) have been RESET!");
    }

    // Các hàm Save/Load giữ nguyên nhưng lưu ý biến manaRegenMultiplier_InRun giờ bắt đầu từ 0
    public void SaveRunStats()
    {
        PlayerPrefs.SetFloat("Run_MaxHealthBonus", maxHealthBonus_InRun);
        PlayerPrefs.SetFloat("Run_DamageMult", damageMultiplier_InRun);
        PlayerPrefs.SetFloat("Run_MoveSpeedBonus", moveSpeedBonus_InRun);
        PlayerPrefs.SetFloat("Run_Resist", damageResistance_InRun);
        PlayerPrefs.SetFloat("Run_MaxManaBonus", maxManaBonus_InRun);
        PlayerPrefs.SetFloat("Run_FireDamageMult", fireDamageMultiplier_InRun);
        PlayerPrefs.SetFloat("Run_FireCostReduct", fireCostReduction_InRun);

        // Sếp có thể thêm Save cho các biến mới nếu cần continue game
    }

    public void LoadRunStats()
    {
        maxHealthBonus_InRun = PlayerPrefs.GetFloat("Run_MaxHealthBonus", 0f);
        damageMultiplier_InRun = PlayerPrefs.GetFloat("Run_DamageMult", 1f);
        moveSpeedBonus_InRun = PlayerPrefs.GetFloat("Run_MoveSpeedBonus", 0f);
        damageResistance_InRun = PlayerPrefs.GetFloat("Run_Resist", 0f);
        maxManaBonus_InRun = PlayerPrefs.GetFloat("Run_MaxManaBonus", 0f);
        fireDamageMultiplier_InRun = PlayerPrefs.GetFloat("Run_FireDamageMult", 1f);
        fireCostReduction_InRun = PlayerPrefs.GetFloat("Run_FireCostReduct", 0f);

        RecalculateStats();
    }
}