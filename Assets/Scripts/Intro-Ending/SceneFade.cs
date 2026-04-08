using UnityEngine;
using UnityEngine.UI; // Cần cái này
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public float fadeDuration = 1.5f;
    private CanvasGroup cg;

    void Awake() // Dùng Awake để chạy trước cả Start
    {
        // 1. Tự động thêm Canvas nếu chưa có để chiếm quyền ưu tiên
        Canvas myCanvas = GetComponent<Canvas>();
        if (myCanvas == null) myCanvas = gameObject.AddComponent<Canvas>();

        // 2. Set quyền ưu tiên vẽ đè lên TẤT CẢ mọi thứ
        // ArenaUI của bạn thường có Order thấp (0 hoặc 10), nên 999 là trùm cuối.
        myCanvas.overrideSorting = true;
        myCanvas.sortingOrder = 999;
    }

    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        // Đảm bảo bắt đầu là che kín màn hình
        cg.alpha = 1f;
        RestoreGameplayVisuals();
        StartCoroutine(FadeOut());
    }
    void RestoreGameplayVisuals()
    {
        GameObject essentials = GameObject.Find("Essentials");
        if (essentials != null)
        {
            // 1. Bật Player (Để hiện sprite)
            Transform player = essentials.transform.Find("Cael");
            if (player != null) player.gameObject.SetActive(true);

            // 2. Bật UI (ArenaUI) nhưng nó sẽ nằm DƯỚI cái Fader này
            // Vì Fader Order = 999, còn ArenaUI Order = 1
            Transform arenaUI = essentials.transform.Find("ArenaUI");
            if (arenaUI != null)
            {
                CanvasGroup uiCG = arenaUI.GetComponent<CanvasGroup>();
                if (uiCG != null)
                {
                    uiCG.alpha = 1f;           // Hiện hình
                    uiCG.blocksRaycasts = true; // Cho phép bấm
                    uiCG.interactable = true;
                }
            }
        }
    }
    IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        gameObject.SetActive(false);
    }
}