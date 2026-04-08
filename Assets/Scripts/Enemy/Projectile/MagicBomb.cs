using UnityEngine;

public class MagicBomb : MonoBehaviour
{
    [Header("Settings")]
    public float warningTime = 1.5f; // Thời gian nhấp nháy cảnh báo
    public float explosionRadius = 2.5f;
    public float damage = 20f;
    public float knockbackForce = 5f;
    public LayerMask playerLayer;

    [Header("Visuals")]
    public GameObject warningVisual; // Sprite vòng tròn cảnh báo
    public GameObject explosionVisual; // Sprite/Effect nổ
    public AudioClip explosionSound;
    private AudioSource audioSource;

    private bool hasExploded = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Lúc đầu hiện cảnh báo, ẩn vụ nổ
        if (warningVisual) warningVisual.SetActive(true);
        if (explosionVisual) explosionVisual.SetActive(false);

        // Bắt đầu đếm ngược nổ
        Invoke("Explode", warningTime);
    }

    void Explode()
    {
        hasExploded = true;

        // 1. Đổi Visual
        if (warningVisual) warningVisual.SetActive(false);
        if (explosionVisual) explosionVisual.SetActive(true);

        // 2. Phát âm thanh
        audioSource.PlayOneShot(explosionSound);

        // 3. Gây damage (AoE)
        Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);
        if (hit != null && hit.CompareTag("Player"))
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                ph.ApplyKnockback(dir * knockbackForce);
                ph.TakeDamage(damage);
            }
        }

        // 4. Tự hủy sau khi hiệu ứng nổ xong (khoảng 0.5 - 1s)
        Destroy(gameObject, 0.8f);
    }

    private void OnDrawGizmos()
    {
        // Vẽ vòng tròn đỏ để debug bán kính nổ
        Gizmos.color = new Color(1, 0, 0, 0.4f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}