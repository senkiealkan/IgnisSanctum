using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [Header("Game Config")]
    public List<ArenaConfig> allArenas;

    [Header("Runtime State")]
    public int currentArenaIndex = 0;

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

    public void StartNewRun()
    {
        currentArenaIndex = 0;
        LoadArena(currentArenaIndex);
        // Reset vị trí player (EssentialsManager sẽ lo việc này ở OnSceneLoaded, nhưng giữ đây cũng ko sao)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerObject.transform.position = Vector3.zero;
    }

    // [FIX QUAN TRỌNG] Logic chuyển màn thông minh
    public void GoToNextArena()
    {
        // 1. Thay vì ++ biến đếm cũ, ta tìm xem mình đang đứng ở đâu trong List
        string currentSceneName = SceneManager.GetActiveScene().name;
        int foundIndex = -1;

        for (int i = 0; i < allArenas.Count; i++)
        {
            if (allArenas[i].sceneName == currentSceneName)
            {
                foundIndex = i;
                break;
            }
        }

        // 2. Tính toán màn tiếp theo
        if (foundIndex != -1)
        {
            // Nếu tìm thấy màn hiện tại, màn tiếp theo là +1
            int nextIndex = foundIndex + 1;

            if (nextIndex < allArenas.Count)
            {
                // Vẫn còn màn để chơi
                currentArenaIndex = nextIndex; // Cập nhật lại biến đếm cho đồng bộ
                LoadArena(nextIndex);
            }
            else
            {
                // Hết màn trong List -> Win Game
                Debug.Log("YOU BEAT THE GAME! (Về màn hình chính hoặc New Game+)");
                // Reset về menu
                if (EssentialsManager.Instance != null) EssentialsManager.Instance.ClearRunData();
                SceneManager.LoadScene("EndingScene");
            }
        }
        else
        {
            // Trường hợp hy hữu: Đang chơi ở một Scene không có trong danh sách ArenaConfig (Ví dụ test editor)
            Debug.LogWarning("Cảnh hiện tại không nằm trong danh sách Arena Config! Tự động load màn đầu tiên.");
            StartNewRun();
        }
    }

    public void ResetProgression()
    {
        currentArenaIndex = 0;
        Debug.Log("Game Progression Reset!");
    }

    private void LoadArena(int index)
    {
        if (index >= 0 && index < allArenas.Count)
        {
            string sceneToLoad = allArenas[index].sceneName;
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("LoadArena: Index không hợp lệ! " + index);
        }
    }

    public ArenaConfig GetCurrentArenaConfig()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        foreach (var config in allArenas)
        {
            if (config.sceneName == currentSceneName)
            {
                return config;
            }
        }

        // Fallback an toàn
        if (currentArenaIndex < allArenas.Count)
            return allArenas[currentArenaIndex];

        return null;
    }
}