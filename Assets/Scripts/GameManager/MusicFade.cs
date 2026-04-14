using UnityEngine;
using System.Collections;

public class MusicFade : MonoBehaviour
{
    public AudioSource musicSource;

    void Awake()
    {
        if (musicSource == null) musicSource = GetComponent<AudioSource>();
    }

    public IEnumerator FadeOutMusic(float duration)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Giảm volume từ mức hiện tại về 0
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop(); 
    }

    // Hàm Fade In 
    public IEnumerator FadeInMusic(float duration, float targetVolume = 1f)
    {
        if (musicSource == null) yield break;

        musicSource.volume = 0f;
        musicSource.Play();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }
}