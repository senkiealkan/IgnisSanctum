using UnityEngine;
using UnityEngine.Audio;


public class ChasedProjectileAttacker : MonoBehaviour
{
   

    public float projectileSpeed = 8f;
    public float lifetime = 3f; // Thời gian tồn tại tối đa
    public float explosionDuration = 0.5f; // Thời gian cho animation Expode
    private Transform playerTarget;
    private Animator animator;
    private bool isExploding = false;
    private Collider2D projectileCollider;
    private Rigidbody2D rb;
    private Vector2 targetDirection;
    private AudioSource audioSource;
    public AudioClip explodeSound;
    private static float lastExplosionTime = 0f;
    private const float explosionSoundCooldown = 0.1f;
    [Header("Projectile Stats")]
    public int damageToPlayer = 15; 
    public float knockbackForce = 5f;
    [Header("Homing Settings")]
    public float rotationSpeed = 300f; // Tốc độ xoay (độ/giây)
    private Vector2 currentDirection;
    // Biến này được gọi bởi EnemyRangedCombat khi khởi tạo
    public void Initialize(Transform target)
    {
        playerTarget = target;
        // Tính toán hướng bay ngay lúc khởi tạo
        if (playerTarget != null)
        {

            currentDirection = new Vector2(transform.position.x, transform.position.y);
            targetDirection = (playerTarget.position - transform.position).normalized;
            //currentDirection = targetDirection;
            // a. Lấy góc hiện tại của projectile (từ hướng hiện tại)
            // Lưu ý: Angle là góc tính từ trục X dương, từ -180 đến 180 độ
            float currentAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;

            // b. Lấy góc mục tiêu (từ hướng mục tiêu)
            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            // c. Xoay góc hiện tại về góc mục tiêu một cách mượt mà
            // Mathf.MoveTowardsAngle xử lý tự động việc đi qua điểm -180/180 độ
            float newAngle = Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                rotationSpeed * Time.fixedDeltaTime // rotationSpeed là tốc độ xoay tối đa (độ/giây)
            );

            // d. Chuyển góc xoay mới (newAngle) trở lại thành Vector hướng mới
            // newAngle * Mathf.Deg2Rad chuyển độ sang radian
            currentDirection = new Vector2(
                Mathf.Cos(newAngle * Mathf.Deg2Rad),
                Mathf.Sin(newAngle * Mathf.Deg2Rad)
            );
            // Xoay sprite cho phù hợp với hướng bay (tùy chọn)
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        projectileCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        // Bắt đầu đếm thời gian tự hủy
        Invoke("StartExplosion", lifetime);
    }

    void FixedUpdate()
    {
        if (isExploding) return;

        // --- DI CHUYỂN THEO HƯỚNG ĐÃ TÍNH TOÁN BAN ĐẦU ---
        if (playerTarget != null)
        {
            // 1. TÍNH TOÁN HƯỚNG MỤC TIÊU MỚI
            // Hướng tới Player ở vị trí hiện tại
            Vector2 directionToTarget = (playerTarget.position - transform.position).normalized;

            // --- 2. XỬ LÝ XOAY MỀM MẠI BẰNG GÓC (Angle-based Rotation) ---

            // a. Lấy góc hiện tại của projectile (từ hướng hiện tại)
            // Lưu ý: Angle là góc tính từ trục X dương, từ -180 đến 180 độ
            float currentAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;

            // b. Lấy góc mục tiêu (từ hướng mục tiêu)
            float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

            // c. Xoay góc hiện tại về góc mục tiêu một cách mượt mà
            // Mathf.MoveTowardsAngle xử lý tự động việc đi qua điểm -180/180 độ
            float newAngle = Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                rotationSpeed * Time.fixedDeltaTime // rotationSpeed là tốc độ xoay tối đa (độ/giây)
            );

            // d. Chuyển góc xoay mới (newAngle) trở lại thành Vector hướng mới
            // newAngle * Mathf.Deg2Rad chuyển độ sang radian
            currentDirection = new Vector2(
                Mathf.Cos(newAngle * Mathf.Deg2Rad),
                Mathf.Sin(newAngle * Mathf.Deg2Rad)
            );
            // 3. XOAY SPRITE THEO HƯỚNG HIỆN TẠI
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // 4. DI CHUYỂN
            if (rb != null)
            {
                // Di chuyển theo hướng hiện tại đã được làm mượt
                rb.linearVelocity = currentDirection * projectileSpeed;
            }
        }
        // Đặt biến animation "isMoving" thành true (nếu có)
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Pojectile va chạm với player");
        // Va chạm với Player (cần đảm bảo Player có Tag "Player")
        if (other.CompareTag("Player") && !isExploding)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 1. Gây Sát Thương
                playerHealth.TakeDamage(damageToPlayer);
                

                // 2. Tính toán hướng Knockback
                // Knockback ra khỏi vị trí Projectile
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                playerHealth.ApplyKnockback(knockbackDirection * knockbackForce);

            }

            // Dù có gây sát thương hay không, projectile cũng phải nổ khi đụng Player
            StartExplosion();
        }
        else if (other.CompareTag("Wall"))
        {
            StartExplosion();
        }
    }


    // --- XỬ LÝ NỔ VÀ TỰ HỦY ---
    public void StartExplosion()
    {
        if (isExploding) return;

        isExploding = true;
        // Ngừng di chuyển
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        // Tắt Collider để không va chạm nữa
        if (projectileCollider != null)
        {
            projectileCollider.enabled = false;
        }

        // Kích hoạt animation Expode
        if (animator != null)
        {
            if (Time.time - lastExplosionTime >= explosionSoundCooldown)
            {
                if (explodeSound != null)
                {
                    // Random nhẹ Pitch để âm thanh đỡ bị đều
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(explodeSound);
                }

                // Cập nhật thời gian nổ lần cuối
                lastExplosionTime = Time.time;
            }
            animator.SetBool("isExploding", true);
            animator.SetBool("isMoving", false);
        }

        // Hủy Projectile sau khi animation Expode kết thúc
        Destroy(gameObject, explosionDuration);
    }
}