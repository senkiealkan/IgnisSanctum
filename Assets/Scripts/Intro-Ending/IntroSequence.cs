using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroSequence : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI introText;
    public CanvasGroup transitionOverlay; 
    public string nextSceneName = "Arena0-Tutorial";

    [Header("Timing Settings")]
    public float fadeInTime = 1.5f;
    public float stayTime = 2.5f;
    public float fadeOutTime = 1.0f;
    public float interval = 0.5f;
    public float sceneTransitionTime = 1.0f; // Thời gian màn hình bùng sáng

    [TextArea(3, 10)]
    public string[] lines;

    private GameObject essentials;

    void Start()
    {
        if (introText != null) introText.alpha = 0;
        if (transitionOverlay != null) transitionOverlay.alpha = 0; // Đảm bảo ban đầu trong suốt

        StartCoroutine(StartSequenceRoutine());
    }

    IEnumerator StartSequenceRoutine()
    {
        // 1. Đợi Essentials ổn định
        yield return null;

        // 2. Tắt Player/UI
        essentials = GameObject.Find("Essentials");
        if (essentials != null) ToggleGameplay(false);

        // 3. Chạy Intro Text
        yield return StartCoroutine(PlayIntroFade());
    }

    IEnumerator PlayIntroFade()
    {
        foreach (string line in lines)
        {
            introText.text = line;
            yield return StartCoroutine(FadeAlpha(introText, 0f, 1f, fadeInTime)); // Hiện chữ
            yield return new WaitForSeconds(stayTime);
            yield return StartCoroutine(FadeAlpha(introText, 1f, 0f, fadeOutTime)); // Ẩn chữ
            yield return new WaitForSeconds(interval);
        }

        // --- HIỆU ỨNG CHUYỂN SCENE ---
        if (transitionOverlay != null)
        {
            // Bật màn hình trắng dần lên (0 -> 1)
            yield return StartCoroutine(FadeCanvasGroup(transitionOverlay, 0f, 1f, sceneTransitionTime));
        }

        SceneManager.LoadScene(nextSceneName);
    }

    // Hàm Fade dùng chung cho cả Text (TMP)
    IEnumerator FadeAlpha(TextMeshProUGUI text, float start, float end, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            text.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        text.alpha = end;
    }

    // Hàm Fade dùng chung cho Panel (CanvasGroup)
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    void ToggleGameplay(bool isActive)
    {
        if (essentials == null) return;
        Transform player = essentials.transform.Find("Cael");
        if (player != null) player.gameObject.SetActive(isActive);

        Transform arenaUI = essentials.transform.Find("ArenaUI");
        if (arenaUI != null)
        {
            CanvasGroup cg = arenaUI.GetComponent<CanvasGroup>();
            if (cg == null) cg = arenaUI.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = isActive ? 1f : 0f;
            cg.blocksRaycasts = isActive;
        }
    }
}