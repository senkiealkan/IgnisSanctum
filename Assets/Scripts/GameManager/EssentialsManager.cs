using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;
public class EssentialsManager : MonoBehaviour
{
    public static EssentialsManager Instance;

    [Header("References")]
    public GameObject player;
    public GameObject gameplayUI;
    public GameObject mainCamera;

    [Header("Settings")]
    public string menuSceneName = "MainMenu";

    // Cờ lệnh (Flags)
    public bool isStartingNewRun = false;
    public bool isLoadingSavedGame = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // --- LOGIC ĐIỀU PHỐI CHÍNH ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CinemachineCamera vcam = FindAnyObjectByType<CinemachineCamera>();
        if (scene.name == menuSceneName || scene.name == "EndingScene")
        {
            // --- Ở MENU ---
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic();
            }
            if (player != null) {
                player.transform.position = Vector3.zero; 
                player.SetActive(false);
                if (vcam != null)
                {
                    // Lệnh này bắt VCam cập nhật vị trí ngay lập tức theo target
                    vcam.OnTargetObjectWarped(player.transform, Vector3.zero - vcam.transform.position);
                    vcam.PreviousStateIsValid = false; // Reset bộ đệm của Cinemachine
                }
            }
            if (gameplayUI != null) gameplayUI.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Transform arenaUI = transform.Find("ArenaUI"); 
            if (arenaUI != null) arenaUI.GetComponent<Canvas>().enabled = true;
            // --- Ở GAME ---
            if (player != null)
            {
                player.SetActive(true);
                player.transform.position = Vector3.zero;
            }
            if (gameplayUI != null) gameplayUI.SetActive(true);
            if (mainCamera != null) mainCamera.SetActive(true);
            WaveManager waveMgr = FindAnyObjectByType<WaveManager>();

