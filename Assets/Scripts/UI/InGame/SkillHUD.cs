using UnityEngine;
using UnityEngine.UI; // Để dùng Image

public class SkillHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerMagic playerMagic; 

    [Header("Skill 1 (Fireball)")]
    public Image skill1Overlay; 

    [Header("Skill 2 (Tornado)")]
    public Image skill2Overlay; 

    [Header("Skill 3 (Explosion)")]
    public Image skill3Overlay; 

    void Update()
    {
        if (playerMagic == null) return;

        // --- CÔNG THỨC: FillAmount = Thời gian còn lại / Tổng thời gian hồi ---

        // Skill 1: Fireball
        if (skill1Overlay != null)
        {
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