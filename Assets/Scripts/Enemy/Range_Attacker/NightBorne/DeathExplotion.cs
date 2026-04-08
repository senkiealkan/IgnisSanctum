using UnityEngine;
using UnityEngine.Audio;

public class DeathExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 2.5f;    
    public int damageToPlayer = 20;          
    public float playerKnockbackForce = 10f;
    public Vector2 explosionOffset;

    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip explosionSound;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void PerformExplosion()
    {
        Vector2 explosionCenter = (Vector2)transform.position + explosionOffset;
        audioSource.PlayOneShot(explosionSound);
        // Tìm tất cả các Collider2D trong phạm vi hình tròn
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionCenter, explosionRadius);
        foreach (Collider2D other in hitColliders)
        {
            // Kiểm tra nếu đối tượng trúng đòn là Player
            if (other.CompareTag("Player"))
            {
                // Tính toán hướng đẩy lùi: (Vị trí Player - Vị trí tâm nổ)
                Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;

                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Áp dụng lực đẩy và sát thương như logic bạn yêu cầu
                    playerHealth.ApplyKnockback(knockbackDirection * playerKnockbackForce);
                    playerHealth.TakeDamage(damageToPlayer);

                    Debug.Log("Player bị trúng nổ!");
                }
            }
        }
    }

    // Vẽ Gizmos để bạn dễ dàng căn chỉnh phạm vi nổ trong cửa sổ Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 explosionCenter = (Vector2)transform.position + explosionOffset;
        Gizmos.DrawWireSphere(explosionCenter, explosionRadius);
    }
}