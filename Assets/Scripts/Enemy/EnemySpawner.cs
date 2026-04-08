using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 5f;
    private float nextSpawnTime = 0f;
    private Transform playerTransform;

    // PHẠM VI TRÒ CHƠI BÊN TRONG (Khu vực CẤM spawn)
    public float gameMinX = -91f;
    public float gameMaxX = 91f;
    public float gameMinY = -50f;
    public float gameMaxY = 50f;

    // PHẠM VI LỚN HƠN (Khu vực ĐƯỢC spawn) - Xác định phạm vi tối đa quái có thể xuất hiện
    public float outerBorder = 30f; // Khoảng cách tối đa từ viền game ra ngoài.

    // Ví dụ: Spawn từ X=-121 đến X=121 và Y=-80 đến Y=80
    private float spawnMinX => gameMinX - outerBorder;
    private float spawnMaxX => gameMaxX + outerBorder;
    private float spawnMinY => gameMinY - outerBorder;
    private float spawnMaxY => gameMaxY + outerBorder;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found by Spawner!");
        }
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || playerTransform == null) return;

        Vector3 spawnPosition;
        float randomX, randomY;

        // B1: Chọn ngẫu nhiên spawn ở 4 cạnh: Trên, Dưới, Trái, hoặc Phải.
        // Chọn một số nguyên ngẫu nhiên từ 0 đến 3 (4 trường hợp)
        int side = Random.Range(0, 4);

        if (side == 0) // Spawn Cạnh TRÊN (Y cố định, X ngẫu nhiên)
        {
            // Y nằm ngoài khu vực game, ví dụ: Y = Random.Range(50, 50 + outerBorder)
            randomY = Random.Range(gameMaxY, spawnMaxY);
            // X nằm trong phạm vi lớn hơn
            randomX = Random.Range(spawnMinX, spawnMaxX);
        }
        else if (side == 1) // Spawn Cạnh DƯỚI (Y cố định, X ngẫu nhiên)
        {
            // Y nằm ngoài khu vực game, ví dụ: Y = Random.Range(-50 - outerBorder, -50)
            randomY = Random.Range(spawnMinY, gameMinY);
            // X nằm trong phạm vi lớn hơn
            randomX = Random.Range(spawnMinX, spawnMaxX);
        }
        else if (side == 2) // Spawn Cạnh TRÁI (X cố định, Y ngẫu nhiên)
        {
            // X nằm ngoài khu vực game, ví dụ: X = Random.Range(-91 - outerBorder, -91)
            randomX = Random.Range(spawnMinX, gameMinX);
            // Y nằm trong phạm vi lớn hơn
            randomY = Random.Range(spawnMinY, spawnMaxY);
        }
        else // side == 3 - Spawn Cạnh PHẢI (X cố định, Y ngẫu nhiên)
        {
            // X nằm ngoài khu vực game, ví dụ: X = Random.Range(91, 91 + outerBorder)
            randomX = Random.Range(gameMaxX, spawnMaxX);
            // Y nằm trong phạm vi lớn hơn
            randomY = Random.Range(spawnMinY, spawnMaxY);
        }

        spawnPosition = new Vector3(randomX, randomY, 0f);
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        EnemyMovement_Rush enemyMovement = newEnemy.GetComponent<EnemyMovement_Rush>();
        if (enemyMovement != null)
        {
            enemyMovement.SetTarget(playerTransform);
        }
    }
}