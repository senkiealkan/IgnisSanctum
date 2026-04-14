using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewArena", menuName = "Game/Arena Config")]
public class ArenaConfig : ScriptableObject
{
    public string arenaName; 
    public string sceneName; 

    public List<WaveConfig> waves;

    [Header("Music Settings")]
    public List<AudioClip> normalWaveBGM; 
    public AudioClip bossWaveBGM;         
    public int bossWaveIndex = 9;
}