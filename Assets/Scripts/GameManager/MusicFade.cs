using UnityEngine;
using System.Collections;

public class MusicFade : MonoBehaviour
{
    public AudioSource musicSource;

    // Tự động tìm AudioSource nếu quên kéo thả
    void Awake()
    {
        if (musicSource == null) musicSource = GetComponent<AudioSource>();
    }

    // Hàm này để gọi từ script khác (trả về Coroutine để script kia biết mà đợi)
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
        musicSource.Stop(); // Tắt hẳn cho nhẹ nợ
    }

    // Hàm Fade In (nếu cần khi vào Scene mới)
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