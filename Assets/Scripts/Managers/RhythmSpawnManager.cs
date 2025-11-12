using UnityEngine;

public class RhythmSpawnManager : MonoBehaviour 
{
   [Header("Current Song Settings")]
public SongData currentSong;
   
    [SerializeField]private float spawnThreshold = 10f;
   
    [SerializeField]private float spawnCooldown = 0.5f;
    
    [SerializeField]private float currentCooldown = 0f;
    private bool canSpawn = true;      
    private AudioAnalyzer audioAnalyzer;
    [SerializeField]private GameObject enemyPrefab;
    [SerializeField]private float minSpawnDistance = 15f;
   
    [SerializeField]private float maxSpawnDistance = 30f;

    private Transform playerTransform;
    private float songDuration = 0f;
    private float songTimer = 0f;
    private bool victoryTriggered = false;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            this.enabled = false;
            return;
        }
        SongData currentSong = GameManager.Instance.selectedSong;
        if (currentSong == null)
        {
            this.enabled = false;
            return;
        }
        audioAnalyzer = FindFirstObjectByType<AudioAnalyzer>(); 
        if (audioAnalyzer == null)
        {
            this.enabled = false;
            return;
        }
        spawnThreshold = currentSong.spawnThresholdOverride;
        spawnCooldown = currentSong.spawnCooldownOverride;
        enemyPrefab = currentSong.easyEnemyPrefab;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); 
        if (playerObject!= null)
        {
            playerTransform = playerObject.transform; 
        }
        else
        {
            this.enabled = false; 
        }

        audioAnalyzer.StartMusic(currentSong.songClip);
        songDuration = currentSong.songLength;
        songTimer = 0f;
        victoryTriggered = false;
        if (songDuration <= 0f && currentSong.songClip != null)
        {
            songDuration = currentSong.songClip.length;
        }
    }
    
    void Update()
    {
        if (songDuration > 0 && !victoryTriggered)
        {
            songTimer += Time.deltaTime;
            if (songTimer >= songDuration)
            {
                victoryTriggered = true; 
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerVictory();
                }
            }
        }
        if (!canSpawn)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
            {
                canSpawn = true;
            }
        }
        if (canSpawn && playerTransform!= null)
        {
            if (AudioAnalyzer.currentBassEnergy > spawnThreshold)
            {
                SpawnEnemy(); 
                canSpawn = false;
                currentCooldown = spawnCooldown;
            }
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || playerTransform == null)
        {
            return;
        }
        int attemptCount = 0;
        int maxAttempts = 10;
        bool foundValidPosition = false;
        Vector3 finalSpawnPosition = Vector3.zero;
        while (!foundValidPosition && attemptCount < maxAttempts)
        {
            attemptCount++;
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 baseSpawnPos = playerTransform.position + (randomDirection * randomDistance);
            float verticalOffset = 1.0f;
            RaycastHit hit;
            Vector3 rayStart = new Vector3(baseSpawnPos.x, 100f, baseSpawnPos.z);

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f))
            {
                finalSpawnPosition = new Vector3(baseSpawnPos.x, hit.point.y + verticalOffset, baseSpawnPos.z);
                foundValidPosition = true;
            }
        }

        if (foundValidPosition)
        {
            GameObject spawnedEnemyObject = Instantiate(enemyPrefab, finalSpawnPosition, Quaternion.identity);
            EnemyHealth spawnedHealth = spawnedEnemyObject.GetComponent<EnemyHealth>();
            if (spawnedHealth != null && currentSong != null)
            {
                spawnedHealth.Initialize(currentSong.baseEnemyHealth);
            }
        }
        else
        {

        }
    }
}