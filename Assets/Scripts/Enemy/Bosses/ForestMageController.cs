using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(EnemyHealth))]
[RequireComponent(typeof(AudioSource))]
public class ForestMageController : MonoBehaviour
{
    public enum MageState { Walk, Teleport, ThornAttack, CastSummon, CastExplosion, Dead }

    [Header("State Info")]
    public MageState currentState;

    [Header("Movement AI")]
    public float moveSpeed = 2.5f;
    public float keepDistance = 8f;
    public float strafeSpeed = 1.5f;

    [Header("Teleport Settings")]
    public float teleportTriggerDist = 3f;
    public float teleportCooldown = 5f;
    public float minTeleportDistance = 6f;
    private float lastTeleportTime;

    [Header("Thorn Attack")]
    public Vector2 thornRangeX = new Vector2(3f, 8f);
    public Vector2 thornRangeY = new Vector2(1f, 4f);
    public GameObject thornHitBoxObj;
    private Collider2D thornHitBoxCol;
    private EnemyHitBox thornHitBoxScript;

    [Header("Explosion Skill")]
    public GameObject magicBombPrefab;
    public int bombCount = 5;

    [Header("Summon Settings")]
    public GameObject knightPrefab;
    public GameObject meleeMinionPrefab;
    public GameObject rangedMinionPrefab;

    private List<GameObject> activeKnights = new List<GameObject>();
    private List<GameObject> activeMelee = new List<GameObject>();
    private List<GameObject> activeRanged = new List<GameObject>();

    public GameObject spawnEffect;
    public LayerMask obstacleLayer;
    public float summonRadius = 5f;

    [Header("Audio & References")]
    public AudioClip sfxAppear;
    public AudioClip sfxDisappear;
    public AudioClip sfxCastSpell;
    public AudioClip sfxThorn;

    private Transform playerTarget;
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHealth enemyHealth;
    private AudioSource audioSource;
    private Coroutine currentActionCoroutine;

    private float strafeDirection = 1f;
    private float strafeTimer = 0f;

    // Cache biên giới map cục bộ phòng trường hợp WaveManager null
    private Vector2 fallbackMinBounds = new Vector2(-75, -45);
    private Vector2 fallbackMaxBounds = new Vector2(75, 45);
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerTarget = playerObject.transform;

        enemyHealth.OnDeath += OnDeath;

        if (thornHitBoxObj != null)
        {
            thornHitBoxCol = thornHitBoxObj.GetComponent<Collider2D>();
            thornHitBoxScript = thornHitBoxObj.GetComponent<EnemyHitBox>();
            thornHitBoxObj.SetActive(true);
            if (thornHitBoxCol) thornHitBoxCol.enabled = false;
        }

