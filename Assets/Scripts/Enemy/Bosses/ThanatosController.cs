using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyHealth))]
public class ThanatosController : MonoBehaviour
{
   

    public enum ThanatosState { Idle, Chase, MeleeAttack, SummonMinions, SummonThunder, HorizontalSlash, Dead }
    [Header("State Info")]
    public ThanatosState currentState;
    private ThanatosState lastState;

    [Header("Movement Stats")]
    public float moveSpeed = 3f;
    public float dashSpeed = 12f; // Tốc độ lướt để vào thế chém
    private SpriteRenderer spriteRenderer;

    [Header("Melee Settings (Sweet Spot)")]
    public float minAttackRange = 4f; // < 4 là quá gần
    public float maxAttackRange = 7f; // > 7 là quá xa
    // Khoảng cách lý tưởng Boss sẽ cố lướt tới (trung bình 4-7)
    public float idealDistance = 5.5f;

    [Header("Hitbox Settings")]
    [SerializeField] GameObject hitbox;
    private Collider2D meleeHitboxCol;
    private EnemyHitBox meleeHitbox;

    [Header("Summon Minion Settings")]
    public GameObject minionPrefab;
    public int maxMinions = 3;
    public float summonRadius = 4f;
    public GameObject spawnEffect;
    [Tooltip("Layer của Tường và Chướng ngại vật")]
    public LayerMask obstacleLayer;
    [Tooltip("Bán kính của con đệ tử (để check va chạm)")]
    public float minionColliderRadius = 0.5f;
    private List<GameObject> activeMinions = new List<GameObject>();


    [Header("Thunder Settings")]
    public GameObject thunderPrefab; // Prefab sấm sét
    public float thunderCastTime = 0.5f; // Thời gian niệm chú

    [Header("Horizontal Slash Settings")]
    public GameObject horizontalBladePrefab;
    public int bladeCount = 5; // Số lượng dao bay ra
    public float bladeSpeed = 6f;
    public float bladeGap = 1.5f; // Khoảng cách giữa các dao để player né

    [Header("Enrage & Brain")]
    [Range(0, 1)] public float enrageThreshold = 0.4f; // 40% máu là nổi điên

    // References
    private Transform playerTarget;
    private EnemyHealth enemyHealth;
    private Animator animator;
    private Rigidbody2D rb;
    private Coroutine currentActionCoroutine;
    private AudioSource audioSource;
    public AudioClip attackingSound;

    void Start()
    {
        meleeHitboxCol = hitbox.GetComponent<Collider2D>();
        meleeHitbox = hitbox.GetComponent<EnemyHitBox>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (meleeHitboxCol != null) meleeHitboxCol.enabled = false;
        if (playerObject != null) playerTarget = playerObject.transform;

        enemyHealth = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        enemyHealth.OnDeath += OnBossDie;
        ChangeState(ThanatosState.Idle);
    }

    private void OnDestroy()
    {
        if (enemyHealth != null) enemyHealth.OnDeath -= OnBossDie;
    }

    void Update()
    {
        if (currentState == ThanatosState.Dead) return;

        // Chỉ quay mặt khi đang Idle, Chase hoặc chuẩn bị skill
        // Khi đang tấn công (Melee/Slash) thì khóa hướng nhìn để đòn đánh có lực
        if (currentState == ThanatosState.Idle || currentState == ThanatosState.Chase)
        {
            if (playerTarget != null) FlipSprite(playerTarget.position - transform.position);
        }
    }

    // --- STATE MACHINE ---
    void ChangeState(ThanatosState newState)
    {
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);

        // Reset Animation Parameters
        animator.SetBool("isMoving", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isSummoningMinions", false);
        animator.SetBool("isSummoningThunder", false);

        rb.linearVelocity = Vector2.zero;
        lastState = currentState;
        currentState = newState;

        switch (newState)
        {
            case ThanatosState.Idle:
                currentActionCoroutine = StartCoroutine(IdleRoutine());
                break;
            case ThanatosState.Chase:
                currentActionCoroutine = StartCoroutine(ChaseRoutine());
                break;
            case ThanatosState.MeleeAttack:
                currentActionCoroutine = StartCoroutine(MeleeAttackRoutine());
                break;
            case ThanatosState.SummonThunder:
                currentActionCoroutine = StartCoroutine(SummonThunderRoutine());
                break;
            case ThanatosState.HorizontalSlash:
                currentActionCoroutine = StartCoroutine(HorizontalSlashRoutine());
                break;
            case ThanatosState.SummonMinions:
                currentActionCoroutine = StartCoroutine(SummonMinionRoutine());
                break;
            case ThanatosState.Dead:
                HandleDeath();
                break;
        }
    }

