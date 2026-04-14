using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Để dùng Button
using System.Collections;

public class EndingSequence : MonoBehaviour
{
    [Header("1. Story Text Settings")]
    public TextMeshProUGUI storyText;
    [TextArea(2, 5)] public string[] endingLines;
    public float textFadeDuration = 1.5f;
    public float textStayDuration = 3.0f;

    [Header("2. Title & Credits Settings")]
    public CanvasGroup gameTitleCG;   
    public CanvasGroup creditsCG;     
    public float elementFadeDuration = 2.0f; // Thời gian hiện Title/Credits
    public float elementStayDuration = 4.0f; // Thời gian giữ Title/Credits

    [Header("3. Button & Music Settings")]
    public CanvasGroup buttonCG;      
    public Button returnButton;       
    public AudioSource musicSource;   
    public string menuSceneName = "MainMenu";

    private bool hasClickedButton = false; 

    void Start()
    {
        // Setup ban đầu: Ẩn tất cả, chỉ để màn hình đen
        storyText.alpha = 0;
        gameTitleCG.alpha = 0;
        creditsCG.alpha = 0;
        buttonCG.alpha = 0;
        buttonCG.interactable = false;
        buttonCG.blocksRaycasts = false;

        returnButton.onClick.AddListener(OnReturnClick);

        // Bắt đầu chuỗi sự kiện
        StartCoroutine(PlayEndingSequence());
    }

    IEnumerator PlayEndingSequence()
    {
        yield return new WaitForSeconds(1f);
        // === GIAI ĐOẠN 1: CỐT TRUYỆN ===
        foreach (string line in endingLines)
        {
            storyText.text = line;
            yield return StartCoroutine(FadeText(storyText, 0f, 1f, textFadeDuration)); // Hiện
            yield return new WaitForSeconds(textStayDuration);
            yield return StartCoroutine(FadeText(storyText, 1f, 0f, textFadeDuration)); // Ẩn
            yield return new WaitForSeconds(0.5f); // Nghỉ tí
        }

        // === GIAI ĐOẠN 2: HIỆN TITLE GAME ===
        yield return StartCoroutine(FadeCanvasGroup(gameTitleCG, 0f, 1f, elementFadeDuration));
        yield return new WaitForSeconds(elementStayDuration);

        yield return new WaitForSeconds(0.5f);

        // === GIAI ĐOẠN 3: HIỆN CREDITS ===
        yield return StartCoroutine(FadeCanvasGroup(creditsCG, 0f, 1f, elementFadeDuration));
        yield return new WaitForSeconds(elementStayDuration); 
        yield return StartCoroutine(FadeCanvasGroup(creditsCG, 1f, 0f, elementFadeDuration));

        // === GIAI ĐOẠN 4: HIỆN NÚT VỀ MENU ===
        yield return new WaitForSeconds(1f);
        // Hiện nút
        yield return StartCoroutine(FadeCanvasGroup(buttonCG, 0f, 1f, 1.0f));
        // Bật khả năng bấm nút
        buttonCG.interactable = true;
        buttonCG.blocksRaycasts = true;

        // === GIAI ĐOẠN 5: CHỜ HẾT NHẠC ===
        if (musicSource != null && musicSource.clip != null)
        {
            // Tính thời gian nhạc còn lại
            // (Tổng thời lượng - Thời gian đã phát)
            float remainingTime = musicSource.clip.length - musicSource.time;

  
            if (remainingTime > 0)
            {
            
                float timer = 0f;
                while (timer < remainingTime)
                {
                    if (hasClickedButton) yield break; // Nếu đã bấm nút -> Dừng chờ, thoát Coroutine

                    timer += Time.deltaTime;
                    yield return null;
                }
            }
        }
        else
        {
            // Nếu không có nhạc, đợi tầm 5 giây rồi tự out
            float timer = 0;
            while (timer < 5f)
            {
                if (hasClickedButton) yield break;
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // Nếu chạy hết vòng lặp mà chưa bấm nút -> Tự động về Menu
        if (!hasClickedButton)
        {
            ReturnToMenu();
        }
    }

    // Hàm sự kiện khi bấm nút
    void OnReturnClick()
    {
        hasClickedButton = true; 
        ReturnToMenu();
    }

    void ReturnToMenu()
    {
        StartCoroutine(ReturnToMenuRoutine());
    }
    IEnumerator ReturnToMenuRoutine()
    {
        // 1. Tìm thằng Fader trong Essentials
        GameObject bgmManager = GameObject.Find("BGMManager");
        if (bgmManager != null)
        {
            MusicFade fader = bgmManager.GetComponentInChildren<MusicFade>();

            if (fader != null)
            {
                // Gọi Fade Out trong 1.5 giây và ĐỢI nó chạy xong
                yield return StartCoroutine(fader.FadeOutMusic(1.5f));
            }
        }

        // 2. Nhạc tắt xong rồi thì mới Load Scene
        SceneManager.LoadScene("MainMenu");
    }
    // --- CÁC HÀM FADE TIỆN ÍCH ---

    // Fade cho TextMeshPro
    IEnumerator FadeText(TextMeshProUGUI text, float start, float end, float duration)
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

    // Fade cho Canvas Group (Title, Credits, Button)
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
}