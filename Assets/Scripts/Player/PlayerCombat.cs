using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public bool IsAttacking => isAttacking;
    public TrailRenderer weaponTrail;
    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip swing;
    public AudioClip slash;


    [Header("References")]
    public Transform weaponPivot;          // điểm tay cầm
    public SpriteRenderer weaponRenderer;  // sprite của kiếm
    public Animator playerAnimator;        // animator của nhân vật
    private DamageArea damageArea;
    private PlayerHealth playerHealth;

    [Header("Settings")]
    public float swingDownTime = 0.25f;     // thời gian chém xuống (frame 0–20)
    public float swingUpTime = 0.35f;       // thời gian đưa kiếm về vai (frame 20–60)
    public float swingAngle = 270f;        // góc quét kiếm

    private bool isAttacking = false;
    private float timer = 0f;
    private bool isFacingRight = true;     // dựa vào flipX trong Animator

    private float startAngle;   // góc bắt đầu
    private float midAngle;     // góc thấp nhất (sau khi chém xuống)
    private float endAngle;     // góc idle (vác kiếm trên vai)

    [Header("Attack Timing")]
    public float preAttackPause = 0.1f;    // dừng trước khi bắt đầu chém
    public float postAttackPause = 0.1f;   // dừng sau khi chém xuống hết

    [Header("Pivot Movement")]
    public Vector2 idlePivotPos = new Vector2(0.8f, -0.1f);
    public Vector2 downPivotPos = new Vector2(1f, -1.7f); // tùy chỉnh để hạ tay khi chém


    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        weaponPivot.GetComponentInChildren<Collider2D>().enabled = false;
        audioSource = GetComponent<AudioSource>();
        weaponTrail.emitting = false;
        damageArea = weaponPivot.GetComponentInChildren<DamageArea>(true);
        if (damageArea != null)
    {
        // Gán AudioSource (tương tự như cách khắc phục trước)
        damageArea.playerAudioSource = audioSource; 
    }
    }
    private void Update()
    {
        if (playerHealth.knockbackTimer > 0) // <--- KIỂM TRA QUAN TRỌNG
        {
            // [FIX] Nếu đang tấn công mà bị dính đòn (Knockback) -> Hủy tấn công ngay
            if (isAttacking)
            {
                EndAttack();
            }

            // Player đang bị Knockback, thoát ra không xử lý input tiếp
            return;
        }

        if (playerHealth.isDead) return;
        HandleInput();
        HandleAttack();
    }

    private void HandleInput()
    {

        // Khi nhấn chuột trái thì chém
        if (Mouse.current.leftButton.wasPressedThisFrame && !isAttacking)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        // RESET flag âm thanh slash
        if (damageArea != null)
        {
            damageArea.ResetSlashSound();
        }
        if (playerMovement.IsMoving == false)
        {
            playerAnimator.SetBool("isAttacking", true);
            playerAnimator.Play("Player_Attack", 0, 0f);
            //playerMovement.enabled = false;
        }
       
        isFacingRight = transform.localScale.x > 0;
        isAttacking = true;
        timer = 0f;




        // Thiết lập góc
        if (isFacingRight)
        {
            startAngle = -0f;               // idle (vác vai bên phải)
            midAngle = startAngle - swingAngle; // quét xuống (270°)
            endAngle = -0f;                 // quay lại idle
        }
        else
        {
            startAngle = 0f;              // idle bên trái
            midAngle = startAngle - swingAngle;
            endAngle = 0f;
        }
        
         // kiếm trước người
       
        weaponPivot.localRotation = Quaternion.Euler(0, 0, startAngle);
    }

    private void HandleAttack()
    {
        if (!isAttacking) return;

        timer += Time.deltaTime;
        // Tổng thời gian chia ra:
        // [0 - preAttackPause]         -> đứng yên (chuẩn bị)
        // [preAttackPause - preAttackPause+swingDownTime] -> chém xuống
        // [dừng postAttackPause tại đáy]
        // [sau đó swingUpTime -> đưa về idle]

        float totalDownEnd = preAttackPause + swingDownTime;
        float postPauseEnd = totalDownEnd + postAttackPause;
        float totalDuration = preAttackPause + swingDownTime + postAttackPause + swingUpTime;
        if (timer <= preAttackPause)
        {
            // Giai đoạn chuẩn bị — giữ nguyên tư thế idle
            weaponPivot.localRotation = Quaternion.Euler(0, 0, startAngle);
            weaponPivot.localPosition = idlePivotPos;
            
        }
        else if (timer <= totalDownEnd)
        {

            weaponTrail.emitting = true;
            //kích hoạt hitbox trong giai đoạn chém
            weaponPivot.GetComponentInChildren<Collider2D>().enabled = true;

            float t = (timer - preAttackPause) / swingDownTime;
            float currentAngle = Mathf.Lerp(startAngle, midAngle, t);
            weaponPivot.localRotation = Quaternion.Euler(0, 0, currentAngle);
            weaponRenderer.sortingOrder = 3;
            Vector2 currentPos = Vector2.Lerp(idlePivotPos, downPivotPos, t);
            weaponPivot.localPosition = currentPos;
            
        }
        else if (timer <= postPauseEnd)
        {
            // Giai đoạn chém xuống
            if (timer - Time.deltaTime <= totalDownEnd)
            {
                audioSource.PlayOneShot(swing);
            }
            weaponTrail.emitting = false;
            // Dừng lại ở đáy cú chém
            weaponPivot.localRotation = Quaternion.Euler(0, 0, midAngle);
            weaponPivot.localPosition = downPivotPos;
        }
        else if (timer <= totalDuration)
        {
            weaponPivot.GetComponentInChildren<Collider2D>().enabled = false;
            // Giai đoạn đưa kiếm về vai
            float t = (timer - postPauseEnd) / swingUpTime;
            float currentAngle = Mathf.Lerp(midAngle, endAngle, t);
            weaponPivot.localRotation = Quaternion.Euler(0, 0, currentAngle);

            Vector2 currentPos = Vector2.Lerp(downPivotPos, idlePivotPos, t);
            weaponPivot.localPosition = currentPos;
        }
        else
        {
            EndAttack();
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
        timer = 0f;

        // [FIX QUAN TRỌNG] Tắt ngay lập tức hitbox và hiệu ứng khi kết thúc (hoặc bị hủy)
        weaponPivot.GetComponentInChildren<Collider2D>().enabled = false;
        weaponTrail.emitting = false;

        // Reset lại hiển thị
        weaponRenderer.sortingOrder = 1;
        playerAnimator.SetBool("isAttacking", false);

        // Đưa kiếm về vị trí và góc Idle
        weaponPivot.localRotation = Quaternion.Euler(0, 0, endAngle);
        weaponPivot.localPosition = idlePivotPos; // [FIX] Trả vị trí tay về chỗ cũ nếu đang chém dở
    }


}
