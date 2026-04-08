using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class EnemyRangedCombat : MonoBehaviour
{
    public GameObject projectilePrefab; // Projectile Prefab
    public Transform firePoint; // Điểm xuất phát của Projectile (thường là miệng/tay của Enemy)
    private Transform playerTarget;
    private EnemyMovement_Attacker enemyMovement;
    private Animator animator;
    private AudioSource audioSource;
    public AudioClip attackingSound;


    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement_Attacker>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        // Tìm Player Target (hoặc lấy từ EnemyMovement_Attacker)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }

        // Đảm bảo firePoint được gán
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned on " + gameObject.name);
        }
    }
    private void Update()
    {
        // Kiểm tra nếu Enemy đang trong trạng thái tấn công
        if (enemyMovement != null && enemyMovement.isAttacking)
        {
            animator.SetBool("isAttacking", true);
        }
        
    }
    // Hàm này được gọi bằng Animation Event trong animation tấn công của Enemy
    public void ShootProjectile()
    {
        if (projectilePrefab != null && firePoint != null && playerTarget != null)
        {
            // 1. Tạo Projectile
            GameObject projectileObject = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // 2. Khởi tạo Projectile
            ChasedProjectileAttacker projectileScript = projectileObject.GetComponent<ChasedProjectileAttacker>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(playerTarget);
            }
            else
            {
                Debug.LogError("Projectile Prefab does not have EnemyProjectile_Attacker script!");
            }
        }
        else
        {
            Debug.LogWarning("Missing required components for shooting projectile.");
        }
    }
    public void EndAttack()
    {
        animator.SetBool("isAttacking", false);
        if (enemyMovement != null)
        {
            enemyMovement.isAttackInProgress = false;
            enemyMovement.isAttacking = false;
        }
       
    }
   
    // Thêm hàm phát âm thanh cho Vung vũ khí
    public void PlayAttackingSound()
    {
        if (!enemyMovement.isMoving) audioSource.PlayOneShot(attackingSound);
    }
}