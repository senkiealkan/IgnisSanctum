using UnityEngine;
using UnityEngine.Audio;

public class FireballProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;
    private float explosionRadius = 2f;
    private Animator animator;
    public bool isCritical = false;
    [HideInInspector] public float damage; 
    [HideInInspector] public XPManager xpManager; 

    private AudioSource audioSource;
    public AudioClip explodeSound;

    private Rigidbody2D rb;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);

        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
           rb.linearVelocity = Vector2.zero;
           Explode();
        }
        else if (other.CompareTag("Wall"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        // 1. Tạo hiệu ứng nổ
        animator.SetBool("isExploding", true);
        audioSource.PlayOneShot(explodeSound);
        // 2. Gây sát thương diện rộng 
        // Ở đây làm đơn giản là quét vùng nổ
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in enemiesHit)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {

                    enemy.TakeDamage(damage, isCritical);
                    if (xpManager != null) xpManager.GainXP(damage);
                }
            }
        }
        Destroy(gameObject,0.3f);
    }

    // Vẽ gizmos để xem phạm vi nổ trong Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}