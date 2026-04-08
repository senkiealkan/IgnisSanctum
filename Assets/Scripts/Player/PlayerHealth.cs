using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Combat Settings")]
    [Tooltip("Thời gian bị choáng, không thể di chuyển")]
    public float knockbackDuration = 0.4f; // Giữ cái này ngắn thôi (0.3 - 0.5)

    [Tooltip("Thời gian bất tử sau khi bị đánh")]
    public float invincibilityDuration = 2.0f; // Cái này phải dài hơn knockback (1.5 - 2.0)

    [Tooltip("Tốc độ nhấp nháy khi bất tử")]
    public float flashDuration = 0.1f;

    [Header("Dissolve & Visuals")]
    [SerializeField] public float dissolveTime = 3f;
    [SerializeField] SpriteRenderer spriteRenderer;
    private int dissolveAmount = Shader.PropertyToID("_DissolveAmount");

    [Header("References")]
    public GameOverManager gameOverManager;
    private Animator animator;
    private Rigidbody2D rb;
    public PlayerStats stats;
    public HealthBar healthBar;
    private Collider2D col;
    private Collider2D[] allColliders;
    private AudioSource audioSource;
    public AudioClip oofSound;

    [Header("Status")]
    public float maxHealth;
    public float currentHealth;
    public bool isDead = false;

    // Knockback
    public float knockbackTimer = 0f;

    private bool isInvulnerable = false;

    // Hàm này dùng cho Dash hoặc các skill đặc biệt
    public void SetInvulnerability(bool isInvulnerable)
    {
        this.isInvulnerable = isInvulnerable;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        allColliders = GetComponents<Collider2D>();
        if (healthBar == null) healthBar = GetComponentInChildren<HealthBar>();

        if (stats != null)
        {
            stats.RecalculateStats();
            maxHealth = stats.MaxHealth;
            currentHealth = maxHealth;
        }
    }

    void Start()
    {
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        if (stats != null)
        {
            stats.OnStatsChanged += UpdateHealthStats;
        }
    }

    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnStatsChanged -= UpdateHealthStats;
        }
    }

    private void UpdateHealthStats()
    {
        float newMaxHealth = stats.MaxHealth;
        float healthIncrease = newMaxHealth - maxHealth;
        maxHealth = newMaxHealth;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            if (healthIncrease > 0)
            {
                currentHealth += healthIncrease;
                healthBar.SetHealth(currentHealth);
            }
        }
    }

    void Update()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f) // Lưu ý: Logic cũ của bạn là 0.5, hãy chắc chắn nó khớp với knockbackDuration
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    animator.SetBool("isKnockback", false);
                }
            }
            if (knockbackTimer <= 0)
            {
                col.enabled = true;
            }
        }
        if (isDead)
        {
            col.enabled = false;
            return;
        }
    }

    public void TakeDamage(float damage)
    {
        // 1. Nếu đang bất tử thì bỏ qua sát thương
        if (isInvulnerable)
        {
            return;
        }

        // 2. Trừ máu và cập nhật UI
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! HP left: {currentHealth}");

        if (healthBar != null) healthBar.SetHealth(currentHealth);
        if (audioSource != null && oofSound != null) audioSource.PlayOneShot(oofSound);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilitySequence());
        }
    }

    // [MOVED & UPDATED] Logic Flash cũ đã được thay thế bằng Logic xịn hơn
    private IEnumerator InvulnerabilitySequence()
    {
        isInvulnerable = true; // Bật bất tử ngay lập tức

        float timer = 0f;

        // Vòng lặp nhấp nháy trong suốt thời gian invincibilityDuration
        while (timer < invincibilityDuration)
        {
            // Bật hiệu ứng Hit (trắng)
            if (spriteRenderer != null) spriteRenderer.material.SetInt("_hit", 1);
            yield return new WaitForSeconds(flashDuration);

            // Tắt hiệu ứng Hit (về bình thường)
            if (spriteRenderer != null) spriteRenderer.material.SetInt("_hit", 0);
            yield return new WaitForSeconds(flashDuration);

            timer += (flashDuration * 2);
        }

        // Đảm bảo tắt flash khi kết thúc
        if (spriteRenderer != null) spriteRenderer.material.SetInt("_hit", 0);

        // Tắt bất tử
        isInvulnerable = false;

        // Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (isInvulnerable && knockbackTimer <= 0) return; // Nếu đang bất tử thì có thể chọn không bị đẩy lùi thêm

        if (rb != null)
        {
            // col.enabled = false; // [LƯU Ý] Tắt cái này ở đây có thể làm Player rơi khỏi map nếu không cẩn thận layer
            animator.SetBool("isKnockback", true);
            knockbackTimer = knockbackDuration;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Stop coroutine bất tử nếu đang chạy dở để tránh lỗi hiển thị
        StopAllCoroutines();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;
        animator.SetBool("isDead", true);

        StartCoroutine(DeathSequence());
        if (BossHealthBar.Instance != null)
        {
            BossHealthBar.Instance.Hide();
        }
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(0.5f);
        col.enabled = false;
        if (EssentialsManager.Instance != null)
        {
            EssentialsManager.Instance.ClearRunData();
        }

        yield return StartCoroutine(Vanish());

        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }

        gameObject.SetActive(false);
    }

    private IEnumerator Vanish()
    {
        float elapseTime = 0f;
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();

        while (elapseTime < dissolveTime)
        {
            elapseTime += Time.deltaTime;
            float lerpedDissolve = Mathf.Lerp(0.2f, 1f, (elapseTime / dissolveTime));

            foreach (var sr in allRenderers)
            {
                if (sr != null)
                {
                    sr.material.SetFloat(dissolveAmount, lerpedDissolve);
                }
            }
            yield return null;
        }
    }

    public void Revive()
    {
        StopAllCoroutines();
        isDead = false;
        isInvulnerable = false; // Reset bất tử

        if (rb != null)
        {
            rb.simulated = true;
        }
        currentHealth = stats.MaxHealth;
        knockbackTimer = 0;

        foreach (Collider2D col in allColliders)
        {
            col.enabled = true;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.material.SetFloat(dissolveAmount, 0f);
            spriteRenderer.color = Color.white;
            spriteRenderer.material.SetInt("_hit", 0);
        }

        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in allRenderers)
        {
            if (sr.material.HasProperty(dissolveAmount))
            {
                sr.material.SetFloat(dissolveAmount, 0f);
            }
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        gameObject.SetActive(true);
        if (healthBar != null) healthBar.SetHealth(currentHealth);

        if (animator != null)
        {
            animator.SetBool("isKnockback", false);
            animator.SetBool("isDead", false);
            animator.Play("Player_Idle");
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > stats.MaxHealth) currentHealth = stats.MaxHealth;
        healthBar.SetHealth(currentHealth);
    }
}