using UnityEngine;

[CreateAssetMenu(fileName = "New Song", menuName = "Rhythm Game/New Song")]

public class SongData : ScriptableObject
{
    [Header("Music Source")]
    public AudioClip songClip;
    public float songLength;  
    public float spawnThresholdOverride = 10f; 
    public float spawnCooldownOverride = 0.5f; 
    [Header("Difficulty Settings")]
    public int baseEnemyHealth = 100; 
    public GameObject easyEnemyPrefab; 
    public GameObject hardEnemyPrefab;
}