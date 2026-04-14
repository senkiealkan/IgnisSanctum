using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel; 
    public GameObject panelUpgrade;
    public void TriggerGameOver()
    {
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.CommitRunLoot();
        }
        // 1. Hiện UI
        gameOverPanel.SetActive(true);

        // 2. Cập nhật thông tin Shop ngay khi mở (để hiển thị số gem mới nhất)
        MetaUpgradeShop shop = gameOverPanel.GetComponentInChildren<MetaUpgradeShop>();
        if (shop != null)
        {
            shop.UpdateUI();
        }

        // 3. Dừng thời gian (để không bị quái đánh tiếp)
        Time.timeScale = 0f;
    }

   
    public void Rebirth()
    {
        // 1. Lưu Gem 
        if (MetaProgressionManager.Instance != null) MetaProgressionManager.Instance.SaveData();

        // 2. Set cờ báo hiệu đây là một lượt chơi mới
        if (EssentialsManager.Instance != null)
        {
            EssentialsManager.Instance.ClearRunData(); 
            EssentialsManager.Instance.isStartingNewRun = true; 
        }

        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);

        // 3. Load lại Scene 
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
            SceneManager.LoadScene("Arena1-Temple");
        else
            SceneManager.LoadScene("Arena0-Tutorial");
    }

    public void ReturnMainMenu()
    {
        // Lưu Gem lại ngay lập tức trước khi tái sinh 
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.SaveData();
        }

        Time.timeScale = 1f;

        if (PlayerHealth.Instance != null)
        {
            PlayerStats stats = PlayerHealth.Instance.GetComponent<PlayerStats>();
            if (stats != null) stats.RecalculateStats();

            PlayerHealth.Instance.Revive();
            PlayerMana pMana = PlayerHealth.Instance.GetComponent<PlayerMana>();
            if (pMana != null) pMana.ResetMana();

            PlayerInventory inventory = PlayerHealth.Instance.GetComponent<PlayerInventory>();
            if (inventory != null) inventory.ResetInventory();

            PlayerHealth.Instance.transform.position = Vector3.zero;
        }

       
        if (EssentialsManager.Instance != null)
        {
            EssentialsManager.Instance.ClearRunData();
        }

        gameOverPanel.SetActive(false);

        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void TogglePanel()
    {
        if (panelUpgrade != null)
        {
            // Nếu đang bật thì tắt, nếu đang tắt thì bật
            panelUpgrade.SetActive(!panelUpgrade.activeSelf);
        }
    }
}