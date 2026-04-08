using UnityEngine;

public class GemPickup : MonoBehaviour
{
    public GemType gemType;
    public int amount = 1;

    [Header("Magnet Settings")]
    public float flySpeed = 5f;        // Tốc độ bay khởi điểm
    public float acceleration = 10f;   // Gia tốc (bay càng lâu càng nhanh)
    public float stopDistance = 0.5f;  // Khoảng cách để "ăn" gem

    [Header("Audio")]
    public AudioClip pickUpSound;
    public GameObject pickupEffect;
    // Không cần AudioSource component trên object nữa nếu dùng PlayClipAtPoint
    // Nhưng nếu muốn chỉnh volume thì vẫn cần logic khác. 
    // Ở đây mình dùng PlayClipAtPoint cho đơn giản và hiệu quả.

    private Transform targetPlayer;
    private bool isAbsorbing = false;
    private void Start()
    {
      
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ kích hoạt nếu chưa bị hút và đối tượng là Player
        if (other.CompareTag("Player") && !isAbsorbing)
        {
            StartAbsorbing(other.transform);
        }
    }

    private void StartAbsorbing(Transform playerTransform)
    {
        isAbsorbing = true;
        targetPlayer = playerTransform;

        // Tắt Collider ngay lập tức để tránh Gem bị va chạm/cản trở khi bay
        // hoặc tránh bị trigger 2 lần
        GetComponent<Collider2D>().enabled = false;

        // Tùy chọn: Tắt hiệu ứng trọng lực (nếu Gem có Rigidbody) để nó bay mượt
        /*
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        */
    }

    private void Update()
    {
        if (isAbsorbing && targetPlayer != null)
        {
            FlyToPlayer();
        }
    }

    private void FlyToPlayer()
    {
        // 1. Tăng tốc độ theo thời gian để tạo cảm giác lực hút mạnh dần
        flySpeed += acceleration * Time.deltaTime;

        // 2. Di chuyển về phía Player
        transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, flySpeed * Time.deltaTime);

        // 3. Kiểm tra khoảng cách
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        if (distance <= stopDistance)
        {
            CollectGem();
        }
    }

    private void CollectGem()
    {
        // Gọi Manager để cộng tiền
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.CollectGem(gemType, amount);
        }

        Instantiate(pickupEffect, transform.position, Quaternion.identity);
        if (pickUpSound != null)
        {
            // 1. Dùng vị trí của Gem (hoặc Player) thay vì vị trí Camera
            Vector3 audioPos = transform.position;

            // 2. Chơi âm thanh với độ lệch cao độ (pitch) ngẫu nhiên 
            //    để giảm lỗi chồng pha và tạo cảm giác sống động (Juicy)
            float randomPitch = Random.Range(0.9f, 1.1f); // Lệch 10%

            // Sử dụng hàm tĩnh để chơi âm thanh
            PlaySoundWithPitch(pickUpSound, audioPos, 1f, randomPitch);
        }

        Destroy(gameObject);
    }
    public static void PlaySoundWithPitch(AudioClip clip, Vector3 position, float volume, float pitch)
    {
        if (clip == null) return;

        // 1. Tạo GameObject tạm thời
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        // 2. Thêm AudioSource
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = volume;
        aSource.pitch = pitch; // Áp dụng pitch ngẫu nhiên

        // 3. Phát và tự hủy sau khi clip kết thúc
        aSource.Play();
        Destroy(tempGO, clip.length);
    }
}