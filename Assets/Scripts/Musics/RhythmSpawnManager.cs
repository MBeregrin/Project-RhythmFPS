using UnityEngine;

public class RhythmSpawnManager : MonoBehaviour 
{
   [Header("Mevcut Şarkı Ayarları")]
public SongData currentSong;
   
    [SerializeField]private float spawnThreshold = 10f; // Tetikleme eşiği (Inspector'da ayarlanır)
   
    [SerializeField]private float spawnCooldown = 0.5f; // Sakinleşme süresi
    
    [SerializeField]private float currentCooldown = 0f;
    private bool canSpawn = true;      

   
   
    [SerializeField]private GameObject enemyPrefab; // Yaratılacak düşman prefab'ı 

   
    [SerializeField]private float minSpawnDistance = 15f; // Oyuncuya en az bu kadar uzakta doğsun
   
    [SerializeField]private float maxSpawnDistance = 30f; // Oyuncudan en fazla bu kadar uzakta doğsun

    private Transform playerTransform;

    void Start()
    {
        // YENİ: Başlangıçta şarkı verilerini yükle
    if (currentSong!= null)
    {
        spawnThreshold = currentSong.spawnThresholdOverride;
        spawnCooldown = currentSong.spawnCooldownOverride;
        enemyPrefab = currentSong.easyEnemyPrefab; // Veya zorluk kontrolü yap
        // Not: AudioSource'u da buradan currentSong.songClip ile değiştirebilirsiniz.
    }
        // Oyuncuyu 'Player' etiketi ile bulup referansını alıyoruz
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); 
        if (playerObject!= null)
        {
            playerTransform = playerObject.transform; 
        }
        else
        {
            // Hata tespiti
            Debug.LogError("RhythmSpawnManager: Sahnede 'Player' etiketli bir obje bulunamadı!");
            this.enabled = false; 
        }
    }
    
    void Update()
    {
        // --- 1. Sakinleşme (Cooldown) Sayacını Yönet ---
        if (!canSpawn)
        {
            currentCooldown -= Time.deltaTime; 
            if (currentCooldown <= 0f)
            {
                canSpawn = true;
            }
        }

        // --- 2. Ritim Tespiti ve Spawn Etme ---
        if (canSpawn && playerTransform!= null)
        {
            // AudioAnalyzer'daki static değeri oku
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
    // 1. Kontrol: Gerekli referanslar dolu mu?
    if (enemyPrefab == null || playerTransform == null)
    {
        Debug.LogError("RhythmSpawnManager: 'Enemy Prefab' veya 'Player Transform' atanmamış!");
        return; 
    }

    // 2. Rastgele bir YÖN (direction) hesapla
    Vector2 randomCircle = Random.insideUnitCircle.normalized; 
    Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);

    // 3. Rastgele bir MESAFE (distance) hesapla
    float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

    // 4. TEMEL Spawn Pozisyonunu Hesapla (X ve Z)
    Vector3 baseSpawnPos = playerTransform.position + (randomDirection * randomDistance);

    // --- YENİ EKLENEN KISIM: ZEMİN TESPİTİ (Raycast) ---
    Vector3 finalSpawnPosition;
    float verticalOffset = 1.0f; // Düşmanın zeminin 1 metre üstünde doğması için
    RaycastHit hit;

    // Hesaplanan (X,Z) noktasında, yüksek bir yerden (Y=100) aşağıya doğru ışın gönder
    if (Physics.Raycast(new Vector3(baseSpawnPos.x, 100f, baseSpawnPos.z), Vector3.down, out hit, 200f))
    {
        // Işın bir zemine çarptı. Çarpma noktasının Y'sini al + offset ekle.
        finalSpawnPosition = new Vector3(baseSpawnPos.x, hit.point.y + verticalOffset, baseSpawnPos.z);
    }
    else
    {
        // Işın hiçbir yere çarpmadı (haritanın dışı?), varsayılan olarak havada spawn et
        // (veya oyuncunun Y seviyesinde)
        finalSpawnPosition = new Vector3(baseSpawnPos.x, playerTransform.position.y + verticalOffset, baseSpawnPos.z);
        Debug.LogWarning("Düşman spawn noktası zemin bulamadı, havada doğuyor!");
    }
    // --- YENİ KISIM SONU ---

    // 5. Düşmanı Yarat (Instantiate)
    GameObject spawnedEnemyObject = Instantiate(enemyPrefab, finalSpawnPosition, Quaternion.identity); 

    // (EĞER BİR ÖNCEKİ ADIMDAKİ TAVSİYEMİ UYGULADIYSANIZ, BU KODU BURADA TUTUN)
    // 6. Düşmanın "Can" script'ini bul ve SongData'dan canını ayarla
    EnemyHealth spawnedHealth = spawnedEnemyObject.GetComponent<EnemyHealth>();
    if (spawnedHealth != null && currentSong != null)
    {
        spawnedHealth.Initialize(currentSong.baseEnemyHealth);
    }
}
}