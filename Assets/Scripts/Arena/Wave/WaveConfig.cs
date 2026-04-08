using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWave", menuName = "Game/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Settings")]
    public float timeBetweenSpawns = 1f; // Thời gian nghỉ giữa mỗi lần spawn 1 con

    [Header("Enemies List")]
    public List<EnemyGroup> enemyGroups; // Danh sách các nhóm quái trong wave này

    [System.Serializable]
    public class EnemyGroup
    {
        public string enemyName;
        public GameObject enemyPrefab;
        public int amount; // Số lượng
    }
    public int GetTotalEnemyCount()
    {
        int total = 0;
        foreach (var group in enemyGroups) total += group.amount;
        return total;
    }
}