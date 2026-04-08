using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewArena", menuName = "Game/Arena Config")]
public class ArenaConfig : ScriptableObject
{
    public string arenaName; // Tên hiển thị 
    public string sceneName; // Tên Scene trong Unity 

    public List<WaveConfig> waves;

    [Header("Music Settings")]
    public List<AudioClip> normalWaveBGM; // List nhạc nền thường 
    public AudioClip bossWaveBGM;         // Nhạc trùm 
    public int bossWaveIndex = 9;
}