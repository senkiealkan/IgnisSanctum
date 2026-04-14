using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMagic : MonoBehaviour
{
    [Header("References")]
    public PlayerStats stats;
    public PlayerMana playerMana;
    public PlayerCombat playerCombat;
    public Animator playerAnimator;
    public XPManager xpManager;
    public Transform firePoint;

    [Header("Skill 1: Fireball (RMB)")]
    public GameObject fireballPrefab;
    public float fireballCooldown = 0.4f;
    public float fireballTimer = 0f;

    [Header("Skill 2: Fire Tornado (Key 1)")]
    public GameObject tornadoPrefab;
    public float tornadoCooldown = 3f; 
    public float tornadoTimer = 0f;

    [Header("Skill 3: Explosion (Key 2)")]
    public GameObject explosionPrefab;
    public float explosionCooldown = 5f; 
    public float explosionTimer = 0f;

    private void Start()
    {
        if (stats == null) stats = GetComponent<PlayerStats>();
        if (playerMana == null) playerMana = GetComponent<PlayerMana>();
        if (playerCombat == null) playerCombat = GetComponent<PlayerCombat>();
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Giảm timer cooldown
        if (fireballTimer > 0) fireballTimer -= Time.deltaTime;
        if (tornadoTimer > 0) tornadoTimer -= Time.deltaTime;
        if (explosionTimer > 0) explosionTimer -= Time.deltaTime;

        HandleMagicInput();
    }

    private void HandleMagicInput()
    {
        // Không cho cast phép khi đang chém tay
        if (playerCombat.IsAttacking) return;

        // --- SKILL 1: FIREBALL (Chuột Phải) ---
        if (Mouse.current.rightButton.isPressed) // Dùng isPressed để có thể giữ chuột bắn liên thanh
        {
            if (fireballTimer <= 0 && playerMana.TryUseMana(stats.FireballCost))
            {
                CastProjectile(fireballPrefab, stats.FireballDamage, 15f, "CastSpell"); // Speed 10 
                fireballTimer = fireballCooldown * stats.CooldownMultiplier;
            }
        }

        // --- SKILL 2: TORNADO (Phím 1) ---
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (tornadoTimer <= 0 && playerMana.TryUseMana(stats.TornadoCost))
            {
                CastTornado(); 
                tornadoTimer = tornadoCooldown * stats.CooldownMultiplier;
            }
        }

        // --- SKILL 3: EXPLOSION (Phím 2) ---
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (explosionTimer <= 0 && playerMana.TryUseMana(stats.ExplosionCost))
            {
                CastExplosionAtMouse();
                explosionTimer = explosionCooldown * stats.CooldownMultiplier;
            }
        }
    }

 
    private void CastProjectile(GameObject prefab, float damage, float speedOverride, string animTrigger)
    {
        if (playerAnimator != null) playerAnimator.SetTrigger(animTrigger);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePos - firePoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        GameObject projObj = Instantiate(prefab, firePoint.position, rotation);

        // Cấu hình đạn
        FireballProjectile script = projObj.GetComponent<FireballProjectile>();
        if (script != null)
        {
            // 1. TÍNH CRIT 
            // Lấy chỉ số Crit từ stats (đã có sẵn trong PlayerMagic)
            float critChance = (stats != null) ? stats.CriticalChance : 0f;
            bool isCrit = UnityEngine.Random.value < critChance;

            // 2. TÍNH DAMAGE CUỐI CÙNG
            float finalDamage = damage;
            if (isCrit)
            {
                finalDamage *= 3f; // Nhân đôi nếu Crit
            }

            // 3. TRUYỀN DỮ LIỆU VÀO VIÊN ĐẠN
            script.damage = finalDamage;     
            script.isCritical = isCrit;      

            script.xpManager = xpManager;
            script.speed = speedOverride;
        }
    }
    private void CastTornado()
    {
        if (playerAnimator != null) playerAnimator.SetTrigger("CastSpell");

        // 1. Tính hướng bắn
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = (mousePos - firePoint.position).normalized;

        // 2. Sinh ra Tornado 
        // Để sprite luôn đứng thẳng
        GameObject tornadoObj = Instantiate(tornadoPrefab, firePoint.position, Quaternion.identity);

        TornadoProjectile script = tornadoObj.GetComponent<TornadoProjectile>();

        if (script != null)
        {
            bool isCrit = UnityEngine.Random.value < stats.CriticalChance;
            float finalDamage = stats.TornadoDamage * (isCrit ? 3f : 1f);

            script.damage = finalDamage;
            script.isCritical = isCrit; 

            script.xpManager = xpManager;
            script.flyDirection = direction;
        }
    }
    // Nổ tại vị trí chuột
    private void CastExplosionAtMouse()
    {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0; // Đảm bảo z = 0 cho game 2D

        GameObject expObj = Instantiate(explosionPrefab, mousePos, Quaternion.identity);
        float scale = stats.AreaScale;
        expObj.transform.localScale = new Vector3(scale, scale, scale);
        AreaExplosion script = expObj.GetComponent<AreaExplosion>();
        if (script != null)
        {
            bool isCrit = UnityEngine.Random.value < stats.CriticalChance;
            float finalDamage = stats.ExplosionDamage * (isCrit ? 3f : 1f);

            script.damage = finalDamage;
            script.isCritical = isCrit;
            script.xpManager = xpManager;
        }
    }
}