using UnityEngine;
using System.Collections;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] public float dissolveTime = 2f;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem bloodSplashParticle;
    
    private int dissolveAmount = Shader.PropertyToID("_DissolveAmount");

    public event Action OnDeath;
    public float maxHealth = 40;
    public float currentHealth;
    private Rigidbody2D rb;
    private Collider2D[] allColliders;


    //Biến kiểm soát tốc độ phục hồi sau Knockback
    public float knockbackDuration = 0.4f; // Thời gian tối đa để lực đẩy có hiệu lực
    public float knockbackTimer = 0f;
    public bool isDead = false;
    
    [Header("Animation")]
    private Animator animator;
    public bool hasDeadAnim = true;
    public bool hasHitAnim = true;
    [Header("Loot")]
    public GameObject statGemPrefab;
    public GameObject fireGemPrefab;
    public GameObject hpPotionPrefab;   
    public GameObject manaPotionPrefab;
    [Range(0f, 1f)] public float gemDropChance = 0.5f;   // 50%
    [Range(0f, 1f)] public float potionDropChance = 0.2f; // 20% (Thấp hơn)

    [Header("Boss Settings")]
    public bool isBoss = false; // Tích vào ô này trong Prefab của Boss
    public string bossName = "Dark Lord :D";
    private void Start()
    {
        allColliders = GetComponents<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        if (isBoss && BossHealthBar.Instance != null)
        {
            BossHealthBar.Instance.Initialize(bossName, maxHealth);
        }

    }
    private void Update()
    {
        // Giảm timer Knockback
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;

            // Nếu hết thời gian, ngắt vận tốc ngay lập tức để Enemy không trôi
            if (knockbackTimer <= 0)
            {
                rb.linearVelocity = Vector2.zero;
                if (hasHitAnim) animator.SetBool("isHit", false);
            }
        }

        // **LƯU Ý:** Đảm bảo logic di chuyển của Enemy được kiểm tra:
        // Enemy CHỈ di chuyển (theo đuổi Player) nếu knockbackTimer <= 0. 
        // Nếu không, logic di chuyển của Enemy sẽ ghi đè vận tốc Knockback.
    }
    public void TakeDamage(float damage, bool isCritical = false)
    {
        if (bloodSplashParticle != null)
        {
            // Tạo bản sao của Particle System Prefab
            ParticleSystem effectInstance = Instantiate(bloodSplashParticle, transform.position, Quaternion.identity);
            // Hủy bỏ GameObject sau khi Particle System kết thúc
            Destroy(effectInstance.gameObject, effectInstance.main.duration);
        }
        //Kích hoạt hiệu ứng flash
        
        IEnumerator TakeDamage_Cor()
        {
            
            spriteRenderer.material.SetInt("_hit", 1);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.material.SetInt("_hit", 0);

        }
        StartCoroutine(TakeDamage_Cor());
       
        currentHealth -= damage;
        if (DamagePopupPool.Instance != null)
        {
            // Vị trí spawn
            Vector3 spawnPos = transform.position + Vector3.up * 1.5f;

            // Gọi hàm Get từ Pool
            DamagePopupPool.Instance.Get(spawnPos, damage, isCritical);
        }
        if (isBoss && BossHealthBar.Instance != null)
        {
            BossHealthBar.Instance.UpdateHealth(currentHealth);
        }
        Debug.Log($"{gameObject.name} took {damage} damage! HP left: {currentHealth}");

        //if (animator != null)
        //    animator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }
    public void ApplyKnockback(Vector2 force)
    {
        if (rb != null)
        {
        
            if (hasHitAnim && currentHealth > 0) animator.SetBool("isHit", true);
            knockbackTimer = knockbackDuration;
            // Dừng vận tốc hiện tại để Knockback rõ ràng hơn
            rb.linearVelocity = Vector2.zero;
            // Áp dụng lực
            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }
    public IEnumerator Die()
    {

        isDead = true;
        rb.linearVelocity = Vector2.zero;
        foreach (Collider2D col in allColliders)
        {
            col.enabled = false; 
        }
        // Gọi sự kiện chết
        OnDeath?.Invoke();
        OnDeath = null;

        if (animator != null && hasDeadAnim)
        {
            animator.SetBool("isDead", true);
            yield return new WaitForSeconds(1f);
        }

     
        if (isBoss && BossHealthBar.Instance != null) BossHealthBar.Instance.Hide();
        StartCoroutine(Vanish());
        DropLoot();
        Destroy(gameObject, 2f);
    }
    private void DropLoot()
    {
        // 1. Check rơi Gem (Độc lập)
        if (UnityEngine.Random.value <= gemDropChance)
        {
            GameObject gem = (UnityEngine.Random.value > 0.5f) ? statGemPrefab : fireGemPrefab;
            Instantiate(gem, transform.position, Quaternion.identity);
        }

        // 2. Check rơi Potion 
        if (UnityEngine.Random.value <= potionDropChance)
        {
            GameObject potion = (UnityEngine.Random.value > 0.5f) ? hpPotionPrefab : manaPotionPrefab;
            // Random lệch vị trí một chút để không bị chồng lên Gem
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), 0);
            Instantiate(potion, transform.position + offset, Quaternion.identity);
        }
    }
    private IEnumerator Vanish()
    {
        float elapseTime = 0f;
        while (elapseTime < dissolveTime)
        {
            elapseTime += Time.deltaTime;
            float lerpedDissolve = Mathf.Lerp(0.2f, 1f, (elapseTime / dissolveTime));
            spriteRenderer.material.SetFloat(dissolveAmount, lerpedDissolve);
            yield return null;
        }
    }
}
