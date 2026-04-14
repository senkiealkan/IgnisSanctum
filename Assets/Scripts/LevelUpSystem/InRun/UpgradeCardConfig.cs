using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradeCard", menuName = "Game/In-Run/Upgrade Card")]
public class UpgradeCardConfig : ScriptableObject
{
    public string cardName;
    public string description;
    public Sprite icon;
    public UpgradeType type;
    public float value;
}
public enum UpgradeType
{
    MaxHealth, //Tăng máu tối đa
    DamageMultiplier, //Tăng dam
    MoveSpeed, //Tốc độ di chuyển
    MaxMana, //Mana tối đa
    CooldownReduction,  // Giảm thời gian hồi chiêu
    ManaRegen,          // Tăng tốc độ hồi Mana
    CriticalChance,     // Tỷ lệ chí mạng
    ManaCostReduction,   // Giảm tiêu hao Mana
    AreaScale //Tăng phạm vi ảnh hưởng
}