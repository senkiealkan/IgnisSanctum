using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class EnemyCombat : MonoBehaviour
{
    private EnemyMovement_Attacker enemyMovement;
    private Animator animator;
    public Collider2D hitbox;
    private AudioSource audioSource;
    public AudioClip attackingSound;
    public EnemyHitBox enemyHitBox;
    public float attackAnimationDuration = 1.8f;
 
   

    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement_Attacker>();
        animator = GetComponent<Animator>();
        hitbox.GetComponentInChildren<Collider2D>();
        hitbox.enabled = false;
        audioSource  = GetComponent<AudioSource>();
      
        enemyHitBox = GetComponentInChildren<EnemyHitBox>();
    }

    void Update()
    {
        // Kiểm tra nếu Enemy đang trong trạng thái tấn công
        if (enemyMovement != null && enemyMovement.isAttacking)
        {
            animator.SetBool("isAttacking", true);
        }
        else if (enemyMovement != null && !enemyMovement.isAttackInProgress)
        {
            DeactivateHitBox();
        }
    }

    // Kích hoạt bằng animation event
    public void EndAttack()
    {
        animator.SetBool("isAttacking", false);
        if (enemyMovement != null)
        {
            enemyMovement.isAttackInProgress = false;
            enemyMovement.isAttacking = false;
        }
        if (enemyHitBox != null)
        {
            enemyHitBox.ResetHit();
        }
    }
    public void ActivateHitBox()
    {
        hitbox.enabled = true;
        // RESET CỜ ĐẢM BẢO CHẮC CHẮN LUÔN CÓ SÁT THƯƠNG
        if (enemyHitBox != null)
        {
            enemyHitBox.ResetHit();
        }
    }

    public void DeactivateHitBox()
    {
        hitbox.enabled = false;
    }

    // Thêm hàm phát âm thanh cho Vung vũ khí
    public void PlayAttackingSound()
    {
        audioSource.PlayOneShot(attackingSound);
    }
}