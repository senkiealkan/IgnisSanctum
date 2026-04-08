using UnityEngine;
using UnityEngine.UI; // Để dùng Image

public class SkillHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerMagic playerMagic; // Kéo thả Player vào đây

    [Header("Skill 1 (Fireball)")]
    public Image skill1Overlay; // Kéo cái Cooldown_Overlay của Fireball vào

    [Header("Skill 2 (Tornado)")]
    public Image skill2Overlay; // Kéo cái Cooldown_Overlay của Tornado vào

    [Header("Skill 3 (Explosion)")]
    public Image skill3Overlay; // Kéo cái Cooldown_Overlay của Explosion vào

    void Update()
    {
        if (playerMagic == null) return;

        // --- CÔNG THỨC: FillAmount = Thời gian còn lại / Tổng thời gian hồi ---

        // Skill 1: Fireball
        if (skill1Overlay != null)
        {
            // Nếu timer > 0 thì hiển thị tỷ lệ, nếu = 0 thì fillAmount = 0 (trong suốt)
            skill1Overlay.fillAmount = playerMagic.fireballTimer / playerMagic.fireballCooldown;
        }

        // Skill 2: Tornado
        if (skill2Overlay != null)
        {
            skill2Overlay.fillAmount = playerMagic.tornadoTimer / playerMagic.tornadoCooldown;
        }

        // Skill 3: Explosion
        if (skill3Overlay != null)
        {
            skill3Overlay.fillAmount = playerMagic.explosionTimer / playerMagic.explosionCooldown;
        }
    }
}