using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.Rendering.DebugUI;

public class ThunderHitbox : MonoBehaviour
{
    [Header("Damage & Knockback")]
    public int damageToPlayer = 20;
    private AudioSource audioSource;
    public AudioClip thunder;
    public Collider2D hitbox;     // Cờ để đảm bảo chỉ gây sát thương 1 lần mỗi lần HitBox được kích hoạt


    private void Start()
    {
        hitbox = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameObject.activeInHierarchy && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
               
              
                playerHealth.TakeDamage(damageToPlayer);

            }

        }
    }

    public void ActivateHitBox()
    {
        hitbox.enabled = true;
        if (thunder != null)
        {
            audioSource.PlayOneShot(thunder);
        }
    }

    public void DeactivateHitBox()
    {
        hitbox.enabled = false;
    }


}