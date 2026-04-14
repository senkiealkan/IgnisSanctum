using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialExit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.currentArenaIndex = 0; 
              
            }
            else
            {
                SceneManager.LoadScene("Arena1-Temple"); // Fallback
            }
        }
    }
}