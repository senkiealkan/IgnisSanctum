using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyHealth))]
public class BossSlimeController : MonoBehaviour
{
    public enum BossState { Idle, Walk, Jump, SpinRush, SleepSummon, Dead }

    [Header("State Info")]
    public BossState currentState;
    private BossState lastState; // Lưu trạng thái trước để tránh lặp

    [Header("Stats")]
    public float moveSpeed = 4f;
    public float jumpSpeed = 8f;
    public float spinRushSpeed = 15f;
    public float damageToPlayer = 25f;
    public float playerKnockbackForce = 10f;

    [Header("Durations")]
    public float walkDuration = 3f;
    public float spinPrepareTime = 0.5f;
    public float spinRushDuration = 1.5f;
    public float jumpPrepareTime = 0.5f; // Thời gian gồng nhảy
    public float jumpDuration = 1.0f;    // Thời gian bay

    [Header("Summon Settings")]
    public GameObject minionPrefab;
    public int maxMinions = 4;
    public float summonRadius = 3f;
    public GameObject spawnEffect;
    private List<GameObject> activeMinions = new List<GameObject>();
    [Tooltip("Layer của Tường và Chướng ngại vật")]
    public LayerMask obstacleLayer;
    [Tooltip("Bán kính của con đệ tử (để check va chạm)")]
    public float minionColliderRadius = 0.5f;


    [Header("Enrage Settings")]
    [Range(0, 1)] public float enrageThreshold = 0.5f; // Dưới 50% máu sẽ nổi điên

    [Header("References")]
    public Transform puddlePoint;
    public GameObject puddlePrefab;

    private Transform playerTarget;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth enemyHealth;
    private Animator animator;
    private Rigidbody2D rb;
    private Coroutine currentActionCoroutine;

    void Start()
    {
        // Cache references
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerTarget = playerObject.transform;

        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Event lắng nghe cái chết
        enemyHealth.OnDeath += OnBossDie;

        // Bắt đầu vòng lặp
        ChangeState(BossState.Idle);
    }

    private void OnDestroy()
    {
        if (enemyHealth != null) enemyHealth.OnDeath -= OnBossDie;
    }

    void Update()
    {
        if (currentState == BossState.Dead) return;
        FlipSprite(playerTarget.position - transform.position);
        // xử lý quay mặt (flip) liên tục nếu không phải đang rush hoặc ngủ
        if (currentState != BossState.SleepSummon && playerTarget != null)
        {
            FlipSprite(playerTarget.position - transform.position);
        }
    }

    // --- STATE MACHINE CORE ---
    void ChangeState(BossState newState)
    {
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);

        // Cleanup state cũ
        animator.SetBool("isMoving", false);
        animator.SetBool("isSpining", false);
        animator.SetBool("isSleeping", false);
        animator.SetBool("isJumping", false);
        rb.linearVelocity = Vector2.zero; // Dừng di chuyển ngay lập tức

        lastState = currentState;
        currentState = newState;

