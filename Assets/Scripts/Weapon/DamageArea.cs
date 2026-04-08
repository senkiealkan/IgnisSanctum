using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Audio;


public class DamageArea : MonoBehaviour
{
    public PlayerStats stats;
    public float damage; // sát thương mỗi đòn chém
    public string targetTag = "Enemy"; // mục tiêu có tag Enemy
    public AudioSource playerAudioSource;
    public AudioClip slash;
    private bool hasPlayedSlash = false;
    private bool hasShaken = false;
    private bool isHitstopActive = false;
    private Animator animator;

    private CinemachineImpulseSource impulseSource;
    [SerializeField] public float dissolveTime = 2.5f;
    [SerializeField] SpriteRenderer spriteRenderer;
    private int dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    public XPManager xpManager;
    [Header("Hit Effects")]
    public float hitstopDuration = 0.2f; // Thời gian dừng khung hình
    public float enemyKnockbackForce = 8f;
    [Header("References")]
    public PlayerHealth playerHealth;

    private void Start()
    {
       
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerAudioSource = GetComponent<AudioSource>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        // [ADD] Tìm stats nếu chưa gán (đề phòng)
        if (stats == null && playerHealth != null)
        {
            stats = playerHealth.GetComponent<PlayerStats>();
        }

        // [CHANGE] Lấy damage ban đầu
        if (stats != null)
        {
            damage = stats.TotalDamage;
            // [ADD] Đăng ký sự kiện
            stats.OnStatsChanged += UpdateDamageStats;
        }
    }
    private void Update()
    {
        if (playerHealth.isDead) {
            animator.SetTrigger("Fall");
        } 
        
    }
    // [ADD] Hủy đăng ký
    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnStatsChanged -= UpdateDamageStats;
        }
    }

    // [ADD] Cập nhật damage
    private void UpdateDamageStats()
    {
        damage = stats.TotalDamage;
        Debug.Log($"Sát thương vùng cập nhật: {damage}");
    }
   
    public void ResetSlashSound()
    {
        hasPlayedSlash = false;
        hasShaken = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            // CHỈ PHÁT ÂM THANH NẾU CHƯA PHÁT trong lần chém này
            if (!hasPlayedSlash && playerAudioSource != null && slash != null)
            {
                playerAudioSource.PlayOneShot(slash);
                hasPlayedSlash = true; // Đặt cờ thành true sau khi phát
            }
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                float critChance = (stats != null) ? stats.CriticalChance : 0f;
                bool isCritical = UnityEngine.Random.value < stats.CriticalChance;
                float finalDamage = damage;

                if (isCritical)
                {
                    finalDamage *= 3f; 
                    Debug.Log("CRITICAL HIT!");
                }
                // 1. Gây sát thương
                enemy.TakeDamage(finalDamage, isCritical);
                xpManager.GainXP(finalDamage);

                // 2. Knockback
                Vector2 sourcePosition = playerHealth.transform.position;
                Vector2 knockbackDirection = (other.transform.position - (Vector3)sourcePosition).normalized;
                enemy.ApplyKnockback(knockbackDirection * enemyKnockbackForce);

                // 3. Camera shake chỉ 1 lần mỗi slash
                if (!hasShaken)
                {
                    impulseSource.GenerateImpulse();
                    hasShaken = true;
                }
                //4.Hitstop effect(pause nhẹ khi chém trúng)
                if (!isHitstopActive)
                {
                    StartCoroutine(HitstopCoroutine(hitstopDuration));
                }
            }
        }
    }
    private System.Collections.IEnumerator HitstopCoroutine(float duration)
    {
        isHitstopActive = true;

        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f; // Dừng thời gian game
        yield return new WaitForSecondsRealtime(duration); // Thời gian tính theo thời gian thực, không phụ thuộc TimeScale
        Time.timeScale = originalTimeScale;

        isHitstopActive = false;
    }
 
}
