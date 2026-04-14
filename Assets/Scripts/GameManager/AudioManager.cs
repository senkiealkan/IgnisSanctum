using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;
    private AudioSource activeSource;

    [Header("Settings")]
    public float musicVolume = 1f;
    public float crossFadeDuration = 2.0f;

    private List<AudioClip> currentPlaylist;
    private int currentTrackIndex = 0;
    private bool isPlayingPlaylist = false;
    private Coroutine playlistCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (sourceA == null) sourceA = gameObject.AddComponent<AudioSource>();
            if (sourceB == null) sourceB = gameObject.AddComponent<AudioSource>();

            sourceA.loop = false;
            sourceB.loop = false;
            activeSource = sourceA;
        }
        else Destroy(gameObject);
    }

    public void PlayPlaylist(List<AudioClip> playlist)
    {
        if (playlist == null || playlist.Count == 0) return;
        if (currentPlaylist == playlist && isPlayingPlaylist && activeSource.isPlaying) return; 

        currentPlaylist = playlist;
        currentTrackIndex = 0;
        isPlayingPlaylist = true;

        CrossFadeToNewClip(currentPlaylist[0], false);

        if (playlistCoroutine != null) StopCoroutine(playlistCoroutine);
        playlistCoroutine = StartCoroutine(PlaylistRoutine());
    }

    private IEnumerator PlaylistRoutine()
    {
        while (isPlayingPlaylist)
        {
            // Kiểm tra: 
            // 1. Nếu nhạc đang chạy và sắp hết -> Chuyển
            // 2. Nếu nhạc ĐÃ DỪNG (do lỡ nhịp check) nhưng vẫn đang chế độ Playlist -> Chuyển

            bool shouldSwitch = false;

            if (activeSource.clip != null)
            {
                if (activeSource.isPlaying)
                {
                    // Đang chạy: Check thời gian còn lại
                    float remainingTime = activeSource.clip.length - activeSource.time;
                    if (remainingTime <= crossFadeDuration)
                    {
                        shouldSwitch = true;
                    }
                }
                else
                {
                    if (activeSource.time == 0 || activeSource.time >= activeSource.clip.length)
                    {
                        shouldSwitch = true;
                    }
                }
            }

            if (shouldSwitch)
            {
                currentTrackIndex++;
                if (currentTrackIndex >= currentPlaylist.Count) currentTrackIndex = 0;

                CrossFadeToNewClip(currentPlaylist[currentTrackIndex], false);

                // Chờ đúng bằng thời gian fade để tránh gọi chồng chéo
                // Dùng Realtime để không bị ảnh hưởng bởi Pause Game
                yield return new WaitForSecondsRealtime(crossFadeDuration);
            }

            // Check tần suất dày hơn (0.5s) để bắt nhịp chuẩn hơn
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    public void PlayBossMusic(AudioClip bossClip)
    {
        if (bossClip == null) return;
        if (activeSource.clip == bossClip && activeSource.isPlaying) return;

        isPlayingPlaylist = false;
        if (playlistCoroutine != null) StopCoroutine(playlistCoroutine);

        CrossFadeToNewClip(bossClip, true);
    }

    public void StopMusic()
    {
        isPlayingPlaylist = false;
        if (playlistCoroutine != null) StopCoroutine(playlistCoroutine);

        StartCoroutine(FadeOutSource(sourceA));
        StartCoroutine(FadeOutSource(sourceB));
    }

    private void CrossFadeToNewClip(AudioClip newClip, bool loop)
    {
        // Chỉ dừng các coroutine fade, không dừng playlist routine )
        StopAllCoroutines();

        AudioSource newSource = (activeSource == sourceA) ? sourceB : sourceA;
        AudioSource oldSource = activeSource;

        newSource.clip = newClip;
        newSource.volume = 0;
        newSource.loop = loop;
        newSource.Play();

        activeSource = newSource;

        StartCoroutine(CrossFadeRoutine(oldSource, newSource));

        if (isPlayingPlaylist && !loop)
        {
            playlistCoroutine = StartCoroutine(PlaylistRoutine());
        }
    }

    // --- DÙNG UNSCALED TIME ĐỂ KHÔNG BỊ TREO KHI PAUSE ---
    private IEnumerator CrossFadeRoutine(AudioSource outSource, AudioSource inSource)
    {
        float timer = 0f;
        while (timer < crossFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float percent = timer / crossFadeDuration;

            if (outSource.isPlaying)
                outSource.volume = Mathf.Lerp(musicVolume, 0f, percent);

            inSource.volume = Mathf.Lerp(0f, musicVolume, percent);

            yield return null;
        }

        outSource.Stop();
        outSource.volume = 0;
        inSource.volume = musicVolume;
    }

    private IEnumerator FadeOutSource(AudioSource source)
    {
        float startVol = source.volume;
        while (source.volume > 0)
        {
            source.volume -= Time.unscaledDeltaTime;
            yield return null;
        }
        source.Stop();
        source.volume = startVol;
    }
}