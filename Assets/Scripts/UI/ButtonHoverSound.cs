using UnityEngine;

public class ButtonHoverSound : MonoBehaviour
{
    // Kéo thả file âm thanh (AudioClip) vào đây trong Inspector
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip cardSound;
    public AudioClip upgradeSound;

    // Tham chiếu đến AudioSource
    private AudioSource audioSource;

    void Start()
    {
        // Lấy tham chiếu đến Audio Source trên cùng đối tượng
        audioSource = GetComponent<AudioSource>();
    }

    // Hàm public được gọi bởi Event Trigger
    public void PlayHoverSound()
    {
        if (audioSource != null && hoverSound != null)
        {
            // Phát âm thanh MỘT LẦN duy nhất
            audioSource.PlayOneShot(hoverSound);
        }
    }
    public void PlayClickSound()
    {
        if (audioSource != null && hoverSound != null)
        {
            // Phát âm thanh MỘT LẦN duy nhất
            audioSource.PlayOneShot(clickSound);
        }
    }
    public void PlayCardSound()
    {
        AudioSource.PlayClipAtPoint(cardSound, Camera.main.transform.position);
    }

    public void PlayUpgradeSound()
    {
        if (audioSource != null && hoverSound != null)
        {
            // Phát âm thanh MỘT LẦN duy nhất
            audioSource.PlayOneShot(upgradeSound);
        }
    }
}