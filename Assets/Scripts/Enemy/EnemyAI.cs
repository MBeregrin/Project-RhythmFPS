using UnityEngine;

// 1. ADIM (Gelecekte): Animator bileşenini zorunlu kılmak için bu satırın yorumunu kaldır.
// [RequireComponent(typeof(Animator))] 
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Ayarları")]
    public float moveSpeed = 3f; 
    public float attackRange = 2f;      // Bu mesafeye gelince durup saldırır
    public float attackCooldown = 2f;   // İki saldırı arası bekleme süresi
    public int attackDamage = 10;       // Vereceği hasar

    // Referanslar
    private Transform playerTransform; 
    private PlayerHealth playerHealth;  
    private Rigidbody rb; 
    private Animator animator; // Animasyonları kontrol etmek için

    // Durum Değişkenleri
    private float currentCooldown = 0f;
    private bool canMove = true;
    private bool playerIsAlive = true;

    // Awake, bileşenleri alır.
    void Awake() 
    {
        rb = GetComponent<Rigidbody>(); 
        
        // 2. ADIM (Gelecekte): Animator'ü almak için bu satırın yorumunu kaldır.
        // animator = GetComponent<Animator>(); 
    }
    
    // Start, oyuncuyu bulur. (Tekrar 'void' yaptık, 'IEnumerator' değil)
    void Start() 
{
    // --- YÖNTEM GÜNCELLENDİ ---
    // 'FindObjectOfType' (eskimiş) yerine
    // 'FindFirstObjectByType' (yeni/hızlı) kullanıyoruz.
    playerHealth = FindFirstObjectByType<PlayerHealth>(); 

    if (playerHealth != null)
    {
        // Script'i bulduysak, o script'in bağlı olduğu
        // objenin 'transform'unu al.
        playerTransform = playerHealth.transform; 
    }
    else
    {
        // Bu hata, 'Player' objende 'PlayerHealth.cs' script'i yoksa tetiklenir.
        Debug.LogError("EnemyAI: Sahnede 'PlayerHealth.cs' script'ine sahip bir obje bulunamadı!");
    }
}

    // Update, Karar Verme (AI Brain) için kullanılır.
    void Update()
    {
        // 1. Cooldown sayacı
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }

        // 2. Oyuncu var mı ve yaşıyor mu? (Bu GÜVENLİK KONTROLÜ çok önemli)
        if (playerTransform == null || playerHealth == null || playerHealth.currentHealth <= 0)
        {
            if (playerIsAlive) 
            {
                playerIsAlive = false;
                canMove = false; 
                // 3. ADIM (Gelecekte): Yorumu kaldır
                // animator.SetTrigger("PlayerDead"); 
            }
            return; // Oyuncu yoksa veya ölmüşse AI'ı durdur
        }

        // 3. Mesafe kontrolü
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 4. Karar Verme (State Machine)
        if (distance <= attackRange)
        {
            // --- MENZİLDE ---
            canMove = false; // Hareketi durdur
            TryAttack();     // Saldırmayı dene
        }
        else
        {
            // --- MENZİL DIŞINDA ---
            canMove = true; // Harekete devam et
            
            // 4. ADIM (Gelecekte): Yorumu kaldır
            // animator.SetBool("IsMoving", true); 
        }

        // 5. Yüzünü oyuncuya dön (sadece XZ ekseninde)
        Vector3 lookDirection = playerTransform.position - transform.position;
        lookDirection.y = 0; 
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    void FixedUpdate()
    {
        // KİLİT GÜVENLİK KONTROLÜ
        if (playerTransform == null || !playerIsAlive)
        {
            // Oyuncu yoksa/ölmüşse, YATAY hızı sıfırla, DÜŞEY hızı (yerçekimi) koru
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
            return;
        }

        // Karar 'Update()' içinde verilir ('canMove' ayarlanır)
        if (canMove)
        {
            // Hareket etmemiz gerekiyorsa
            MoveTowardsPlayer();
        }
        else
        {
            // Hareket etmememiz gerekiyorsa (saldırı menzilindeysek)
            // YATAY hızı sıfırla, DÜŞEY hızı (yerçekimi) koru
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    // Hareket fonksiyonu
    void MoveTowardsPlayer()
    {
        // 1. Hedef yönü (Y eksenini koruyarak) hesapla
        Vector3 targetDirection = playerTransform.position - transform.position;
        targetDirection.y = 0; // Y ekseninde ona doğru gitmeye çalışma
        Vector3 moveDirection = targetDirection.normalized;

        // 2. Hedef hızı (Velocity) ayarla. 
        // 'MovePosition' yerine 'velocity' kullanmak,
        // Rigidbody'nin yerçekimine ve çarpışmalara uymasını sağlar.
        Vector3 targetVelocity = moveDirection * moveSpeed;
        
        // 3. Y hızına DOKUNMA, yerçekiminin halletmesine izin ver.
        targetVelocity.y = rb.linearVelocity.y; 
        
        // 4. Rigidbody'ye yeni hızı uygula
        rb.linearVelocity = targetVelocity; 
    }
    // Saldırıyı deneyen fonksiyon
    void TryAttack()
    {
        if (currentCooldown <= 0f)
        {
            currentCooldown = attackCooldown;
            Debug.Log("EnemyAI: Saldırıyor!");
            
            // 5. ADIM (Gelecekte): Yorumu kaldır
            // animator.SetTrigger("Attack");
            
            // 6. ADIM (Gelecekte): Bu satırı YORUM SATIRI YAP
            AnimationTrigger_DealDamage(); // Şimdilik hasarı manuel (gecikmesiz) veriyoruz
        }
    }

    // Hasar verme fonksiyonu
    public void AnimationTrigger_DealDamage()
    {
        // 'Update'teki 'null' kontrolü sayesinde bu fonksiyon çağrıldığında
        // 'playerHealth'in 'null' olması imkansız.
        
        Debug.Log("EnemyAI: Hasar verme fonksiyonu (DealDamage) tetiklendi!");

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distance <= attackRange * 1.2f) // (Tolerans payı)
        {
            Debug.Log("EnemyAI: Oyuncuya vurdu!");
            playerHealth.TakeDamage(attackDamage);
        }
        else
        {
            Debug.Log("EnemyAI: Vuruş ıska geçti (Oyuncu saldırıdan kaçtı)");
        }
    }
}