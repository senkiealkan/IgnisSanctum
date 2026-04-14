using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class NecromancerSummon : MonoBehaviour
{
    public GameObject projectilePrefab; 
    public Transform firePoint; // Điểm xuất phát của Projectile 
    private EnemyMovement_Attacker enemyMovement;
    private Animator animator;
    private AudioSource audioSource;
    public AudioClip attackingSound;


    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement_Attacker>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

 
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned on " + gameObject.name);
        }
    }
    private void Update()
    {
       
        if (enemyMovement != null && enemyMovement.isAttacking)
        {
            animator.SetBool("isAttacking", true);
        }

    }
    public void ShootProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

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

    public void PlayAttackingSound()
    {
        if (!enemyMovement.isMoving) audioSource.PlayOneShot(attackingSound);
    }
}