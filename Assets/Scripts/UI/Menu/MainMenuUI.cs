using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OpenOptionsParams()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OpenOptionsPanel();
        }
        else
        {
            Debug.LogError("Không tìm thấy PauseManager! Kiểm tra lại Essentials.");
        }
    }

    
   
}