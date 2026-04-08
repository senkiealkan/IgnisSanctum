using UnityEngine;

public class PotionPickup : MonoBehaviour
{
    public PotionType type;
    public int amount = 1;
    public GameObject pickupEffect;
    [Header("Magnet Settings")]
    public float flySpeed = 5f;
    public float stopDistance = 0.5f;

    private Transform targetPlayer;
    private bool isAbsorbing = false;

    [Header("Audio")]
    public AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isAbsorbing)
        {
            isAbsorbing = true;
            targetPlayer = other.transform;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    private void Update()
    {
        if (isAbsorbing && targetPlayer != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, flySpeed * Time.deltaTime);
            flySpeed += 10f * Time.deltaTime; // Gia tốc

            if (Vector3.Distance(transform.position, targetPlayer.position) <= stopDistance)
            {
                Collect(targetPlayer.gameObject);
            }
        }
    }

    private void Collect(GameObject player)
    {
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            inventory.AddPotion(type, amount);
        }
        Instantiate(pickupEffect, transform.position, Quaternion.identity);
        if (pickupSound != null)
        {
            // 1. Dùng vị trí của Gem (hoặc Player) thay vì vị trí Camera
            Vector3 audioPos = transform.position;

            // 2. Chơi âm thanh với độ lệch cao độ (pitch) ngẫu nhiên 
            //    để giảm lỗi chồng pha và tạo cảm giác sống động (Juicy)
            float randomPitch = Random.Range(0.9f, 1.1f); // Lệch 10%

            // Sử dụng hàm tĩnh để chơi âm thanh
            PlaySoundWithPitch(pickupSound, audioPos, 1f, randomPitch);
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