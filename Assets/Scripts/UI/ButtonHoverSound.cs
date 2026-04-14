using UnityEngine;

public class ButtonHoverSound : MonoBehaviour
{
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip cardSound;
    public AudioClip upgradeSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayHoverSound()
    {
        if (audioSource != null && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }
    public void PlayClickSound()
    {
        if (audioSource != null && hoverSound != null)
        {
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
            audioSource.PlayOneShot(upgradeSound);
        }
    }
}