    // --- BRAIN (AI) ---
    private IEnumerator IdleRoutine()
    {
        float waitTime = IsEnraged() ? 0.2f : 0.8f;
        yield return new WaitForSeconds(waitTime);

        if (playerTarget == null) yield break;

        float distance = Vector2.Distance(transform.position, playerTarget.position);
        ThanatosState nextState = ThanatosState.Chase;

        // Logic chọn skill
        // 1. Nếu Player ở quá xa (> 10 đơn vị) -> Ưu tiên triệu hồi Sấm hoặc Dao bay
        if (distance > 10f)
        {
            float rand = Random.value;
            if (rand < 0.4f) nextState = ThanatosState.SummonThunder;
            else if (rand < 0.7f) nextState = ThanatosState.HorizontalSlash;
            else nextState = ThanatosState.Chase; // Vẫn có tỉ lệ đuổi theo
        }
        // 2. Nếu ở khoảng cách trung bình -> Có thể lao vào chém (Melee)
        else
        {
            // Random trọng số
            List<ThanatosState> pot = new List<ThanatosState>();
            pot.Add(ThanatosState.Chase);
            pot.Add(ThanatosState.MeleeAttack); // Lao vào chém
            pot.Add(ThanatosState.SummonThunder); // Đánh lén

            // Check Minion
            CleanUpMinions();
            if (activeMinions.Count < maxMinions) pot.Add(ThanatosState.SummonMinions);

            if (IsEnraged())
            {
                pot.Add(ThanatosState.HorizontalSlash); // Enrage mới spam dao nhiều
                pot.Add(ThanatosState.MeleeAttack);
            }

            nextState = pot[Random.Range(0, pot.Count)];
        }

        ChangeState(nextState);
    }

    // --- ACTIONS ---

