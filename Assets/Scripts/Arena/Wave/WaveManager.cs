using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Dùng cho UI

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    [Header("Wave Configuration")]
    public ArenaConfig defaultArenaConfig;
    private List<WaveConfig> waves; 
    public int currentWaveIndex = 0;    

    [Header("Portal Settings")]
    public GameObject portalPrefab;
    public Transform centerMap;

    [Header("Arena Bounds")]
    public Vector2 minBounds = new Vector2(-75, -45);
    public Vector2 maxBounds = new Vector2(75, 45);
    public Transform playerTransform; // Để tránh spawn trúng đầu player

    [Header("Spawn Settings")]
    public float minSpawnDistance = 10f; // Khoảng cách tối thiểu với Player (để không spawn trước mặt)
    public float checkRadius = 3f;     // Bán kính kiểm tra va chạm (to bằng con quái)
    public LayerMask obstacleLayer;      // Layer của Tường, Cột, Chướng ngại vật
    public int maxSpawnAttempts = 30;    // Số lần thử tìm vị trí tối đa (tránh treo máy)
    public GameObject spawnEffect;
    
    [Header("UI References")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;
    public GameObject waveCompletedPanel; // Hiện khi thắng hết các wave

    // State
    private int totalEnemiesInWave;
    private int enemiesAlive;
    private bool isSpawning = false;
    private bool isWaveFinished = false;

    private void Start()
    {
        if (EssentialsManager.Instance == null)
        {
            Debug.LogWarning("Chạy WaveManager độc lập (Test Mode)");
            // Khi test thì cứ cho là New Game (false)
            InitializeWave(false);
        }
    }

    private void Update()
    {
        // Update UI số lượng quái còn lại
        UpdateEnemiesLeftUI();
        // Kiểm tra điều kiện qua màn: Không còn spawn VÀ hết quái
        if (!isSpawning && enemiesAlive <= 0 && !isWaveFinished)
        {
            isWaveFinished = true;
            StartCoroutine(WaveCompletedSequence());
        }
    }
    public void InitializeWave(bool useSaveData)
    {
        if (BossHealthBar.Instance != null)
        {
            BossHealthBar.Instance.Hide();
        }
        // 1. Dọn dẹp sạch sẽ trước khi bắt đầu
        StopAllCoroutines(); // [QUAN TRỌNG] Giết chết mọi luồng spawn cũ đang chạy dở
        isSpawning = false;
        isWaveFinished = false;

        // 2. Load Config Arena
        if (GameFlowManager.Instance != null)
        {
            ArenaConfig config = GameFlowManager.Instance.GetCurrentArenaConfig();
            if (config != null) this.waves = config.waves;
        }

        // Fallback nếu không có config
        if (this.waves == null || this.waves.Count == 0)
        {
            if (defaultArenaConfig != null) this.waves = defaultArenaConfig.waves;
            else return;
        }

        // 3. Quyết định Wave Index
        if (useSaveData && PlayerPrefs.GetInt("Run_HasSave", 0) == 1)
        {
            // Chỉ load index từ file save khi ĐƯỢC PHÉP (Continue Game)
            currentWaveIndex = PlayerPrefs.GetInt("Run_WaveIndex", 0);
            Debug.Log($"[WaveManager] Loading Save: Wave {currentWaveIndex + 1}");
        }
        else
        {
            // Các trường hợp khác (New Game, Qua Arena mới) -> Reset về 0
            currentWaveIndex = 0;
            Debug.Log($"[WaveManager] New Start: Wave 1");
        }

        // 4. Bắt đầu
        StartCoroutine(StartWave(currentWaveIndex));
    }

    private IEnumerator StartWave(int index)
    {
        if (waves == null || waves.Count == 0) yield break;
        if (GameFlowManager.Instance != null && AudioManager.Instance != null)
        {
            Debug.Log("--- BẮT ĐẦU GỌI NHẠC ---"); 
            ArenaConfig currentArena = GameFlowManager.Instance.GetCurrentArenaConfig();

            if (currentArena != null)
            {
                if (index == currentArena.bossWaveIndex)
                {
                    Debug.Log("--> ĐÂY LÀ WAVE BOSS! Gọi nhạc Boss."); 
                    if (currentArena.bossWaveBGM != null)
                        AudioManager.Instance.PlayBossMusic(currentArena.bossWaveBGM);
                    else
                        Debug.LogError("--> LỖI: Chưa gắn nhạc Boss trong Config!"); // <--- Bắt lỗi
                }
                else
                {
                    Debug.Log("--> WAVE THƯỜNG. Gọi Playlist."); 
                    if (currentArena.normalWaveBGM != null && currentArena.normalWaveBGM.Count > 0)
                        AudioManager.Instance.PlayPlaylist(currentArena.normalWaveBGM);
                    else
                        Debug.LogError("--> LỖI: Playlist nhạc thường đang TRỐNG!"); // <--- Bắt lỗi
                }
            }
            else Debug.LogError("--> LỖI: Không tìm thấy Arena Config!");
        }
        else Debug.LogError("--> LỖI: GameFlow hoặc AudioManager bị NULL!");
        // Kiểm tra Index hợp lệ
        if (index >= waves.Count)
        {
            Debug.Log("Arena Cleared! Spawning Portal.");
            SpawnPortal();
            if (waveCompletedPanel != null) waveCompletedPanel.SetActive(true);
            yield break;
        }

        // [FIX] Save ngay đầu wave để đảm bảo dữ liệu đúng với wave hiện tại
        if (EssentialsManager.Instance != null && !EssentialsManager.Instance.isLoadingSavedGame)
        {
            EssentialsManager.Instance.SaveRunData();
        }

        isWaveFinished = false;
        isSpawning = true;

        WaveConfig currentWave = waves[index];
        totalEnemiesInWave = currentWave.GetTotalEnemyCount();
        enemiesAlive = totalEnemiesInWave; // Set lại số lượng chuẩn

        if (waveText != null) waveText.text = $"WAVE {index + 1}";
        UpdateEnemiesLeftUI(); // Cập nhật UI ngay lập tức

        // --- Spawn Loop ---
        foreach (var group in currentWave.enemyGroups)
        {
            for (int i = 0; i < group.amount; i++)
            {
                // Kiểm tra an toàn: Nếu wave đã bị hủy hoặc game over thì dừng spawn ngay
                if (isWaveFinished) yield break;

                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(currentWave.timeBetweenSpawns);
            }
        }

        isSpawning = false;
        
    }
    private void UpdateEnemiesLeftUI()
    {
        if (enemiesLeftText != null)
        {
            enemiesLeftText.text = $"Enemies: {enemiesAlive}";
        }

    }

   

    private void SpawnEnemy(GameObject prefab)
    {
        Vector2 spawnPos = Vector2.zero;
        bool validPositionFound = false;

        // Thử tìm vị trí hợp lệ trong maxSpawnAttempts lần
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 potentialPos = GetRandomPositionInArena();

            // 1. KIỂM TRA KHOẢNG CÁCH VỚI PLAYER
            if (playerTransform != null)
            {
                float distance = Vector2.Distance(potentialPos, playerTransform.position);
                if (distance < minSpawnDistance)
                {
                    continue; // Quá gần, bỏ qua, tìm điểm khác
                }
            }

            // 2. KIỂM TRA VẬT CẢN (TƯỜNG/CỘT)
            // Quét một vòng tròn xem có trúng Layer Tường không
            Collider2D hit = Physics2D.OverlapCircle(potentialPos, checkRadius, obstacleLayer);

            if (hit == null)
            {
                // Không va vào tường -> Vị trí ngon!
                spawnPos = potentialPos;
                validPositionFound = true;
                break; // Thoát vòng lặp
            }
        }

        if (validPositionFound)
        {
            Instantiate(spawnEffect, spawnPos, Quaternion.identity);
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            // Setup Enemy logic cũ
            EnemyHealth healthScript = enemy.GetComponent<EnemyHealth>();
            if (healthScript != null)
            {
                healthScript.OnDeath -= OnEnemyKilled;
                healthScript.OnDeath += OnEnemyKilled;
            }
        }
        else
        {
            Debug.LogWarning("Không tìm được chỗ spawn hợp lệ sau " + maxSpawnAttempts + " lần thử!");
            if (enemiesAlive > 0)
            {
                enemiesAlive--;
                UpdateEnemiesLeftUI();
            }
        }
    }

    // Hàm random thuần túy trong vùng bounds (sửa lại logic spawn rìa cũ thành spawn toàn map)
    private Vector2 GetRandomPositionInArena()
    {
        float x = Random.Range(minBounds.x, maxBounds.x);
        float y = Random.Range(minBounds.y, maxBounds.y);
        return new Vector2(x, y);
    }

    // Vẽ Gizmos để sếp dễ chỉnh minBounds/maxBounds trong Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Vẽ khung bao quanh vùng spawn
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1);
        Gizmos.DrawWireCube(center, size);

        // Vẽ vòng tròn test thử bán kính quái
        Gizmos.color = Color.red;
        if (playerTransform != null)
            Gizmos.DrawWireSphere(playerTransform.position, minSpawnDistance);
    }
    private void OnEnemyKilled()
    {
        if (enemiesAlive > 0)
        {
            enemiesAlive--;
            UpdateEnemiesLeftUI();
          
        }
        else
        {
            // Debug cảnh báo khi có sự kiện chết dư thừa được gọi
            Debug.LogWarning("WaveManager: OnEnemyKilled bị gọi dư thừa! EnemiesAlive đã là 0.");
        }

       
    }

    private IEnumerator WaveCompletedSequence()
    {
        Debug.Log("Wave Completed!");

        // Chờ 2 giây nghỉ ngơi
        yield return new WaitForSeconds(3f);

        // Chuyển sang wave tiếp theo
        currentWaveIndex++;

        if (currentWaveIndex < waves.Count)
        {
            StartCoroutine(StartWave(currentWaveIndex));
        }
        else
        {
            Debug.Log("ARENA CLEARED! OPENING PORTAL...");
            SpawnPortal();
            if (waveCompletedPanel != null) waveCompletedPanel.SetActive(true);
        }
    }
    private void SpawnPortal()
    {
        if (portalPrefab != null)
        {
            Instantiate(portalPrefab, centerMap.position, Quaternion.identity);

          
        }
    }
}