using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;          
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    public GameObject optionsPanel;
    public Button continueButton;
    public Canvas mainMenuCanvas;
    void Start()
    {
        Time.timeScale = 1f;
        GameObject essentials = GameObject.Find("Essentials");

        if (essentials != null)
        {
            Transform cameraTransform = essentials.transform.Find("Main Camera");

            if (cameraTransform != null)
            {
                Camera myCam = cameraTransform.GetComponent<Camera>();

         
                if (mainMenuCanvas != null)
                {
                    mainMenuCanvas.worldCamera = myCam;
                }

                Canvas currentCanvas = GetComponent<Canvas>();
                if (currentCanvas != null)
                {
                    currentCanvas.worldCamera = myCam;
                }

                GameObject optionsCanvasObj = GameObject.Find("OptionsCanvas"); 
                if (optionsCanvasObj != null)
                {
                    Canvas optCanvas = optionsCanvasObj.GetComponent<Canvas>();
                    if (optCanvas != null) optCanvas.worldCamera = myCam;
                }
            }
        }
        if (PlayerPrefs.GetInt("HasPlayedBefore", 0) == 0)
        {
            if (continueButton != null)
            {
                // 1. Tắt khả năng bấm (Sẽ không gọi hàm OnClick)
                continueButton.interactable = false;

                // 2. Tắt EventTrigger (Để không phát âm thanh/hiệu ứng khi Hover chuột)
                EventTrigger trigger = continueButton.GetComponent<EventTrigger>();
                if (trigger != null)
                {
                    trigger.enabled = false;
                }

                // 3. Làm mờ nút đi cho người chơi hiểu là bị khóa
                CanvasGroup cg = continueButton.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0.5f; // Mờ đi 50%
            }
        }
        if (essentials != null)
        {
            Transform arenaUI = essentials.transform.Find("ArenaUI");
            if (arenaUI != null)
            {
                // Dùng Canvas để tắt hiển thị mà không tắt GameObject (tránh lỗi Coroutine)
                Canvas cv = arenaUI.GetComponent<Canvas>();
                if (cv != null) cv.enabled = false;
            }

            // Tắt luôn cả Player
            Transform player = essentials.transform.Find("Cael");
            if (player != null) player.gameObject.SetActive(false);
        }
    }
    public void NewGame()
    {
       StartCoroutine(NewGameRoutine());
    }
    IEnumerator NewGameRoutine()
    {
        // 1. Tìm và Fade Out nhạc
        GameObject bgmManager = GameObject.Find("BGMManager");
        if (bgmManager != null)
        {
            MusicFade fader = bgmManager.GetComponentInChildren<MusicFade>();
            if (fader != null)
            {
                yield return StartCoroutine(fader.FadeOutMusic(1.0f));
            }
        }
        PlayerPrefs.SetInt("HasPlayedBefore", 1);
        PlayerPrefs.Save();
        // A. Xóa dữ liệu Meta (Gem, Upgrade)
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.ResetData();
        }

        // B. Xóa trạng thái đã chơi Tutorial (để game coi như mới cài lại)
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        PlayerPrefs.Save();

        // C. Bắt đầu vào intro
        StartRunLogic("IntroScene");
    } 

    // --- 2. NÚT CONTINUE (CHƠI TIẾP HOẶC RUN MỚI GIỮ ĐỒ) ---
    public void ContinueGame()
    {
       StartCoroutine(ContinueGameRoutine());
    }
    IEnumerator ContinueGameRoutine()
    {
        // 1. Tìm và Fade Out nhạc
        GameObject bgmManager = GameObject.Find("BGMManager");
        if (bgmManager != null)
        {
            MusicFade fader = bgmManager.GetComponentInChildren<MusicFade>();
            if (fader != null)
            {
                yield return StartCoroutine(fader.FadeOutMusic(1.0f));
            }
        }
        // Kiểm tra xem có đang chơi dở không (File save snapshot)
        if (PlayerPrefs.GetInt("Run_HasSave", 0) == 1)
        {
            // === TRƯỜNG HỢP 1: CÓ SAVE DỞ ===
            string savedScene = PlayerPrefs.GetString("Run_SceneName");

            if (EssentialsManager.Instance != null)
            {
                EssentialsManager.Instance.isLoadingSavedGame = true;
            }

            SceneManager.LoadScene(savedScene);
        }
        else
        {
            // === TRƯỜNG HỢP 2: KHÔNG CÓ SAVE (Do vừa GiveUp hoặc vừa Win) ===
            Debug.Log("Không có file save dở dang -> Bắt đầu Run mới (Giữ nguyên Meta)");


            // Kiểm tra xem đã học xong Tutorial chưa để chọn map
            if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
            {
                StartRunLogic("Arena1-Temple");
            }
            else
            {
                StartRunLogic("Arena0-Tutorial"); //Tutorial
            }
        }
    }

    // --- HÀM PHỤ TRỢ  ---
    private void StartRunLogic(string sceneName)
    {
        // 1. Reset tiến độ đi màn 
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ResetProgression();
        }

        // 2. Reset trạng thái nhân vật (Máu đầy, Mana đầy, Level 1)
        if (EssentialsManager.Instance != null)
        {
            EssentialsManager.Instance.isStartingNewRun = true;
            StartCoroutine(EssentialsManager.Instance.ResetPlayerState());
        }

        // 3. Vào game
        SceneManager.LoadScene(sceneName);
    }

    public void OpenOptionsPanel()
    {
        optionsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameRoutine());
    }

    IEnumerator QuitGameRoutine()
    {
        // 1. Tìm và Fade Out nhạc
        GameObject bgmManager = GameObject.Find("BGMManager");
        if (bgmManager != null)
        {
            MusicFade fader = bgmManager.GetComponentInChildren<MusicFade>();
            if (fader != null)
            {
                yield return StartCoroutine(fader.FadeOutMusic(1.0f));
            }
        }

        // 2. Thoát game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}