    // 1. DI CHUYỂN BÌNH THƯỜNG
    private IEnumerator ChaseRoutine()
    {
        animator.SetBool("isMoving", true);
        float chaseTime = 2.5f;
        float timer = 0f;
        float stopDistance = 4f;
        while (timer < chaseTime && playerTarget != null)
        {
            float dist = Vector2.Distance(transform.position, playerTarget.position);

            // Nếu đã lọt vào tầm đánh lý tưởng (4-7) thì chuyển sang đánh luôn
            if (dist >= minAttackRange && dist <= maxAttackRange)
            {
                ChangeState(ThanatosState.MeleeAttack);
                yield break;
            }

            // Di chuyển tới player
            if (dist > stopDistance)
            {
                // Nếu còn xa -> Tiếp tục đuổi
                Vector2 dir = (playerTarget.position - transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
                animator.SetBool("isMoving", true);
            }
            else
            {
                // Nếu đã quá gần (< 1.5f) -> Dừng lại để giữ khoảng cách
                // Boss sẽ đứng nhìn Player một chút thay vì chen lấn
                //rb.linearVelocity = Vector2.zero;
                //animator.SetBool("isMoving", false); // Tắt anim chạy cho tự nhiên
                ChangeState(ThanatosState.MeleeAttack);
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
        ChangeState(ThanatosState.Idle);
    }

    // 2. CẬN CHIẾN (DASH -> SLASH)
    private IEnumerator MeleeAttackRoutine()
    {
        if (playerTarget == null) { ChangeState(ThanatosState.Idle); yield break; }

        animator.SetBool("isMoving", true); // Dùng anim chạy để lướt tới

        // A. Giai đoạn tiếp cận (Positioning)
        // Tìm điểm "Sweet Spot": Là điểm nằm trên đường thẳng nối Boss-Player, cách Player 1 khoảng idealDistance
        Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
        // Vector từ Player ngược lại Boss nhân với khoảng cách lý tưởng
        Vector2 sweetSpot = (Vector2)playerTarget.position - (directionToPlayer * idealDistance);

        float dashTimer = 0f;
        float maxDashTime = 1.0f; // Chỉ cho phép lướt tối đa 1s để tránh kẹt

        // Lướt nhanh tới vị trí lý tưởng
        while (Vector2.Distance(transform.position, sweetSpot) > 0.5f && dashTimer < maxDashTime)
        {
            // Cập nhật lại sweetSpot liên tục phòng trường hợp player chạy mất
            directionToPlayer = (playerTarget.position - transform.position).normalized;
            sweetSpot = (Vector2)playerTarget.position - (directionToPlayer * idealDistance);

            Vector2 moveDir = (sweetSpot - (Vector2)transform.position).normalized;
            rb.linearVelocity = moveDir * dashSpeed; // Tốc độ cao

            dashTimer += Time.deltaTime;
            yield return null;
        }

        // B. Giai đoạn tấn công
        rb.linearVelocity = Vector2.zero; // Phanh gấp
        //FlipSprite(playerTarget.position - transform.position); // Quay mặt lần cuối cho chuẩn

        animator.SetBool("isMoving", false);
        animator.SetBool("isAttacking", true); // Trigger animation chém

        // Chờ animation kết thúc (giả sử anim dài 1s)
        yield return new WaitForSeconds(0.4f);

        animator.SetBool("isAttacking", false);
        ChangeState(ThanatosState.Idle);
    }

    // 3. TRIỆU HỒI SẤM (TARGET LOCK)
    private IEnumerator SummonThunderRoutine()
    {
        animator.SetBool("isSummoningThunder", true);
        rb.linearVelocity = Vector2.zero;

        // Boss đứng niệm chú
        yield return new WaitForSeconds(thunderCastTime);

        if (playerTarget != null && thunderPrefab != null)
        {
            // Tạo sấm sét ngay tại vị trí Player hiện tại (cộng thêm offset Y trên cao)
            Vector3 spawnPos = playerTarget.position;
            spawnPos += Vector3.up * 6f; 

            Instantiate(thunderPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.5f); // Hồi phục sau khi cast
        animator.SetBool("isSummoningThunder", false);
        ChangeState(ThanatosState.Idle);
    }

    // 4. HORIZONTAL SLASH (DAO BAY NGANG)
    private IEnumerator HorizontalSlashRoutine()
    {
        animator.SetBool("isAttacking", true);
        rb.linearVelocity = Vector2.zero;

        // --- 1. XÁC ĐỊNH HƯỚNG ---
        // Sprite gốc quay về bên trái, nên logic scale bị ngược:
        // Scale.x DƯƠNG (+) -> Đang quay TRÁI -> Hướng bắn là (-1, 0)
        // Scale.x ÂM (-)    -> Đang quay PHẢI -> Hướng bắn là (+1, 0)

        // Nếu Scale > 0 (quay trái) thì directionX = -1. 
        // Nếu Scale < 0 (quay phải) thì directionX = 1.
        float directionX = (transform.localScale.x > 0) ? -1f : 1f;
        Vector2 flyDirection = new Vector2(directionX, 0);

        // --- 2. XÁC ĐỊNH VỊ TRÍ SPAWN (NGOÀI MÀN HÌNH) ---
        float cameraX = Camera.main.transform.position.x;
        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Camera.main.aspect;
        float screenHalfWidth = screenWidth / 2f;

        // Nếu bắn sang TRÁI (Dir = -1) -> Spawn ở mép PHẢI (CameraX + HalfWidth)
        // Nếu bắn sang PHẢI (Dir = 1) -> Spawn ở mép TRÁI (CameraX - HalfWidth)
        float spawnX = (directionX < 0)
            ? (cameraX + screenHalfWidth + 2f)  // Mép Phải
            : (cameraX - screenHalfWidth - 2f); // Mép Trái

        // Tính toán khoảng cách rải dao theo chiều dọc
        float cameraY = Camera.main.transform.position.y;
        float cameraHalfHeight = Camera.main.orthographicSize;
        float startY = cameraY - cameraHalfHeight + 1f; // Bắt đầu từ đáy màn hình
        float calculatedGap = (cameraHalfHeight * 2f - 2f) / bladeCount;

        for (int i = 0; i < bladeCount; i++)
        {
            // Tạo lỗ hổng ngẫu nhiên (30%)
            if (Random.value > 0.7f)
            {
                startY += calculatedGap;
                continue;
            }

            Vector3 spawnPos = new Vector3(spawnX, startY, 0);

            if (horizontalBladePrefab != null)
            {
                GameObject blade = Instantiate(horizontalBladePrefab, spawnPos, Quaternion.identity);

                // --- ĐOẠN QUAN TRỌNG NHẤT ---
                StraightProjectile projScript = blade.GetComponent<StraightProjectile>();
                if (projScript != null)
                {
                    projScript.Initialize(flyDirection);
                }

          
                float angle = (directionX > 0) ? 0 : 180;
                blade.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            startY += calculatedGap;
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1.0f);
        animator.SetBool("isAttacking", false);
        ChangeState(ThanatosState.Idle);
    }

    // 5. TRIỆU HỒI MINION
    private IEnumerator SummonMinionRoutine()
    {
        animator.SetBool("isSummoningMinions", true);
        rb.linearVelocity = Vector2.zero;
        float spawnDelay = IsEnraged() ? 0.5f : 1.0f;
        yield return new WaitForSeconds(1f); // Thời gian gồng

        CleanUpMinions();
        int amountToSpawn = maxMinions - activeMinions.Count;
        amountToSpawn = Mathf.Min(amountToSpawn, 2); // Chỉ gọi tối đa 2 con 1 lần

        for (int i = 0; i < amountToSpawn; i++)
        {
            // [FIX] Tìm vị trí hợp lệ thay vì random bừa
            Vector2? spawnPos = GetValidSummonPosition();

            if (spawnPos.HasValue)
            {
                // Spawn hiệu ứng khói/bụi nếu có (Optional)
                Instantiate(spawnEffect, spawnPos.Value, Quaternion.identity);

                GameObject minion = Instantiate(minionPrefab, spawnPos.Value, Quaternion.identity);
                activeMinions.Add(minion);
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        animator.SetBool("isSummoningMinions", false);
        ChangeState(ThanatosState.Idle);
    }
    private Vector2? GetValidSummonPosition()
    {
        int maxAttempts = 10; // Số lần thử tìm chỗ trống

        for (int i = 0; i < maxAttempts; i++)
        {
            // 1. Random vị trí quanh Boss
            Vector2 randomOffset = Random.insideUnitCircle * summonRadius;
            Vector2 candidatePos = (Vector2)transform.position + randomOffset;

            // 2. Kiểm tra xem vị trí này có dính tường hay cột không
            // Dùng OverlapCircle để quét thử 1 vòng tròn to bằng con Minion
            Collider2D hit = Physics2D.OverlapCircle(candidatePos, minionColliderRadius, obstacleLayer);

            // Nếu hit == null nghĩa là không va vào Tường/Obstacle -> Vị trí ngon
            if (hit == null)
            {
                return candidatePos;
            }
        }

        // Nếu thử 10 lần mà vẫn kẹt (do boss đứng quá sát góc tường) -> Trả về null (Không spawn con này)
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
    }

    // --- HELPERS ---
    private bool IsEnraged() => (enemyHealth.currentHealth / enemyHealth.maxHealth) <= enrageThreshold;

    private void CleanUpMinions() => activeMinions.RemoveAll(item => item == null);

    private void FlipSprite(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) < 0.5f) return;

        // Logic cũ giữ nguyên
        if (dir.x > 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (dir.x < 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);

        if (dir.y < 0)
            spriteRenderer.sortingOrder = 1;
        else if (dir.y > 0)
            spriteRenderer.sortingOrder = 3;
    }

    private void HandleDeath()
    {
        currentState = ThanatosState.Dead;
        rb.linearVelocity = Vector2.zero;
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);

        // Clean minions
        foreach (var m in activeMinions) { if (m) Destroy(m); }
        activeMinions.Clear();

        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    private void OnBossDie() => HandleDeath();
    public void ActivateHitBox()
    {
        meleeHitboxCol.enabled = true;
    }

    public void DeactivateHitBox()
    {
        meleeHitboxCol.enabled = false;
        meleeHitbox.ResetHit();
    }

    public void PlayAttackingSound()
    {
        audioSource.PlayOneShot(attackingSound);
    }
}