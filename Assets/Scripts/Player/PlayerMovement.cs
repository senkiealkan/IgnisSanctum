using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
    public PlayerCombat playerCombat;
    private bool isMoving;
    public bool IsMoving => isMoving;
    public PlayerStats stats;
    public float moveSpeed;
    public InputAction playerControls;
    private Rigidbody2D rb;
    private Animator animator;
    public Animator weaponAnimator;
    public SpriteRenderer spriteRenderer;
    private Vector2 moveDirection = Vector2.zero;
    private PlayerHealth playerHealth;
    // Biến cho Dash
    [Header("Dash")]
    public float dashSpeed = 50f;
    public float dashTime = 0.2f; // Thời gian Dash
    public float dashCooldown = 0.25f; // Thời gian hồi chiêu Dash
    private bool isDashing = false;
    public bool IsDashing => isDashing;
    private float dashCooldownTimer;
    public GameObject dashTrail; // Kéo thả Trail Renderer child vào đây
    // [CHANGE] Biến mới cho Multi-Dash
    private int currentDashCharges;
    private bool canDash = true;
    // Input cho Dash
    public InputAction dashAction; // Thêm một InputAction mới cho phím Dash (ví dụ: Space)

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip footstepSound;
    public AudioClip dashSound;
    public float footstepDelay = 0.4f; // Thời gian giữa các tiếng bước chân
    private float footstepTimer;

    private void OnEnable()
    {
        playerControls.Enable();
        // Bật Input Dash
        // 1. Kích hoạt Input Action
        dashAction.Enable();
        // 2. Đăng ký hàm OnDash vào sự kiện performed (khi phím được nhấn)
        dashAction.performed += OnDash;
    }
    private void OnDisable()
    {
        playerControls.Disable();
        dashAction.performed -= OnDash;
        dashAction.Disable();
    }
    void Start()
    {
        stats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();
        playerCombat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        footstepTimer = footstepDelay;
        moveSpeed = stats.TotalMoveSpeed;
        dashCooldownTimer = 0f; // Bắt đầu Dash sẵn sàng
        currentDashCharges = stats.MaxDashCount;
        // Đảm bảo DashTrail bắt đầu là Disabled nếu có
        if (dashTrail != null) dashTrail.SetActive(false);
        if (stats != null)
        {
            stats.OnStatsChanged += UpdateMovementStats;
        }
    }
    private void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnStatsChanged -= UpdateMovementStats;
        }
    }

    private void UpdateMovementStats()
    {
        // Chỉ cập nhật tốc độ cơ bản nếu KHÔNG đang Dash
        if (!isDashing)
        {
            moveSpeed = stats.TotalMoveSpeed;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (playerHealth.isDead) return;
        if (playerHealth.knockbackTimer > 0.5)
        {
            return;
        }
  
        // Logic hồi Dash Charge
        // Nếu chưa đầy bình
        if (currentDashCharges < stats.MaxDashCount)
        {
            dashCooldownTimer -= Time.deltaTime;
            
            // Khi hết thời gian chờ -> Hồi ĐẦY luôn
            if (dashCooldownTimer <= 0)
            {
                currentDashCharges = stats.MaxDashCount;
                dashCooldownTimer = 0; // Đưa về 0 cho gọn
                Debug.Log($"Dash Fully Recharged! Current: {currentDashCharges}");
            }
        }
        // --- CHỈ ĐỌC INPUT VÀ APPLY KHI KHÔNG DASH ---
        if (!isDashing) 
        {
            moveDirection = playerControls.ReadValue<Vector2>();
        }
        // --- Logic cho Animator ---
        isMoving = moveDirection.sqrMagnitude > 0.01f;
        // **LOGIC NGẮT ANIMATION ATTACK**
        if (isMoving && playerCombat != null && playerCombat.IsAttacking)
        {
            // Nếu đang di chuyển VÀ đang tấn công
            // Ngắt animation tấn công của Player.
            if (animator != null)
            {
                animator.SetBool("isAttacking", false);
            }
        }
        if (animator != null)
        {
            animator.SetBool("isRunning", isMoving);
        }
        //Logic điều khiển Animator của Weapon
        if (weaponAnimator != null)
        {
            weaponAnimator.SetBool("isPlayerRunning", isMoving);
        }
        // Chỉ cho phép flip nếu KHÔNG đang attack
        if (playerCombat == null || !playerCombat.IsAttacking)
        {
            Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            Vector2 lookDirection = mouseWorldPosition - transform.position;

            if (lookDirection.x < 0)
                transform.localScale = new Vector3(-0.5f, 0.5f, 0.5f);
            else
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }


        // Lấy layer index idle
        float playerIdleTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (weaponAnimator != null)
        {
            AnimatorStateInfo weaponState = weaponAnimator.GetCurrentAnimatorStateInfo(0);
            // chỉ sync nếu đang trong trạng thái idle
            if (weaponState.IsName("Weapon_Idle"))
            {
                weaponAnimator.Play("Weapon_Idle", 0, playerIdleTime % 1f);
            }


        }
    
       ManageFootsteps();

    }
    void FixedUpdate()
    {

        if (playerHealth != null && playerHealth.knockbackTimer > 0.5)
        {
            return;
        }
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);


    }
    // Hàm gọi khi phím Dash được nhấn
    private void OnDash(InputAction.CallbackContext context)
    {
        if (playerHealth.isDead || playerHealth.knockbackTimer > 0.5 || isDashing || currentDashCharges <= 0)
            return;

        Vector2 currentMoveDirection = playerControls.ReadValue<Vector2>().normalized;

        // Phải di chuyển mới dash
        if (currentMoveDirection.sqrMagnitude > 0.01f)
        {
            // Nếu đang đầy stack thì mới bắt đầu đếm ngược.
            // Nếu stack đang vơi (đang đếm ngược để hồi stack trước) thì KHÔNG reset timer
            dashCooldownTimer = dashCooldown;

            // Trừ Charge
            currentDashCharges--;
            Debug.Log($"Dashed! Remaining Charges: {currentDashCharges}");

            StartCoroutine(PerformDash(currentMoveDirection));
        }
    }


    private IEnumerator PerformDash(Vector2 direction)
    {
        isDashing = true;
        audioSource.PlayOneShot(dashSound);
        // 1. Vô hiệu hóa sát thương
        if (playerHealth != null)
        {
            playerHealth.SetInvulnerability(true); 
            GetComponent<Collider2D>().enabled = false;
        }

        // 2. Bật DashTrail
        if (dashTrail != null) dashTrail.SetActive(true);

        // Đặt vận tốc Dash 
        float originalMoveSpeed = moveSpeed;
        moveSpeed = dashSpeed; // Tăng tốc độ

        // Khóa hướng di chuyển trong khi Dash
        Vector2 dashVelocity = direction * dashSpeed;
        rb.linearVelocity = dashVelocity; // Set velocity ngay lập tức

        // 3. Chờ hết thời gian Dash
        yield return new WaitForSeconds(dashTime);

        // 4. Kết thúc Dash
        isDashing = false;
        moveSpeed = stats.TotalMoveSpeed; // Khôi phục tốc độ ban đầu

        // 5. Khôi phục trạng thái
        if (playerHealth != null)
        {
            playerHealth.SetInvulnerability(false); 
            GetComponent<Collider2D>().enabled = true; 
        }

        // 6. Tắt DashTrail
        if (dashTrail != null) dashTrail.SetActive(false);
    }
    private void ManageFootsteps()
    {
        // Điều kiện 1: Đang di chuyển
        // Điều kiện 2: KHÔNG đang bị knockback (đã được kiểm tra ở đầu Update, nhưng kiểm tra lại cho chắc)
        if (IsMoving && playerHealth.knockbackTimer <= 0.5)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                if (audioSource != null && footstepSound != null)
                {
                    // Phát âm thanh bước chân một lần
                    audioSource.PlayOneShot(footstepSound, 0.5f);
                }
                // Thiết lập lại timer
                footstepTimer = footstepDelay;
            }
        }
        else
        {
            // Nếu không di chuyển hoặc đang knockback, reset timer để tiếng bước chân đầu tiên phát ngay
            // khi Player bắt đầu di chuyển lại.
            footstepTimer = footstepDelay;
        }
    }

}
