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
        // Reset vị trí player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerObject.transform.position = Vector3.zero;
    }

    // Logic chuyển màn
    public void GoToNextArena()
    {
 
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

        if (foundIndex != -1)
        {
            int nextIndex = foundIndex + 1;

            if (nextIndex < allArenas.Count)
            {
                currentArenaIndex = nextIndex; 
                LoadArena(nextIndex);
            }
            else
            {
                // Hết màn trong List -> Win Game
                if (EssentialsManager.Instance != null) EssentialsManager.Instance.ClearRunData();
                SceneManager.LoadScene("EndingScene");
            }
        }
        else
        {
            StartNewRun();
        }
    }

    public void ResetProgression()
    {
        currentArenaIndex = 0;
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