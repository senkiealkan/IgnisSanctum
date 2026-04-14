using UnityEngine;
using UnityEngine.Audio;

public class EnemyMovement_Attacker : MonoBehaviour
{
    private Transform playerTarget;

    private Rigidbody2D rb;
    public float moveSpeed = 3f;
    public bool isMoving = false;
    public SpriteRenderer spriteRenderer;
    public EnemyHealth enemyHealth;
    public float playerKnockbackForce = 10f;
    private Collider2D collider;
    private Animator animator;
    public bool isAttacking = false;
    public bool isAttackInProgress = false;
    [Header("Attack Range")]
    public float attackRangeX = 4f; // Phạm vi tấn công theo trục X
    public float attackRangeY = 3f; // Phạm vi tấn công theo trục Y
    private bool isWithinRange = false;
    public void SetTarget(Transform target)
    {
        playerTarget = target;
        rb = GetComponent<Rigidbody2D>();
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

        if (enemyHealth.knockbackTimer > 0 || isAttackInProgress ) return;

     
        Vector3 direction = (playerTarget.position - transform.position).normalized;



        // KIỂM TRA PHẠM VI TẤN CÔNG ---
        float distanceX = Mathf.Abs(playerTarget.position.x - transform.position.x);
        float distanceY = Mathf.Abs(playerTarget.position.y - transform.position.y);

        isWithinRange = (distanceX <= attackRangeX && distanceY <= attackRangeY);

        if (isWithinRange)
        {
            // ---TRONG PHẠM VI: DỪNG DI CHUYỂN & CHUYỂN SANG TẤN CÔNG ---
            isMoving = false;
            isAttacking = true; 
            isAttackInProgress = true;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // --- NGOÀI PHẠM VI: TIẾP TỤC DI CHUYỂN ---
            isAttacking = false; 
            isMoving = true;
            if (enemyHealth != null && enemyHealth.knockbackTimer <= 0)
            {        
                rb.linearVelocity = direction * moveSpeed;
            }
        }

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isAttacking", isAttacking);

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
 
}
