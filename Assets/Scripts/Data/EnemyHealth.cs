using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    private int maxHealth = 100; 
    private int currentHealth; 
    
    // YENİ: Düşmanın temel hızı ve referansları
    private float originalMoveSpeed;
    private EnemyAI enemyAI;
    // --- YENİ LBH (KONUM BAZLI HASAR) ÇARPANLARI ---
    private const float HeadshotMultiplier = 2.0f; // Kafa (Kritik)
    private const float TorsoMultiplier = 1.0f;      // Gövde (Temel Hasar)
    private const float ArmLegMultiplier = 0.6f;     // Kol/Bacak (Düşük Hasar)
    // --------------------------------------------------

    [Header("Görsel Efektler")]
    [SerializeField]private GameObject bloodVFXPrefab; // Kan efekti
    
    // Start, oyun başladığında bir kez çalışır.
    void Start()
    {
        currentHealth = maxHealth;
        
        // AI Referansını al ve orijinal hızı sakla
        enemyAI = GetComponent<EnemyAI>();
        if (enemyAI!= null)
        {
            originalMoveSpeed = enemyAI.moveSpeed;
        }
    }

    // DEĞİŞTİ: Artık vurulan bölgeyi ve hasarı alıyor
    public void TakeDamage(int baseDamageAmount, Vector3 hitPoint, Quaternion hitRotation, string hitColliderName)
    {
        // 1. Hasar Çarpanını Hesapla (Hitbox adına göre)
        float damageMultiplier = 1.0f; // Varsayılan değer Torso/Gövde olsun

        // Collider adları büyük/küçük harf duyarlı olabilir, bu yüzden hepsini küçük harfe çevirelim.
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
        // Diğer bölgeler 1.0 çarpanını alır (Default)

        int finalDamage = Mathf.RoundToInt(baseDamageAmount * damageMultiplier); // Hasarı yuvarla
        
        // 2. Hasarı Uygula
        currentHealth -= finalDamage; 
        
        // Konsolda hangi hasarın uygulandığını görelim
        Debug.Log($"{transform.name} hasar aldı. Hasar: {finalDamage}. Kalan Can: {currentHealth}"); 

        // 3. Görsel Geri Bildirim (BloodVFX)
        if (bloodVFXPrefab!= null)
        {
            Instantiate(bloodVFXPrefab, hitPoint, hitRotation); 
        }

        // 4. Yavaşlama Mantığı ve Can Kontrolü (Aynı kalır)
        ApplySlowdown();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // YENİ FONKSİYON: Can azalırken hızı düşürür
    void ApplySlowdown()
    {
        if (enemyAI!= null && originalMoveSpeed > 0)
        {
            // Mevcut canın maksimum cana oranı (örn: 30/100 = 0.3)
            float healthRatio = (float)currentHealth / maxHealth; 
            
            // Hızı, orijinal hızın bu oranı kadar düşür. 
            // Düşük can = Düşük hız.
            enemyAI.moveSpeed = originalMoveSpeed * healthRatio;
        }
    }


    void Die()
    {
        Debug.Log(transform.name + " yok edildi!"); 

        if (GameManager.Instance!= null)
        {
            const int BaseScoreValue = 100; 
            GameManager.Instance.EnemyKilled(BaseScoreValue);
        }

        Destroy(gameObject); 
    }
}