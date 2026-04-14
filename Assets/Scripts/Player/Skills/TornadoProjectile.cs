using UnityEngine;
using UnityEngine.Audio;

public class TornadoProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 10f;
    private Animator animator;
    public bool isCritical = false;
    [HideInInspector] public Vector2 flyDirection;
    [HideInInspector] public float damage; 
    [HideInInspector] public XPManager xpManager; 

    private AudioSource audioSource;
    public AudioClip movingSound;

    private Rigidbody2D rb;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource.PlayOneShot(movingSound);
        // Tự hủy sau một thời gian nếu không trúng gì
        Destroy(gameObject, lifetime);

        // Gán vận tốc bay thẳng về phía trước (Right vector của object)
        if (flyDirection != Vector2.zero)
        {
            rb.linearVelocity = flyDirection * speed;
        }
        else
        {
            // Fallback nếu quên gán hướng: Bay thẳng theo hướng mặt
            rb.linearVelocity = transform.right * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, isCritical);
                if (xpManager != null) xpManager.GainXP(damage);
            }
        }
        
    }
 
}