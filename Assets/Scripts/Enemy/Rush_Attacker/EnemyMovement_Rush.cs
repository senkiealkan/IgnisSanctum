using UnityEngine;
using UnityEngine.Audio;

public class EnemyMovement_Rush : MonoBehaviour
{
    private Transform playerTarget;
    public float moveSpeed = 3f;
    private bool isMoving = false;
    public SpriteRenderer spriteRenderer;
    public EnemyHealth enemyHealth;
    public float playerKnockbackForce = 10f;
    private Collider2D collider;
    public float damageToPlayer = 10f;
    private Rigidbody2D rb;
    private Animator animator;
    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        enemyHealth = gameObject.GetComponent<EnemyHealth>();
        playerTarget = playerObject.transform;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
    
    }
    void Update()
    {

        if (enemyHealth.isDead || playerTarget == null)
        {
            if (playerTarget == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) playerTarget = playerObject.transform;
            }

            if (collider != null) collider.enabled = false; // Chỉ tắt collider nếu chết
            return;
        }

        if (enemyHealth.knockbackTimer > 0) return;
   
 
        // Tính hướng di chuyển
        Vector3 direction = (playerTarget.position - transform.position).normalized;

        if (enemyHealth != null && enemyHealth.knockbackTimer <= 0)
        {
            isMoving = true;     
            rb.linearVelocity = direction * moveSpeed;
        }
        animator.SetBool("isMoving", isMoving);
        // --- Lật sprite ---
        if (spriteRenderer != null)
        {
            // Lấy scale hiện tại
            Vector3 currentScale = transform.localScale;
            // Nếu player ở bên trái enemy → lật ngược
            if (direction.x < 0.5)
            {
                // Đảm bảo component X là âm (để lật)
                if (currentScale.x > 0)
                {
                    transform.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
                }
            }
            // Nếu player ở bên phải enemy (hướng x > 0) → không lật
            else if (direction.x > 0.5)
            {
                // Đảm bảo component X là dương (để không lật)
                if (currentScale.x < 0)
                {
                    transform.localScale = new Vector3(-currentScale.x, currentScale.y, currentScale.z);
                }
            }
            if (direction.y < 0)
                spriteRenderer.sortingOrder = 1;
            else if (direction.y > 0)
                spriteRenderer.sortingOrder = 3;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
              
                playerHealth.ApplyKnockback(knockbackDirection * playerKnockbackForce);
                playerHealth.TakeDamage(damageToPlayer);
            }
        }
    }

}
