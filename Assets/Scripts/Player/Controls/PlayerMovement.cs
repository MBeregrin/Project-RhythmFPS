using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;
using System.Collections; 

public class PlayerMovement : MonoBehaviour 
{
    [Header("Hareket Ayarları")]
    [SerializeField]private float walkSpeed = 5f; 
    [SerializeField]private float sprintSpeed = 10f; 
    [SerializeField]private float jumpForce = 5f;     

    [Header("Zemin Kontrolü")]
    public LayerMask groundLayer; 
    public Transform groundCheck;  
    private float groundDistance = 0.4f; 

    // Stamina Ayarları
   
    [SerializeField]private float maxStamina = 100f;
    [SerializeField]private float currentStamina = 100f;
    [SerializeField]private float staminaConsumptionRate = 30f; 
    [SerializeField]private float staminaRegenRate = 15f;     
    [SerializeField]private float jumpStaminaCost = 10f; // YENİ: Zıplama maliyeti
    [SerializeField]private float dashStaminaCost = 25f; 
    
    private bool canSprint = true;
    private bool isDashing = false;

    // --- YENİ: MELEE AYARLARI ---
   
   
    public float meleeRange = 1.5f;     // Dirsek atmanın menzili
   
    public float meleeStaminaCost = 15f; // Melee Stamina maliyeti
   
    public int meleeDamage = 10;        // Melee hasarı
   
    public float meleePushForce = 5f;   // Düşmanı itme kuvveti
    private bool isMeleeAttacking = false;    

    // UI Referansı
    public Image staminaBarImage; 

    // Girdi Değişkenleri
    private Vector2 moveInput; 
    private Rigidbody rb; 
    private bool isSprinting = false; 

    // Dash Mantığı
   
    [SerializeField]private float dashForce = 20f;      
    [SerializeField]private float dashDuration = 0.2f;  

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    private void Update()
    {
        HandleStamina();
        UpdateStaminaUI(); 
    }
    
    // Input System tarafından çağrılacak FONKSİYONLAR
    
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed; 
    }
    
    public void OnJump(InputValue value)
    {
        // YENİ KONTROL: Yerdeysek VE Stamina yeterliyse zıpla.
        if (value.isPressed && IsGrounded() &&!isDashing && currentStamina >= jumpStaminaCost) 
        {
            currentStamina -= jumpStaminaCost; // Maliyeti uygula
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); 
            canSprint = true; // Zıplama sprinti kilitlemez
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed &&!isDashing && currentStamina >= dashStaminaCost)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        currentStamina -= dashStaminaCost; 
        canSprint = true; 

        Vector3 dashDirection = (moveInput.magnitude > 0) 
          ? (transform.forward * moveInput.y + transform.right * moveInput.x).normalized 
            : transform.forward;

        dashDirection.y = 0; 
        
        rb.linearVelocity = Vector3.zero; 
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange); 

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y, rb.linearVelocity.z * 0.1f);
    }
    
    // Stamina Yönetim Mantığı (Hata Düzeltildi)
    private void HandleStamina()
    {
        // Dash yapıyorsak mantığı Coroutine yönetir.
        if (isDashing) return;

        // --- DÜZELTME: Tüketim mantığı ---
        // Sadece Sprint tuşu basılıyken VE hareket ediyorsak tüket.
        if (isSprinting && canSprint && moveInput.magnitude > 0) 
        {
            currentStamina -= staminaConsumptionRate * Time.deltaTime; 
            
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false; 
            }
        }
        // --- YENİLENME mantığı ---
        // Eğer sprint yapmıyorsak VEYA hareket etmiyorsak yenile.
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime; 
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina); 
            
            if (currentStamina > maxStamina * 0.1f) 
            {
                canSprint = true;
            }
        }
    }

    private void UpdateStaminaUI()
    {
        if (staminaBarImage == null) return; 
        staminaBarImage.fillAmount = currentStamina / maxStamina;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return true; // Hata almamak için geçici çözüm
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer); 
    }

    private void FixedUpdate()
    {
        if (isDashing) return; // Dash yapıyorsak normal hareketi engelle

    // Yeni Kontrol: Eğer hareket girdisi yoksa, karakteri hemen durdur.
    if (moveInput.magnitude < 0.1f) // 0.1f'den küçük sinyali "hareket yok" say.
    {
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Yatay hızı sıfırla, dikey hızı (yer çekimi) koru.
        return; // Fonksiyondan çık
    }
    
    // Eski hareket mantığı (moveInput > 0 ise çalışır)
    float currentSpeed = (isSprinting && canSprint && moveInput.magnitude > 0)? sprintSpeed : walkSpeed;

    Vector3 localMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
    Vector3 worldMoveDirection = (transform.forward * localMoveDirection.z) + (transform.right * localMoveDirection.x);

    worldMoveDirection.y = 0;

    Vector3 newPosition = rb.position + worldMoveDirection * currentSpeed * Time.fixedDeltaTime; 

    rb.MovePosition(newPosition);
    }
    public void OnMelee(InputValue value)
    {
        // Kontrol: Basıldı mı, Melee yapmıyor muyuz, Stamina yeterli mi?
        if (value.isPressed && !isMeleeAttacking && currentStamina >= meleeStaminaCost)
        {
            StartCoroutine(PerformMelee());
        }
    }
    // --- MELEE COROUTINE ---
    private IEnumerator PerformMelee()
{
    isMeleeAttacking = true;
    currentStamina -= meleeStaminaCost;

    // 1. Etki Alanını Tespit Et (OverlapSphere)
    // Karakterin önünde, belirlenen menzildeki tüm Collider'ları bul.
    Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, meleeRange);

    // 2. Etkilenen Düşmanlara Hasar Ver
    foreach (Collider enemyCollider in hitEnemies)
        {
            // Sadece düşmanları hedefle
            if (enemyCollider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
                Rigidbody enemyRb = enemyCollider.GetComponent<Rigidbody>();

                if (enemyHealth!= null)
                {
                    // YENİ VE DÜZELTİLMİŞ ÇAĞRI: Melee, her zaman Gövde hasarı (Torso) verir.
                    // HitPoint olarak düşmanın pozisyonunu, ColliderName olarak "Torso" gönderiyoruz.
                    enemyHealth.TakeDamage(
                        meleeDamage, 
                        enemyCollider.transform.position, 
                        Quaternion.identity, 
                        "Torso" // Sabit hasar (Gövde) uygulamak için 'Torso' adını gönder.
                    ); 

                    // İtme Kuvveti (Aynı kalır)
                    if (enemyRb!= null)
                    {
                        Vector3 pushDirection = (enemyCollider.transform.position - transform.position).normalized;
                        pushDirection.y = 0.5f; 
                        enemyRb.AddForce(pushDirection * meleePushForce, ForceMode.Impulse); 
                    }
                }
            }
        }

    // 3. Animasyon Süresini Taklit Et (1 Kare bekleme)
    yield return null;

    isMeleeAttacking = false;
}

}