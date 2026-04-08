using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    public float projectileSpeed = 10f;
    public float lifetime = 3f;
    public float explosionDuration = 0.5f;

    [Header("Projectile Stats")]
    public int damageToPlayer = 15;
    public float knockbackForce = 5f;

    [Header("Rotation Settings")]
    public float selfRotationSpeed = 720f; // Tốc độ xoay quanh tâm (độ/giây)
    private float currentVisualAngle = 0f;

    [Header("Audio & Visual")]
    public AudioClip explodeSound;
    private AudioSource audioSource;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private bool isExploding = false;

    private Vector2 travelDirection = Vector2.right;

    public void Initialize(Vector2 direction)
    {
        travelDirection = direction.normalized;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        projectileCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        Invoke("StartExplosion", lifetime);
    }

    void Update()
    {
        if (isExploding) return;

        // Tự xoay quanh chính nó theo thời gian
        currentVisualAngle += selfRotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, 0f, currentVisualAngle);
    }

    void FixedUpdate()
    {
        if (isExploding) return;

        if (rb != null)
        {
            // Di chuyển thẳng theo hướng đã định
            rb.linearVelocity = travelDirection * projectileSpeed;
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isExploding)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                playerHealth.ApplyKnockback(knockbackDirection * knockbackForce);
            }
            StartExplosion();
        }
        else if (other.CompareTag("Wall"))
        {
            StartExplosion();
        }
    }

    public void StartExplosion()
    {
        if (isExploding) return;
        isExploding = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (projectileCollider != null) projectileCollider.enabled = false;

        if (animator != null)
        {
            if (explodeSound != null && audioSource != null)
                audioSource.PlayOneShot(explodeSound);

            animator.SetBool("isExploding", true);
         
        }

        Destroy(gameObject, explosionDuration);
    }
}