        ChangeState(MageState.Walk);
    }
    void Update()
    {
        if (playerTarget == null || spriteRenderer == null) return;

        float dirY = playerTarget.position.y - transform.position.y;
        
        if (dirY < 0)
        {
            spriteRenderer.sortingOrder = 1; // Player ở dưới -> Enemy vẽ sau 
        }
        else if (dirY > 0)
        {
            spriteRenderer.sortingOrder = 3; // Player ở trên -> Enemy vẽ trước
        }
    }
    private void OnDestroy()
    {
        if (enemyHealth != null) enemyHealth.OnDeath -= OnDeath;
    }

    void ChangeState(MageState newState)
    {
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);

        animator.SetBool("isMoving", false);
        rb.linearVelocity = Vector2.zero;
        currentState = newState;

        switch (newState)
        {
            case MageState.Walk:
                currentActionCoroutine = StartCoroutine(WalkAndDecideRoutine());
                break;
            case MageState.Teleport:
                currentActionCoroutine = StartCoroutine(TeleportRoutine());
                break;
            case MageState.ThornAttack:
                currentActionCoroutine = StartCoroutine(ThornAttackRoutine());
                break;
            case MageState.CastSummon:
                currentActionCoroutine = StartCoroutine(SummonRoutine());
                break;
            case MageState.CastExplosion:
                currentActionCoroutine = StartCoroutine(ExplosionRoutine());
                break;
            case MageState.Dead:
                HandleDeath();
                break;
        }
    }

    IEnumerator WalkAndDecideRoutine()
    {
        animator.SetBool("isMoving", true);
        float decisionTimer = 0f;

        float decisionDelay = 0.5f;

        while (playerTarget != null)
        {
            float dist = Vector2.Distance(transform.position, playerTarget.position);
            Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;

            // --- MOVEMENT ---
            if (dist < keepDistance - 1f) rb.linearVelocity = -dirToPlayer * moveSpeed;
            else if (dist > keepDistance + 2f) rb.linearVelocity = dirToPlayer * moveSpeed;
            else
            {
                Vector2 perpendicular = Vector2.Perpendicular(dirToPlayer) * strafeDirection;
                rb.linearVelocity = perpendicular * strafeSpeed;
                strafeTimer += Time.deltaTime;
                if (strafeTimer > 2f) { strafeDirection *= -1f; strafeTimer = 0f; }
            }
            FlipSprite(playerTarget.position.x - transform.position.x);

            // --- DECISION ---
            if (decisionTimer > decisionDelay)
            {
                decisionTimer = 0f;

                // 1. Ưu tiên Teleport nếu bị áp sát 
                if (dist < teleportTriggerDist && Time.time > lastTeleportTime + teleportCooldown)
                {
                    ChangeState(MageState.Teleport);
                    yield break;
                }

                // 2. Kiểm tra vị trí để dùng Thorn
                if (IsInThornPosition())
                {
                    ChangeState(MageState.ThornAttack);
                    yield break;
                }

                CleanUpMinions();
                int totalMinions = activeKnights.Count + activeMelee.Count + activeRanged.Count;
                float rand = Random.value;

   
                if (totalMinions < 3 && rand < 0.7f)
                {
                    ChangeState(MageState.CastSummon);
                    yield break;
                }
                else
                {
                    if (Random.value < 0.7f) 
                    {
                        ChangeState(MageState.CastExplosion);
                        yield break;
                    }
                    else if (Time.time > lastTeleportTime + teleportCooldown)
                    {
                        ChangeState(MageState.Teleport);
                        yield break;
                    }
                }
            }
            decisionTimer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator TeleportRoutine()
    {
        lastTeleportTime = Time.time;
        animator.SetTrigger("Disappear");
        PlaySound(sfxDisappear);
        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(1.0f);

        
        Vector2 targetPos = GetTeleportPosition();
        transform.position = targetPos;

        animator.SetTrigger("Appear");
        PlaySound(sfxAppear);

        yield return new WaitForSeconds(0.8f);
        GetComponent<Collider2D>().enabled = true;
        ChangeState(MageState.Walk);
    }

    IEnumerator ThornAttackRoutine()
    {
        animator.SetTrigger("Thorn");
        yield return new WaitForSeconds(1.5f);
        ChangeState(MageState.Walk);
    }

    IEnumerator SummonRoutine()
    {
        animator.SetTrigger("CastSpell");
        PlaySound(sfxCastSpell);
        yield return new WaitForSeconds(0.5f); // Thời gian chờ animation múa gậy

        CleanUpMinions();

        if (knightPrefab) SpawnMinion(knightPrefab, activeKnights);

        // Random 50/50: Gọi thêm Hội Chiến Binh (Melee) hoặc Hội Xạ Thủ (Ranged)
        if (Random.value < 0.5f)
        {
            if (meleeMinionPrefab)
            {
                for (int i = 0; i < 3; i++) SpawnMinion(meleeMinionPrefab, activeMelee);
            }
        }
        else
        {
            if (rangedMinionPrefab)
            {
                for (int i = 0; i < 2; i++) SpawnMinion(rangedMinionPrefab, activeRanged);
            }
        }

        // Khuyến mãi thêm quả bom 
        SpawnBombAt(playerTarget.position);

        yield return new WaitForSeconds(1.0f); 
        ChangeState(MageState.Walk);
    }

    IEnumerator ExplosionRoutine()
    {
        animator.SetTrigger("CastSpell");
        PlaySound(sfxCastSpell);
        yield return new WaitForSeconds(0.5f);
        if (playerTarget != null) SpawnBombAt(playerTarget.position);
        for (int i = 0; i < bombCount; i++)
        {
            Vector2? randomPos = GetValidPosition(playerTarget.position, 4f);
            if (randomPos.HasValue) SpawnBombAt(randomPos.Value);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1.0f);
        ChangeState(MageState.Walk);
    }

    // --- HELPER METHODS NÂNG CẤP ---

    // 1. Hàm kẹp vị trí vào trong biên giới WaveManager
    Vector2 ClampPositionToArena(Vector2 targetPos)
    {
        // Lấy biên giới từ WaveManager (Singleton)
        Vector2 minB = fallbackMinBounds;
        Vector2 maxB = fallbackMaxBounds;

        if (WaveManager.Instance != null)
        {
            minB = WaveManager.Instance.minBounds;
            maxB = WaveManager.Instance.maxBounds;
        }

        // Ép toạ độ nằm trong khung, chừa lề 1 đơn vị để không dính sát mép
        float clampedX = Mathf.Clamp(targetPos.x, minB.x + 1f, maxB.x - 1f);
        float clampedY = Mathf.Clamp(targetPos.y, minB.y + 1f, maxB.y - 1f);

        return new Vector2(clampedX, clampedY);
    }

    // 2. Logic Teleport: Tính toán xa -> Kẹp vào map -> Check tường
    Vector2 GetTeleportPosition()
    {
        if (playerTarget == null) return transform.position;

        Vector2 fleeDir = (transform.position - playerTarget.position).normalized;
        float randomAngle = Random.Range(-45f, 45f);
        fleeDir = Quaternion.Euler(0, 0, randomAngle) * fleeDir;

        // Tính vị trí mong muốn (có thể bay ra ngoài map)
        Vector2 rawPos = (Vector2)playerTarget.position + fleeDir * (keepDistance + minTeleportDistance);
        Vector2 clampedPos = ClampPositionToArena(rawPos);

        // Kiểm tra xem vị trí đã kẹp có bị dính tường (Obstacle) không
        if (Physics2D.OverlapCircle(clampedPos, 0.5f, obstacleLayer))
        {
            // Nếu dính tường, thử Random 10 lần trong map
            for (int i = 0; i < 10; i++)
            {
                Vector2 randomPoint = (Vector2)playerTarget.position + Random.insideUnitCircle.normalized * (keepDistance + minTeleportDistance);
                Vector2 validPoint = ClampPositionToArena(randomPoint);

                if (!Physics2D.OverlapCircle(validPoint, 0.5f, obstacleLayer))
                    return validPoint;
            }
            return transform.position; // Bất lực thì đứng yên
        }

        return clampedPos;
    }

    // 3. Logic Summon/Bomb: Random quanh tâm -> Kẹp vào map -> Check tường
    Vector2? GetValidPosition(Vector2 center, float radius)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randRaw = center + Random.insideUnitCircle * radius;
            Vector2 randClamped = ClampPositionToArena(randRaw);

            if (!Physics2D.OverlapCircle(randClamped, 0.5f, obstacleLayer))
            {
                return randClamped;
            }
        }
        return null;
    }

    void SpawnMinion(GameObject prefab, List<GameObject> listTracker)
    {
        Vector2? pos = GetValidPosition(transform.position, summonRadius);
        if (pos.HasValue)
        {
            if (spawnEffect) Instantiate(spawnEffect, pos.Value, Quaternion.identity);
            GameObject minion = Instantiate(prefab, pos.Value, Quaternion.identity);
            listTracker.Add(minion);
        }
    }

    void SpawnBombAt(Vector2 pos)
    {
        if (magicBombPrefab) Instantiate(magicBombPrefab, pos, Quaternion.identity);
    }

    bool IsInThornPosition()
    {
        if (playerTarget == null) return false;
        Vector2 relativePos = transform.position - playerTarget.position;
        float absX = Mathf.Abs(relativePos.x);
        float relY = relativePos.y;
        bool isXOkay = absX >= thornRangeX.x && absX <= thornRangeX.y;
        bool isYOkay = relY >= thornRangeY.x && relY <= thornRangeY.y;
        return isXOkay && isYOkay;
    }

    void CleanUpMinions()
    {
        activeKnights.RemoveAll(item => item == null);
        activeMelee.RemoveAll(item => item == null);
        activeRanged.RemoveAll(item => item == null);
    }

    void FlipSprite(float dirX)
    {
        if (Mathf.Abs(dirX) > 0.1f)
        {
            float scaleX = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(dirX > 0 ? scaleX : -scaleX, transform.localScale.y, 1);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource && clip) audioSource.PlayOneShot(clip);
    }

    void HandleDeath()
    {
        currentState = MageState.Dead;
        rb.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        CleanUpMinions();
        foreach (var m in activeKnights) if (m) Destroy(m);
        foreach (var m in activeMelee) if (m) Destroy(m);
        foreach (var m in activeRanged) if (m) Destroy(m);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    void OnDeath() => HandleDeath();

    public void EnableThornHitbox()
    {
        PlaySound(sfxThorn);
        if (thornHitBoxCol) { thornHitBoxCol.enabled = true; if (thornHitBoxScript) thornHitBoxScript.ResetHit(); }
    }

    public void DisableThornHitbox()
    {
        if (thornHitBoxCol) thornHitBoxCol.enabled = false;
    }
}