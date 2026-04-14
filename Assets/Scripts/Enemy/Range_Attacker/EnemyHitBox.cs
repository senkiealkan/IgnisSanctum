using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.Rendering.DebugUI;

public class EnemyHitBox : MonoBehaviour
{
    [Header("Damage & Knockback")]
    public int damageToPlayer = 10;
    public float knockbackForce = 10f;
    private AudioSource audioSource;
    private EnemyHealth enemyHealth;
    public AudioClip slash;
    // Cờ để đảm bảo chỉ gây sát thương 1 lần mỗi lần HitBox được kích hoạt
    public bool hasHitPlayer = false;

    private void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        audioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        hasHitPlayer = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ xử lý va chạm nếu HitBox đang active
        if (gameObject.activeInHierarchy && other.CompareTag("Player") && !hasHitPlayer && enemyHealth.isDead == false)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                if (slash!= null)
                {
                    audioSource.PlayOneShot(slash);
                }

                Transform enemyRoot = transform.root;
                Vector2 knockbackDirection = (other.transform.position - enemyRoot.position).normalized;
                playerHealth.TakeDamage(damageToPlayer);

                playerHealth.ApplyKnockback(knockbackDirection * knockbackForce);
             
                // Đánh dấu đã va chạm để tránh gây sát thương lặp lại trong 1 lần tấn công
                hasHitPlayer = true;
            }
  
        }
    }

    public void ResetHit()
    {
        hasHitPlayer = false;
    }

 
}