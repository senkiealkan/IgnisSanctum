using UnityEngine;

public class CollideTakeDamage : MonoBehaviour
{
    private void Update()
    {
        Destroy(gameObject, 5f);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {

                playerHealth.ApplyKnockback(knockbackDirection * 10f);
                playerHealth.TakeDamage(10);
            }

        }
    }
}