        switch (newState)
        {
            case BossState.Idle:
                currentActionCoroutine = StartCoroutine(IdleRoutine());
                break;
            case BossState.Walk:
                currentActionCoroutine = StartCoroutine(WalkRoutine());
                break;
            case BossState.Jump:
                currentActionCoroutine = StartCoroutine(JumpRoutine());
                break;
            case BossState.SpinRush:
                currentActionCoroutine = StartCoroutine(SpinRushRoutine());
                break;
            case BossState.SleepSummon:
                currentActionCoroutine = StartCoroutine(SleepSummonRoutine());
                break;
            case BossState.Dead:
                HandleDeath();
                break;
        }
    }

    // --- LOGIC CHỌN SKILL (BRAIN) ---
    private IEnumerator IdleRoutine()
    {
        // Thời gian nghỉ giữa các chiêu: Ngắn hơn nếu đang cáu (Enraged)
        bool isEnraged = IsEnraged();
        float waitTime = isEnraged ? 0.5f : 1.0f;
        yield return new WaitForSeconds(waitTime);

        BossState nextState = ChooseNextState(isEnraged);
        ChangeState(nextState);
    }

    private BossState ChooseNextState(bool isEnraged)
    {
        List<BossState> potentialStates = new List<BossState>();

        // 1. Logic cơ bản: Thêm các state vào rổ random
        // Thêm Jump (Ưu tiên cao)
        potentialStates.Add(BossState.Jump);
        if (isEnraged) potentialStates.Add(BossState.Jump); // Thêm 1 phiếu nữa nếu cáu

        // Thêm Spin (Ưu tiên cao)
        potentialStates.Add(BossState.SpinRush);
        if (isEnraged) potentialStates.Add(BossState.SpinRush); // Thêm 1 phiếu nữa nếu cáu

        // Thêm Summon (Chỉ thêm nếu chưa full đệ)
        CleanUpMinions();
        if (activeMinions.Count < maxMinions)
        {
            potentialStates.Add(BossState.SleepSummon);
        }

        // 2. Logic Anti-Walk: Chỉ thêm Walk nếu lượt trước KHÔNG PHẢI là Walk
        if (lastState != BossState.Walk)
        {
            potentialStates.Add(BossState.Walk);
            // Nếu Boss chưa cáu, Walk có thể xuất hiện nhiều hơn chút để dễ thở
            if (!isEnraged) potentialStates.Add(BossState.Walk);
        }

        // 3. Chọn random từ danh sách đã lọc
        return potentialStates[Random.Range(0, potentialStates.Count)];
    }

    // --- ACTION ROUTINES ---

    private IEnumerator WalkRoutine()
    {
        animator.SetBool("isMoving", true);
        float timer = 0f;

        while (timer < walkDuration && playerTarget != null)
        {
            // Kiểm tra Knockback/Stun (nếu có logic stun thì chèn break vào đây)

            // Di chuyển
            Vector2 dir = (playerTarget.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;

            timer += Time.deltaTime;
            yield return null;
        }

        ChangeState(BossState.Idle);
    }

    private IEnumerator JumpRoutine()
    {
        // Giai đoạn 1: Chuẩn bị nhảy
        animator.SetBool("isJumping", true); // Trigger animation nhảy lên
        yield return new WaitForSeconds(jumpPrepareTime);

        // Giai đoạn 2: Bay tới Player (Lấy vị trí lúc bắt đầu nhảy)
        Vector2 targetPos = playerTarget != null ? playerTarget.position : transform.position;
        Vector2 startPos = transform.position;
        float timer = 0f;

        while (timer < jumpDuration)
        {
            // Di chuyển Boss tới vị trí mục tiêu (Lerp hoặc MoveTowards)
            // Ở đây dùng Velocity để giữ physics
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * jumpSpeed;

            timer += Time.deltaTime;
            yield return null;
        }

        // Giai đoạn 3: Đáp đất (Spawn puddle xử lý ở Animation Event hoặc tại đây)
        rb.linearVelocity = Vector2.zero;
        // Nếu Animation của bạn có event gọi SpawnPuddle thì để nó tự gọi
        // Nếu không thì gọi: SpawnPuddle();

        yield return new WaitForSeconds(0.2f); // Delay nhỏ khi đáp đất
        ChangeState(BossState.Idle);
    }

    private IEnumerator SpinRushRoutine()
    {
        animator.SetBool("isSpining", true);
        yield return new WaitForSeconds(spinPrepareTime);
        // Khóa mục tiêu lúc bắt đầu
        Vector2 rushDir = playerTarget != null ? (playerTarget.position - transform.position).normalized : Vector2.right;

        float timer = 0f;
        while (timer < spinRushDuration)
        {
            rb.linearVelocity = rushDir * spinRushSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        ChangeState(BossState.Idle);
    }

    private IEnumerator SleepSummonRoutine()
    {
        animator.SetBool("isSleeping", true);

        // Triệu hồi nhanh hơn nếu đang cáu
        float spawnDelay = IsEnraged() ? 0.5f : 1.0f;

        // Spawn đủ số lượng cho đến khi đạt max
        CleanUpMinions();
        int amountToSpawn = maxMinions - activeMinions.Count;
        // Giới hạn mỗi lần ngủ chỉ gọi tối đa 2-3 con thôi để tránh lag
        amountToSpawn = Mathf.Min(amountToSpawn, 3);

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

        animator.SetBool("isSleeping", false);
        ChangeState(BossState.Idle);
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

// --- HELPER METHODS ---

private bool IsEnraged()
    {
        // Kiểm tra % máu
        return (enemyHealth.currentHealth / enemyHealth.maxHealth) <= enrageThreshold;
    }

    private void CleanUpMinions()
    {
        // Xóa các item null khỏi list (do minion đã chết và bị destroy)
        activeMinions.RemoveAll(item => item == null);
    }

    private void FlipSprite(Vector2 dir)
    {
        if (dir.x > 0.5) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (dir.x < 0.5) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    private void HandleDeath()
    {
        currentState = BossState.Dead;
        rb.linearVelocity = Vector2.zero;
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);

        // Giết hết đệ tử
        foreach (var minion in activeMinions)
        {
            if (minion != null)
            {
                var hp = minion.GetComponent<EnemyHealth>();
                if (hp) hp.TakeDamage(9999);
                else Destroy(minion);
            }
        }
        activeMinions.Clear();

        // Logic animation chết nằm ở script EnemyHealth hoặc animator
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    private void OnBossDie()
    {
        ChangeState(BossState.Dead);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == BossState.Dead) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                ph.ApplyKnockback(dir * playerKnockbackForce);
                ph.TakeDamage(damageToPlayer);
            }
        }
    }

    // Animation Event
    public void SpawnPuddle()
    {
        if (puddlePrefab && puddlePoint)
        {
            Instantiate(puddlePrefab, puddlePoint.position, Quaternion.identity);
        }
    }
}