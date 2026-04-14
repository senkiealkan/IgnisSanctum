using TMPro; // Nếu bạn sử dụng TextMeshPro
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;
    public GameObject cardSelectMenuPanel;
    // Biến kiểm tra trạng thái game
    private bool isPaused = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        pauseMenuPanel.SetActive(false);
    }
    private void Start()
    {
        pauseMenuPanel.SetActive(false);
    }
    public InputAction pauseAction;
    private void OnEnable()
    {

        // 1. Kích hoạt Input Action
        pauseAction.Enable();
        // 2. Đăng ký hàm OnDash vào sự kiện performed 
        pauseAction.performed += TogglePause;
    }
    private void OnDisable()
    {
        pauseAction.performed -= TogglePause;
        pauseAction.Disable();
    }
    private void TogglePause(InputAction.CallbackContext context)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "MainMenu" || currentScene == "IntroScene" || currentScene == "EndingScene")
        {
            return;
        }
        if (PlayerHealth.Instance != null)
        {
            if (PlayerHealth.Instance.currentHealth <= 0)
            {
                return;
            }
        }
        if (cardSelectMenuPanel != null && cardSelectMenuPanel.activeSelf)
        {
            return;
        }
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    // --- Các phương thức Game State ---
    public void GiveUpRun()
    {
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        // 1. Xóa Gem tạm 
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.DiscardRunLoot();
        }

        // 2. Xóa file Save Snapshot
        if (EssentialsManager.Instance != null)
        {
            EssentialsManager.Instance.ClearRunData();
        }
        if (PlayerHealth.Instance != null)
        {


            // 3. Sau khi chỉ số Max HP đã tăng, mới gọi Revive để đầy cây máu mới
            PlayerHealth.Instance.Revive();
            PlayerMana pMana = PlayerHealth.Instance.GetComponent<PlayerMana>();
            if (pMana != null)
            {
                pMana.ResetMana();
            }
            // 4. Reset Inventory 
            PlayerInventory inventory = PlayerHealth.Instance.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.ResetInventory();
            }
        }
        // 5. Về Main Menu
        SceneManager.LoadScene("MainMenu");
    }


    // --- Phương thức Dừng Game ---
    public void PauseGame()
    {
       
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;

        isPaused = true;
    }

    // --- Phương thức Tiếp tục Game ---
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    // --- Phương thức Quay lại Menu Chính ---
    public void GoToMainMenu()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");         
    }


    // --- Phương thức Thoát Game  ---
    public void QuitGame()
    {
        Debug.Log("Game Quit!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    public void OpenOptionsPanel()
    {
        optionsPanel.SetActive(true);
    }


}