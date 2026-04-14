using System.Collections;
using UnityEngine;


public class KnightController : MonoBehaviour
{
    public enum KnightState { Walk, LightAttack, Dashing, HeavyAttack, Dead }

    [Header("State Info")]
    public KnightState currentState;
    private Transform playerTarget;

    [Header("Movement Stats")]
    public float moveSpeed = 3.5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;

    [Header("Light Attack Settings")]
    public float lightAttackRangeX = 5f;
    public float lightAttackRangeY = 0.5f;
    public float lightAttackCooldown = 2f;
    public AudioClip lightAttackSound;
    [Tooltip("Kéo GameObject con chứa Light Hitbox vào đây")]
    public GameObject lightHitBoxObj;
    private Collider2D lightHitBoxCol;     
    private EnemyHitBox lightHitBoxScript;

    [Header("Heavy Attack Settings")]
    public float heavyAttackRangeX = 7f;
    public float heavyAttackRangeY = 1.5f;
    public float heavyAttackCooldown = 5f;
    public AudioClip heavyAttackSound;
    [Tooltip("Kéo GameObject con chứa Heavy Hitbox vào đây")]
    public GameObject heavyHitBoxObj;
    private Collider2D heavyHitBoxCol;     
    private EnemyHitBox heavyHitBoxScript;

    public TrailRenderer dashTrail;

    [Header("Timers (Debug)")]
    [SerializeField] private float currentLightCooldown = 0f;
    [SerializeField] private float currentHeavyCooldown = 0f;

    // References
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerTarget = playerObject.transform;

        enemyHealth.OnDeath += OnDeath;

        // --- SETUP LIGHT HITBOX  ---
        if (lightHitBoxObj != null)
        {
            lightHitBoxCol = lightHitBoxObj.GetComponent<Collider2D>();
            lightHitBoxScript = lightHitBoxObj.GetComponent<EnemyHitBox>();

            // Bật GameObject lên, nhưng tắt Collider đi
            lightHitBoxObj.SetActive(true);
            if (lightHitBoxCol != null) lightHitBoxCol.enabled = false;
        }

        // --- SETUP HEAVY HITBOX ---
        if (heavyHitBoxObj != null)
        {
            heavyHitBoxCol = heavyHitBoxObj.GetComponent<Collider2D>();
            heavyHitBoxScript = heavyHitBoxObj.GetComponent<EnemyHitBox>();

            heavyHitBoxObj.SetActive(true);
            if (heavyHitBoxCol != null) heavyHitBoxCol.enabled = false;
        }

        if (dashTrail) dashTrail.emitting = false;
        currentState = KnightState.Walk;
    }

    private void OnDestroy()
    {
        if (enemyHealth != null) enemyHealth.OnDeath -= OnDeath;
    }

    void Update()
    {
        if (currentState == KnightState.Dead) return;

        // Giảm cooldown
        if (currentLightCooldown > 0) currentLightCooldown -= Time.deltaTime;
        if (currentHeavyCooldown > 0) currentHeavyCooldown -= Time.deltaTime;

        if (playerTarget == null)
        {
            // Thử tìm lại player
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) playerTarget = playerObject.transform;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Chỉ xử lý di chuyển khi đang ở state Walk
        if (currentState == KnightState.Walk)
        {
            HandleWalk();
        }
        if (playerTarget != null && spriteRenderer != null)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;

            if (direction.y < 0)
                spriteRenderer.sortingOrder = 1;
            else if (direction.y > 0)
                spriteRenderer.sortingOrder = 3;
        }
    }

    void HandleWalk()
    {
        float distX = Mathf.Abs(playerTarget.position.x - transform.position.x);
        float distY = Mathf.Abs(playerTarget.position.y - transform.position.y);
        Vector2 direction = (playerTarget.position - transform.position).normalized;

        FlipSprite(direction.x);

        bool inLightRange = distX <= lightAttackRangeX && distY <= lightAttackRangeY;
        bool inHeavyRange = distX <= heavyAttackRangeX && distY <= heavyAttackRangeY;

        if (inHeavyRange && currentHeavyCooldown <= 0)
        {
            StartCoroutine(HeavyAttackSequence());
        }
        else if (inLightRange && currentLightCooldown <= 0)
        {
            StartCoroutine(LightAttackSequence());
        }
        else
        {
            animator.SetBool("isMoving", true);
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    // --- COMBAT COROUTINES ---

    IEnumerator LightAttackSequence()
    {
        SwitchState(KnightState.LightAttack);
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);

        animator.SetTrigger("LightAttack");
        currentLightCooldown = lightAttackCooldown;

        yield return new WaitForSeconds(1.0f); // Chờ animation
        SwitchState(KnightState.Walk);
    }

    IEnumerator HeavyAttackSequence()
    {
        SwitchState(KnightState.Dashing);
        animator.SetBool("isMoving", false);
        currentHeavyCooldown = heavyAttackCooldown;

        // --- DASH ---
        if (dashTrail) dashTrail.emitting = true;

        Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
        // Dash xuyên qua (ra sau lưng)
        Vector2 dashDir = new Vector2(dirToPlayer.x > 0 ? 1 : -1, 0);

        float timer = 0;
        while (timer < dashDuration)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        if (dashTrail) dashTrail.emitting = false;

        // --- ATTACK ---
        SwitchState(KnightState.HeavyAttack);

        // Quay mặt lại
        Vector2 newDir = (playerTarget.position - transform.position).normalized;
        FlipSprite(newDir.x);

        animator.SetTrigger("HeavyAttack");

        yield return new WaitForSeconds(1.5f); // Chờ animation
        SwitchState(KnightState.Walk);
    }

    // --- ANIMATION EVENTS  ---

    public void EnableLightHitbox()
    {
        // 1. Phát âm thanh
        if (audioSource && lightAttackSound)
            audioSource.PlayOneShot(lightAttackSound);

        // 2. Bật Collider và Reset Damage
        if (lightHitBoxCol != null)
        {
            lightHitBoxCol.enabled = true;
            if (lightHitBoxScript != null) lightHitBoxScript.ResetHit();
        }
    }

    public void DisableLightHitbox()
    {
        if (lightHitBoxCol != null) lightHitBoxCol.enabled = false;
    }

    public void EnableHeavyHitbox()
    {
        if (audioSource && heavyAttackSound)
            audioSource.PlayOneShot(heavyAttackSound);

        if (heavyHitBoxCol != null)
        {
            heavyHitBoxCol.enabled = true;
            if (heavyHitBoxScript != null) heavyHitBoxScript.ResetHit();
        }
    }

    public void DisableHeavyHitbox()
    {
        if (heavyHitBoxCol != null) heavyHitBoxCol.enabled = false;
    }

    // --- HELPER FUNCTIONS ---

    void SwitchState(KnightState newState)
    {
        currentState = newState;
    }

    void FlipSprite(float directionX)
    {
        if (Mathf.Abs(directionX) > 0.1f)
        {
            if (directionX > 0 && !isFacingRight)
            {
                isFacingRight = true;
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            else if (directionX < 0 && isFacingRight)
            {
                isFacingRight = false;
                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }

    void OnDeath()
    {
        currentState = KnightState.Dead;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);
        StopAllCoroutines();

        // Tắt hết collider khi chết
        DisableLightHitbox();
        DisableHeavyHitbox();

        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }
}