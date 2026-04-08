using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    // Hàm này sẽ gán vào nút Options
    public void OpenOptionsParams()
    {
        // Gọi thằng PauseManager đang sống (Instance A)
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OpenOptionsPanel();
        }
        else
        {
            Debug.LogError("Không tìm thấy PauseManager! Kiểm tra lại Essentials.");
        }
    }

    
    //public void NewGameParams()
    //{
    //    // Gọi MetaProgressionManager reset data...
    //    // Load Scene...
    //}
}