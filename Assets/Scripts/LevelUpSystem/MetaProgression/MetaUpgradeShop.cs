using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MetaUpgradeShop : MonoBehaviour
{
    [Header("Stat Gem UI")]
    public TextMeshProUGUI statGemText;

    // Health
    public TextMeshProUGUI healthLevelText;
    public TextMeshProUGUI healthCostText; 
    public Button healthBuyButton; // Để disable nút nếu không đủ tiền

    // Damage
    public TextMeshProUGUI damageLevelText;
    public TextMeshProUGUI damageCostText;
    public Button damageBuyButton;

    // Crit Chance
    public TextMeshProUGUI critLevelText;
    public TextMeshProUGUI critCostText;
    public Button critBuyButton;

    // Dash
    public TextMeshProUGUI dashLevelText;
    public TextMeshProUGUI dashCostText;
    public Button dashBuyButton;

    [Header("Fire Gem UI")]
    public TextMeshProUGUI fireGemText;
    //Mana
    public TextMeshProUGUI manaLevelText;
    public TextMeshProUGUI manaCostText;
    public Button manaBuyButton;

    // Mana Regen
    public TextMeshProUGUI regenLevelText;
    public TextMeshProUGUI regenCostText;
    public Button regenBuyButton;
    // Fireball
    public TextMeshProUGUI fireDamageLevelText, fireDamageCostText;
    public Button fireDamageBuyButton;

    // Tornado
    public TextMeshProUGUI tornadoLevelText, tornadoCostText;
    public Button tornadoBuyButton;

    // Explosion
    public TextMeshProUGUI explosionLevelText, explosionCostText;
    public Button explosionBuyButton;


    [Header("Cost Config")]
    public int baseHealthCost = 10;
    public int baseDamageCost = 10;
    public int baseCritCost = 25;
    public int baseDashCost = 50;


    public int baseManaCost = 10;
    public int baseRegenCost = 15;
    public int baseFireDamageCost = 10;
    public int baseTornadoCost = 20;   
    public int baseExplosionCost = 50;


    private void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (MetaProgressionManager.Instance == null) return;
        var manager = MetaProgressionManager.Instance;

        // 1. Hiển thị số tiền hiện có
        statGemText.text = manager.statGems.ToString();
        fireGemText.text = manager.fireGems.ToString();

        // 2. Cập nhật từng mục nâng cấp (Level, Cost, Button Interactable)
        //StatGem Update
        UpdateUpgradeSlot(
            manager.healthLevel, baseHealthCost, manager.statGems,
            healthLevelText, healthCostText, healthBuyButton, "Max HP"
        );

        UpdateUpgradeSlot(
            manager.damageLevel, baseDamageCost, manager.statGems,
            damageLevelText, damageCostText, damageBuyButton, "Damage"
        );
        UpdateUpgradeSlot(manager.critChanceLevel, baseCritCost, manager.statGems,
            critLevelText, critCostText, critBuyButton, "Crit Chance");
        UpdateUpgradeSlot(
            manager.dashCountLevel, baseDashCost, manager.statGems,
            dashLevelText, dashCostText, dashBuyButton, "Dash Count"
        );

        //FireGem Update
        UpdateUpgradeSlot(
            manager.maxManaLevel, baseManaCost, manager.fireGems,
            manaLevelText, manaCostText, manaBuyButton, "Max Mana"
        );
        UpdateUpgradeSlot(
            manager.manaRegenLevel, baseRegenCost, manager.fireGems,
            regenLevelText, regenCostText, regenBuyButton, "Mana Regen"
        );

        UpdateUpgradeSlot(
            manager.fireDamageLevel, baseFireDamageCost, manager.fireGems,
            fireDamageLevelText, fireDamageCostText, fireDamageBuyButton, "Fire DMG"
        );
        UpdateUpgradeSlot(
            manager.tornadoLevel, baseTornadoCost, manager.fireGems, 
            tornadoLevelText, tornadoCostText, tornadoBuyButton, "Tornado"
        );
        UpdateUpgradeSlot(
            manager.explosionLevel, baseExplosionCost, manager.fireGems, 
            explosionLevelText, explosionCostText, explosionBuyButton, "Explosion"
        );
    }

    // Hàm phụ trợ để code gọn hơn (xử lý hiển thị cho 1 slot)
    private void UpdateUpgradeSlot(int currentLevel, int baseCost, int currentCurrency,
                                   TextMeshProUGUI levelText, TextMeshProUGUI costText, Button buyButton, string label)
    {
        int cost = CalculateCost(baseCost, currentLevel);

        // Hiển thị Level
        levelText.text = $"{label} Lv.{currentLevel}";

        // Hiển thị Giá
        costText.text = cost.ToString();

        // Kiểm tra đủ tiền không -> Đổi màu nút hoặc Disable
        if (buyButton != null)
        {
            bool canBuy = currentCurrency >= cost;

            // 1. Set trạng thái nút
            buyButton.interactable = canBuy;

            // 2. Set trạng thái EventTrigger theo biến canBuy
            EventTrigger trigger = buyButton.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                trigger.enabled = canBuy;
            }
        }
    }

    private int CalculateCost(int baseCost, int currentLevel)
    {
        // Công thức: Giá tăng dần theo cấp độ
        return baseCost * (currentLevel + 1);
    }

    // --- CÁC HÀM MUA ---

    public void BuyHealthUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseHealthCost, manager.healthLevel);

        if (manager.statGems >= cost)
        {
            manager.statGems -= cost;
            manager.healthLevel++;
            manager.SaveData();
            UpdateUI(); 
        }
    }

    public void BuyDamageUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseDamageCost, manager.damageLevel);

        if (manager.statGems >= cost)
        {
            manager.statGems -= cost;
            manager.damageLevel++;
            manager.SaveData();
            UpdateUI();
        }
    }

    public void BuyCritUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseCritCost, manager.critChanceLevel);
        if (manager.statGems >= cost)
        {
            manager.statGems -= cost;
            manager.critChanceLevel++;
            manager.SaveData();
            UpdateUI();
        }
    }
    public void BuyDashUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseDashCost, manager.dashCountLevel);

        if (manager.statGems >= cost)
        {
            manager.statGems -= cost;
            manager.dashCountLevel++;
            manager.SaveData();
            UpdateUI();
        }
    }

    // Nâng cấp Fire Damage (Dùng Fire Gem)
    public void BuyManaUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseManaCost, manager.maxManaLevel);
        if (manager.fireGems >= cost)
        {
            manager.fireGems -= cost;
            manager.maxManaLevel++;
            manager.SaveData();
            UpdateUI();
        }
    }

    public void BuyRegenUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseRegenCost, manager.manaRegenLevel);
        if (manager.fireGems >= cost)
        {
            manager.fireGems -= cost;
            manager.manaRegenLevel++;
            manager.SaveData();
            UpdateUI();
        }
    }
    public void BuyFireDamageUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseFireDamageCost, manager.fireDamageLevel);

        if (manager.fireGems >= cost) 
        {
            manager.fireGems -= cost;
            manager.fireDamageLevel++;
            manager.SaveData();
            UpdateUI();
        }
    }
    public void BuyTornadoUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseTornadoCost, manager.tornadoLevel);

        if (manager.fireGems >= cost)
        {
            manager.fireGems -= cost;
            manager.tornadoLevel++;
            manager.SaveData();
            UpdateUI();
        }
    }

    public void BuyExplosionUpgrade()
    {
        var manager = MetaProgressionManager.Instance;
        int cost = CalculateCost(baseExplosionCost, manager.explosionLevel);

        if (manager.fireGems >= cost)
        {
            manager.fireGems -= cost;
            manager.explosionLevel++;
            manager.SaveData();
            UpdateUI();
        }
    }
}