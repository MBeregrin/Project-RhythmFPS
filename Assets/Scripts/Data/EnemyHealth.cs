using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    // Varsayılan can 100 olarak ayarlandı
    private int maxHealth = 100; 
    private int currentHealth; 
    
    // Diğer değişkenler
    private float originalMoveSpeed;
    private EnemyAI enemyAI;
    private const float HeadshotMultiplier = 2.0f;
    private const float TorsoMultiplier = 1.0f;
    private const float ArmLegMultiplier = 0.6f; 
    // --- YENİ EKLENEN SES AYARLARI ---
    [Header("Ses Ayarları")]
    [SerializeField] private AudioClip hitSound; // Mermi isabet sesi
    [SerializeField] private AudioClip deathSound; // Ölüm sesi
    [SerializeField]private AudioSource audioSource;
    
    [Header("Drop Ayarları (Eşya Düşürme)")]
    [SerializeField] private GameObject healthPickupPrefab; // Adım 3'te oluşturduğun Can prefab'ı
    [SerializeField] private GameObject ammoPickupPrefab;   // Adım 3'te oluşturduğun Mermi prefab'ı
    
    [Tooltip("Düşmanın eşya düşürme şansı (% olarak, 0-100)")]
    [Range(0, 100)]
    [SerializeField] private float dropChance = 50f;

    [Header("Görsel Efektler")]
    [SerializeField]private GameObject bloodVFXPrefab;

    // 'Initialize' çağrılmasa BİLE canın 100 olmasını garanti eder.
    private void Awake()
    {
        currentHealth = maxHealth;

        // --- BU SATIRLARI GÜNCELLE/EKLE ---
        enemyAI = GetComponent<EnemyAI>();
        audioSource = GetComponent<AudioSource>(); // AudioSource'u al
        audioSource.spatialBlend = 1.0f; // Sesi 3D (konumsal) yap
        // ---
        
        // AI referansını da burada alalım
        enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            originalMoveSpeed = enemyAI.moveSpeed;
        }
    }

    // Bu fonksiyon 'RhythmSpawnManager' tarafından çağrılır
    // ve 100 olan canı, SongData'nın canı ile ezer.
    public void Initialize(int startingHealth)
    {
        maxHealth = startingHealth;
        currentHealth = maxHealth;
        
        // --- BURADAKİ SARI LOG KALDIRILDI ---
    }

    // Hasar alma fonksiyonu
    public void TakeDamage(int baseDamageAmount, Vector3 hitPoint, Quaternion hitRotation, string hitColliderName)
    {
        // --- BURADAKİ KIRMIZI LOG KALDIRILDI ---

        // 1. Hasar Çarpanını Hesapla
        float damageMultiplier = 1.0f;
        string hitNameLower = hitColliderName.ToLower();

        if (hitNameLower.Contains("head"))
        {
            damageMultiplier = HeadshotMultiplier;
            Debug.Log("KRİTİK VURUŞ: Headshot!");
        }
        else if (hitNameLower.Contains("torso") || hitNameLower.Contains("chest") || hitNameLower.Contains("stomach"))
        {
            damageMultiplier = TorsoMultiplier;
            Debug.Log("GÖVDE VURUŞU: Standart.");
        }
        else if (hitNameLower.Contains("arm") || hitNameLower.Contains("hand") || hitNameLower.Contains("leg") || hitNameLower.Contains("foot"))
        {
            damageMultiplier = ArmLegMultiplier;
            Debug.Log("UZUV VURUŞU: Hasar Azaltıldı.");
        }

        int finalDamage = Mathf.RoundToInt(baseDamageAmount * damageMultiplier);
        
        // Hasarı uygula
        currentHealth -= finalDamage;

        Debug.Log($"{transform.name} hasar aldı. Hasar: {finalDamage}. Kalan Can: {currentHealth}"); 
        
        // --- YENİ EKLENDİ (İsabet Sesi) ---
if (hitSound != null && audioSource != null)
{
    // 'PlayOneShot', üst üste binse bile sesi çalar (otomatik silahlarda önemli)
    audioSource.PlayOneShot(hitSound);
}
// ---

        // Görsel efekt
        if (bloodVFXPrefab!= null)
        {
            Instantiate(bloodVFXPrefab, hitPoint, hitRotation); 
        }

        // Yavaşlama ve Ölüm
        ApplySlowdown();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Yavaşlama fonksiyonu
    void ApplySlowdown()
    {
        if (enemyAI!= null && originalMoveSpeed > 0)
        {
            float healthRatio = Mathf.Max(0, (float)currentHealth / maxHealth); // 0'ın altına düşmesin
            enemyAI.moveSpeed = originalMoveSpeed * healthRatio;
        }
    }

    // Ölüm fonksiyonu
    void Die()
    {
        Debug.Log(transform.name + " yok edildi!");

        if (GameManager.Instance != null)
        {
            const int BaseScoreValue = 100;
            GameManager.Instance.EnemyKilled(BaseScoreValue);
        }
        // --- YENİ EKLENEN DROP MANTIĞI ---
        // 1. Zarı at (0 ile 100 arası rastgele bir sayı seç)
        float randomValue = Random.Range(0f, 100f);

        // 2. Şansımız yaver gitti mi? (örn: 50'den küçük mü?)
        if (randomValue <= dropChance)
        {
            // Evet, bir şey düşüreceğiz.
            // %50 şansla Can, %50 şansla Mermi düşürelim
            GameObject prefabToDrop = (Random.value > 0.5f) ? healthPickupPrefab : ammoPickupPrefab;

            // 3. Obje atanmış mı diye kontrol et (Null hatası almamak için)
            if (prefabToDrop != null)
            {
                // 4. Obje'yi düşmanın öldüğü yerde spawn et
                Vector3 spawnPosition = transform.position + (Vector3.up * 0.5f);

                Instantiate(prefabToDrop, spawnPosition, Quaternion.identity);
            }
        }
        if (deathSound != null)
    {
        AudioSource.PlayClipAtPoint(deathSound, transform.position);
    }
    // ---
        // --- DROP MANTIĞI SONU ---
        Destroy(gameObject); 
    }
}