            if (isLoadingSavedGame)
            {
                // Case 1: Load Save -> Dùng dữ liệu Save
                StartCoroutine(LoadSavedGameProcess());
                isLoadingSavedGame = false;
            }
            else if (isStartingNewRun)
            {
                // Case 2: New Game -> KHÔNG dùng Save (Reset về 0)
                StartCoroutine(ResetPlayerState());
                isStartingNewRun = false;
            }
            else
            {
                // Case 3: Chuyển Arena (Đi qua cổng) -> KHÔNG dùng Save cũ (Reset về Wave 0 của Arena mới)
                if (waveMgr != null)
                {
                    StartCoroutine(WaitAndStartWave(waveMgr, false)); // <--- Truyền FALSE
                }
            }
        }
    }
    // Thêm Coroutine phụ trợ
    private IEnumerator WaitAndStartWave(WaveManager waveMgr, bool useSave)
    {
        yield return null;
        if (waveMgr != null) waveMgr.InitializeWave(useSave);
    }

    // --- LOGIC RESET CHO NEW GAME ---
    public IEnumerator ResetPlayerState()
    {
        yield return null; // Chờ 1 frame cho UI ổn định
        if (BossHealthBar.Instance != null)
        {
            BossHealthBar.Instance.Hide();
        }
        Debug.Log("--- BẮT ĐẦU RESET PLAYER ---");
        if (player != null)
        {
            // 1. Reset Stats In-Run (Card)
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null) stats.ResetInRunStats();

            // 2. Hồi sinh & Đầy máu
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.Revive();
                health.transform.position = Vector3.zero;
            }
           // player.transform.position = Vector3.zero ;
            // 3. Reset Mana & Inventory
            PlayerMana mana = player.GetComponent<PlayerMana>();
            if (mana != null) mana.ResetMana();

            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null) inventory.ResetInventory();

            XPManager xpMgr = player.GetComponent<XPManager>();
            if (xpMgr != null)
            {
                xpMgr.ResetXP(); // Gọi hàm ResetXP về lv 1
            }
        }
       
        // Xóa file save cũ để tránh xung đột
        ClearRunData();
        // Sau khi reset xong xuôi, mới bảo WaveManager chạy
        WaveManager waveMgr = FindAnyObjectByType<WaveManager>();
        if (waveMgr != null) waveMgr.InitializeWave(false);
    }

    // --- LOGIC LOAD GAME (SNAPSHOT) ---
    private IEnumerator LoadSavedGameProcess()
    {
        yield return null;
        LoadRunData(); // Gọi hàm load chi tiết
        Debug.Log("--- LOAD GAME THÀNH CÔNG ---");
        // Load xong hết chỉ số rồi mới cho quái ra
        WaveManager waveMgr = FindAnyObjectByType<WaveManager>();
        if (waveMgr != null) waveMgr.InitializeWave(true);
    }


    //Save Snapshot
    public void SaveRunData()
    {

        if (player == null) return;
        PlayerPrefs.SetString("Run_SceneName", SceneManager.GetActiveScene().name);

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null) PlayerPrefs.SetFloat("Run_CurrentHP", health.currentHealth);

        PlayerMana mana = player.GetComponent<PlayerMana>();
        if (mana != null) PlayerPrefs.SetFloat("Run_CurrentMana", mana.currentMana);

        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv != null)
        {
            PlayerPrefs.SetInt("Run_HPPotions", inv.hpPotions);
            PlayerPrefs.SetInt("Run_ManaPotions", inv.manaPotions);
        }

        WaveManager waveMgr = FindAnyObjectByType<WaveManager>();
        if (waveMgr != null)
        {
            PlayerPrefs.SetInt("Run_WaveIndex", waveMgr.currentWaveIndex);
        }

        // Lưu Stats In-Run
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null) stats.SaveRunStats();

        XPManager xpMgr = player.GetComponent<XPManager>();
        if (xpMgr != null)
        {
            PlayerPrefs.SetInt("Run_XP_Level", xpMgr.currentLevel);
            PlayerPrefs.SetFloat("Run_XP_Current", xpMgr.currentXP);
            PlayerPrefs.SetFloat("Run_XP_Required", xpMgr.requiredXP);
        }
        // Lưu thêm số Gem đang có trong túi tạm
        if (MetaProgressionManager.Instance != null)
        {
            PlayerPrefs.SetInt("Run_TempStatGems", MetaProgressionManager.Instance.currentRunStatGems);
            PlayerPrefs.SetInt("Run_TempFireGems", MetaProgressionManager.Instance.currentRunFireGems);
        }
        PlayerPrefs.SetInt("Run_HasSave", 1);
        PlayerPrefs.Save();
        Debug.Log("SNAPSHOT SAVED! (Đã lưu tại đầu Wave)");
    }

    public void LoadRunData()
    {
        if (PlayerPrefs.GetInt("Run_HasSave", 0) == 0) return;

        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null) stats.LoadRunStats(); // Load Card Stats trước

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                float savedHP = PlayerPrefs.GetFloat("Run_CurrentHP", health.maxHealth);
                health.currentHealth = savedHP;
                if (health.healthBar != null) health.healthBar.SetHealth(savedHP);
            }

            PlayerMana mana = player.GetComponent<PlayerMana>();
            if (mana != null)
            {
                float savedMana = PlayerPrefs.GetFloat("Run_CurrentMana");
                mana.currentMana = savedMana;
                if (mana.manaBar != null) mana.manaBar.SetMana(savedMana);
            }

            PlayerInventory inv = player.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.hpPotions = PlayerPrefs.GetInt("Run_HPPotions", 0);
                inv.manaPotions = PlayerPrefs.GetInt("Run_ManaPotions", 0);
                inv.UpdateUI();
            }
            XPManager xpMgr = player.GetComponent<XPManager>();
            if (xpMgr != null)
            {
                int lvl = PlayerPrefs.GetInt("Run_XP_Level", 1);
                float curXP = PlayerPrefs.GetFloat("Run_XP_Current", 0f);
                float reqXP = PlayerPrefs.GetFloat("Run_XP_Required", 1000f);
                xpMgr.LoadXP(lvl, curXP, reqXP);
            }
        }
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.currentRunStatGems = PlayerPrefs.GetInt("Run_TempStatGems", 0);
            MetaProgressionManager.Instance.currentRunFireGems = PlayerPrefs.GetInt("Run_TempFireGems", 0);
        }

    }

    public void ClearRunData()
    {
        PlayerPrefs.DeleteKey("Run_HasSave");
        // Xóa các key khác nếu cần...
        PlayerPrefs.Save();
    }
}