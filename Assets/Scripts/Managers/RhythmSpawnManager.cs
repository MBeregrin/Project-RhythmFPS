using UnityEngine;

public class RhythmSpawnManager : MonoBehaviour 
{
   [Header("Mevcut Şarkı Ayarları")]
public SongData currentSong;
   
    [SerializeField]private float spawnThreshold = 10f; // Tetikleme eşiği (Inspector'da ayarlanır)
   
    [SerializeField]private float spawnCooldown = 0.5f; // Sakinleşme süresi
    
    [SerializeField]private float currentCooldown = 0f;
    private bool canSpawn = true;      
    // --- YENİ EKLENDİ ---
    private AudioAnalyzer audioAnalyzer; // Müzik çaları bulmak için referans

   
   
    [SerializeField]private GameObject enemyPrefab; // Yaratılacak düşman prefab'ı 

   
    [SerializeField]private float minSpawnDistance = 15f; // Oyuncuya en az bu kadar uzakta doğsun
   
    [SerializeField]private float maxSpawnDistance = 30f; // Oyuncudan en fazla bu kadar uzakta doğsun

    private Transform playerTransform;
    // --- YENİ EKLENEN DEĞİŞKENLER ---
    private float songDuration = 0f; // Seçilen şarkının toplam süresi
    private float songTimer = 0f; // Şarkının ne kadar süredir çaldığı
    private bool victoryTriggered = false; // Zaferin 1'den fazla tetiklenmesini önler

    void Start()
    {
        // 1. GameManager'ı bul ve seçilen şarkıyı al
        if (GameManager.Instance == null)
        {
            Debug.LogError("RhythmSpawnManager: GameManager bulunamadı!");
            this.enabled = false;
            return;
        }

        SongData currentSong = GameManager.Instance.selectedSong;

        if (currentSong == null)
        {
            Debug.LogError("RhythmSpawnManager: GameManager'dan şarkı yüklenemedi! (Ana Menü'den mi başlattın?)");
            this.enabled = false;
            return;
        }

        // --- YENİ EKLENDİ: AudioAnalyzer'ı (Müzik Çaları) bul ---
        // (Bu, 'MusicManager' objesinin üzerinde olduğunu varsayar)
        audioAnalyzer = FindFirstObjectByType<AudioAnalyzer>(); 
        if (audioAnalyzer == null)
        {
            Debug.LogError("RhythmSpawnManager: Sahnede 'AudioAnalyzer.cs' script'i bulunamadı!");
            this.enabled = false;
            return;
        }
        // ---

        // 3. Şarkı verilerini yükle
        spawnThreshold = currentSong.spawnThresholdOverride;
        spawnCooldown = currentSong.spawnCooldownOverride;
        enemyPrefab = currentSong.easyEnemyPrefab;

        // 4. Oyuncuyu bul
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); 
        if (playerObject!= null)
        {
            playerTransform = playerObject.transform; 
        }
        else
        {
            Debug.LogError("RhythmSpawnManager: Sahnede 'Player' etiketli bir obje bulunamadı!");
            this.enabled = false; 
        }

        // 5. --- KİLİT ADIM: MÜZİĞİ BAŞLAT ---
        audioAnalyzer.StartMusic(currentSong.songClip);
        // --- YENİ EKLENEN SATIRLAR ---
        // 6. Şarkı süresini ve zamanlayıcıyı ayarla
        songDuration = currentSong.songLength;
        songTimer = 0f;
        victoryTriggered = false;
        
        // (Eğer SongData'da 'songLength' ayarlı değilse, klibin uzunluğunu al)
        if (songDuration <= 0f && currentSong.songClip != null)
        {
            songDuration = currentSong.songClip.length;
        }
        // ---
    }
    
    void Update()
    {
        // --- YENİ EKLENEN ZAFER ZAMANLAYICI ---
        if (songDuration > 0 && !victoryTriggered)
        {
            songTimer += Time.deltaTime; // Zamanı say

            // Zaman doldu mu?
            if (songTimer >= songDuration)
            {
                // Evet, şarkı bitti!
                victoryTriggered = true; // Tekrar tetikleme
                
                // GameManager'a "Zafer" komutu gönder
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerVictory();
                }
            }
        }
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

        // --- YENİ GÜVENLİ SPAWN DÖNGÜSÜ ---
        int attemptCount = 0;
        int maxAttempts = 10; // 10 kez deneme hakkı
        bool foundValidPosition = false;
        Vector3 finalSpawnPosition = Vector3.zero;

        // Geçerli bir nokta bulana kadar (veya 10 kez deneyene) kadar döngüye gir
        while (!foundValidPosition && attemptCount < maxAttempts)
        {
            attemptCount++;

            // 2. Rastgele bir YÖN (direction) ve MESAFE (distance) hesapla
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

            // 3. TEMEL Spawn Pozisyonunu Hesapla (X ve Z)
            Vector3 baseSpawnPos = playerTransform.position + (randomDirection * randomDistance);

            // 4. ZEMİN TESPİTİ (Raycast)
            float verticalOffset = 1.0f; // Düşmanın PİVOT'unun zeminin 1m üstünde olmasını sağlar
            RaycastHit hit;
            Vector3 rayStart = new Vector3(baseSpawnPos.x, 100f, baseSpawnPos.z); // Gökten ışın indir

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f))
            {
                // BAŞARILI: Işın bir zemine çarptı.
                finalSpawnPosition = new Vector3(baseSpawnPos.x, hit.point.y + verticalOffset, baseSpawnPos.z);
                foundValidPosition = true; // Döngüden çık
            }
            // BAŞARISIZ: 'else' bloğu yok. Döngü devam edecek.
        }
        // --- DÖNGÜ SONU ---


        // 5. SONUCU DEĞERLENDİR
        if (foundValidPosition)
        {
            // 5a. Düşmanı Yarat (Instantiate)
            GameObject spawnedEnemyObject = Instantiate(enemyPrefab, finalSpawnPosition, Quaternion.identity);

            // 5b. (Önceki Tavsiye) Düşmanın canını ayarla
            EnemyHealth spawnedHealth = spawnedEnemyObject.GetComponent<EnemyHealth>();
            if (spawnedHealth != null && currentSong != null)
            {
                // (SongData <-> EnemyHealth bağlantısı)
                spawnedHealth.Initialize(currentSong.baseEnemyHealth);
            }
            // !!! DÜZELTME BURADA !!!
            // Önceki kodda bu 'if' bloğunun içini boş bırakıp parantezi
            // yanlış yere koymuştum. Şimdi doğru yerde.
        }
        else
        {
            // 5c. 10 denemede de başarısız oldu
            Debug.LogError($"RhythmSpawnManager: {maxAttempts} denemede de geçerli bir spawn noktası bulunamadı! Harita çok mu küçük veya 'maxSpawnDistance' ({maxSpawnDistance}) çok mu büyük?");
        }
    }
}