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

            // Báo cho GameFlow biết là xong Tutorial rồi, vào màn 1 đi
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.currentArenaIndex = 0; // Hoặc set logic phù hợp
                // Gọi hàm LoadNextArena() của GameFlowManager thay vì LoadScene trực tiếp
              
            }
            else
            {
                SceneManager.LoadScene("Arena1-Temple"); // Fallback
            }
        }
    }
}