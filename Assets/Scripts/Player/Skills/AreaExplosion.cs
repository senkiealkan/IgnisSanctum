using UnityEngine;
using UnityEngine.Audio;

public class AreaExplosion : MonoBehaviour
{
    public float radius = 3f; // Bán kính nổ


    [HideInInspector] public float damage;
    [HideInInspector] public XPManager xpManager;
    private AudioSource audioSource;
    public AudioClip explodeSound;
    public bool isCritical = false;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(explodeSound);
        Destroy(gameObject, 0.5f);


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