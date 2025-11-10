using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;
using System.Collections; 
using System.Collections.Generic; // Melee listesi için bu gerekli

public class PlayerMovement : MonoBehaviour
{
    // --- GİRDİ EYLEM REFERANSLARI (YENİ) ---
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction meleeAction;

    [Header("Hareket Ayarları")]
    [SerializeField]private float walkSpeed = 5f; 
    [SerializeField]private float sprintSpeed = 10f; 
    [SerializeField]private float jumpForce = 5f;     

    [Header("Zemin Kontrolü")]
    public LayerMask groundLayer; 
    public Transform groundCheck;  
    private float groundDistance = 0.4f; 

    [Header("Stamina Ayarları")]
    [SerializeField]private float maxStamina = 100f;
    [SerializeField]private float currentStamina = 100f;
    [SerializeField]private float staminaConsumptionRate = 30f; 
    [SerializeField]private float staminaRegenRate = 15f;     
    [SerializeField]private float jumpStaminaCost = 10f; 
    [SerializeField]private float dashStaminaCost = 25f; 
    
    private bool canSprint = true;
    private bool isDashing = false;

    [Header("Melee Ayarları")]
    public float meleeRange = 1.5f;
    public float meleeStaminaCost = 15f;
    public int meleeDamage = 10;
    public float meleePushForce = 5f;
    private bool isMeleeAttacking = false;    

    [Header("UI Referansı")]
    public Image staminaBarImage; 

    // Girdi Değişkenleri
    private Vector2 moveInput; 
    private Rigidbody rb; 
    private bool isSprinting = false; 

    [Header("Dash Mantığı")]
    [SerializeField]private float dashForce = 20f;      
    [SerializeField]private float dashDuration = 0.2f;  

    // --- YENİ AWAKE (Girdileri Alır) ---
    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerMovement: 'PlayerInput' bileşeni bulunamadı!");
            return;
        }

        // .inputactions dosyasındaki eylem adlarını (string) buraya yaz
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        meleeAction = playerInput.actions["Melee"];
    }

    // --- YENİ ONENABLE (Eylemlere Abone Ol) ---
    private void OnEnable()
    {
        // Hareket (Sürekli okunur)
        moveAction.performed += HandleMove;
        moveAction.canceled += HandleMove; // Durduğumuzu bilmek için bu da lazım

        // Sprint (Basıldı ve Bırakıldı)
        sprintAction.performed += HandleSprintPerformed;
        sprintAction.canceled += HandleSprintCanceled;

        // Diğerleri (Sadece basılma anı)
        jumpAction.performed += HandleJump;
        dashAction.performed += HandleDash;
        meleeAction.performed += HandleMelee;
    }

    // --- YENİ ONDISABLE (Abonelikten Çık) ---
    private void OnDisable()
    {
        moveAction.performed -= HandleMove;
        moveAction.canceled -= HandleMove;
        sprintAction.performed -= HandleSprintPerformed;
        sprintAction.canceled -= HandleSprintCanceled;
        jumpAction.performed -= HandleJump;
        dashAction.performed -= HandleDash;
        meleeAction.performed -= HandleMelee;
    }

    // Update, Stamina ve UI'ı yönetir (Değişiklik yok)
    private void Update()
    {
        HandleStamina();
        UpdateStaminaUI(); 
    }
    
    // --- YENİ GİRDİ YAKALAMA FONKSİYONLARI ---
    
    private void HandleMove(InputAction.CallbackContext context)
    {
        // 'performed' (hareket ederken) veya 'canceled' (durunca 0,0)
        moveInput = context.ReadValue<Vector2>();
    }

    private void HandleSprintPerformed(InputAction.CallbackContext context)
    {
        isSprinting = true;
    }

    private void HandleSprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        // 'context.performed' (basılma anı) zaten kontrol edildi
        if (IsGrounded() && !isDashing && currentStamina >= jumpStaminaCost) 
        {
            currentStamina -= jumpStaminaCost;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); 
            canSprint = true;
        }
    }

    private void HandleDash(InputAction.CallbackContext context)
    {
        if (!isDashing && currentStamina >= dashStaminaCost)
        {
            StartCoroutine(PerformDash());
        }
    }

    // PlayerMovement.cs içindeyiz

// PlayerMovement.cs içindeyiz

private void HandleMelee(InputAction.CallbackContext context)
{
    // 1. KORUMA: Zaten bir saldırı yapılıyorsa, spam'i engelle.
    if (isMeleeAttacking) return;

    // 2. KORUMA: Stamina yoksa engelle.
    if (currentStamina < meleeStaminaCost) return;

    // --- KİLİT DÜZELTME BURADA ---
    // Coroutine'i çağırmadan ÖNCE bayrağı 'true' yap.
    // Böylece bir sonraki 'HandleMelee' çağrısı (1. korumaya) takılır.
    isMeleeAttacking = true; 
    
    // 4. Artık güvenle Coroutine'i SADECE BİR KEZ başlatabiliriz.
    StartCoroutine(PerformMelee());
}
    private IEnumerator PerformDash()
    {
        isDashing = true;
        currentStamina -= dashStaminaCost; 
        canSprint = true; 
        Vector3 dashDirection = (moveInput.magnitude > 0) ? (transform.forward * moveInput.y + transform.right * moveInput.x).normalized : transform.forward;
        dashDirection.y = 0; 
        rb.linearVelocity = Vector3.zero; 
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange); 
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y, rb.linearVelocity.z * 0.1f);
    }
    
    // Stamina (Değişiklik yok, bu mantık artık doğru çalışacak)
    private void HandleStamina()
    {
        if (isDashing || isMeleeAttacking) return; 

        bool isMoving = moveInput.magnitude > 0;
        bool isTryingToSprint = isSprinting && isMoving;

        if (isTryingToSprint && canSprint)
        {
            currentStamina -= staminaConsumptionRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false; 
            }
        }
        else if (currentStamina < maxStamina && !isDashing) 
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina); 
            if (!canSprint && currentStamina > maxStamina * 0.1f)
            {
                canSprint = true;
            }
        }
    }

    // UI (Değişiklik yok)
    private void UpdateStaminaUI()
    {
        if (staminaBarImage == null) return; 
        staminaBarImage.fillAmount = currentStamina / maxStamina;
    }

    // Zemin Kontrolü (Değişiklik yok)
    private bool IsGrounded()
    {
        if (groundCheck == null) return true; 
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer); 
    }

    // Fizik (Değişiklik yok)
    private void FixedUpdate()
    {
        if (isDashing) return;

        if (moveInput.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }
        float currentSpeed = (isSprinting && canSprint && moveInput.magnitude > 0)? sprintSpeed : walkSpeed;
        Vector3 localMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 worldMoveDirection = (transform.forward * localMoveDirection.z) + (transform.right * localMoveDirection.x);
        worldMoveDirection.y = 0;
        Vector3 newPosition = rb.position + worldMoveDirection * currentSpeed * Time.fixedDeltaTime; 
        rb.MovePosition(newPosition);
    }

    // Melee ("Tek atma" sorunu düzeltilmiş versiyon)
    // PlayerMovement.cs içindeyiz

    private IEnumerator PerformMelee()
{
    // 'isMeleeAttacking = true;' satırı buradan KALDIRILDI.
    // Bayrak zaten HandleMelee'de ayarlandı.
    // Sadece staminayı tüket.
    currentStamina -= meleeStaminaCost;
    
    // "Tek atma" sorununu çözen liste mantığı (Bu zaten doğruydu)
    List<EnemyHealth> enemiesHit = new List<EnemyHealth>();
    Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, meleeRange);

    foreach (Collider enemyCollider in hitColliders)
    {
        EnemyHealth enemyHealth = enemyCollider.GetComponentInParent<EnemyHealth>();
        
        // Eğer bu bir düşmansa VE bu saldırıda daha önce vurulmadıysa...
        if (enemyHealth != null && !enemiesHit.Contains(enemyHealth))
        {
            // 1. Vurulanlar listesine ekle
            enemiesHit.Add(enemyHealth);
            Rigidbody enemyRb = enemyHealth.GetComponentInParent<Rigidbody>();
            
            // 2. Hasar ver (sadece 10)
            enemyHealth.TakeDamage(meleeDamage, enemyCollider.transform.position, Quaternion.identity, "Torso"); 

            // 3. İtme kuvveti uygula (Artık bunu göreceksin)
            if (enemyRb != null)
            {
                // Ayarlarından (Resim) Push Force'u 2 gördüm.
                // Eğer 2 yetersiz kalırsa bu 'meleePushForce' değerini
                // Inspector'dan 20 veya 50 yap. Kütle (Mass) de 1 olsun.
                Vector3 pushDirection = (enemyCollider.transform.position - transform.position).normalized;
                pushDirection.y = 0.5f; 
                enemyRb.AddForce(pushDirection * meleePushForce, ForceMode.Impulse); 
            }
        }
    }
    
    // Saldırı 'cooldown' süresi (animasyon süresi gibi düşün)
    yield return new WaitForSeconds(0.4f); 
    
    // Saldırı bitti, bayrağı 'false' yap ki YENİ BİR saldırı yapılabilsin.
    isMeleeAttacking = false; 
}
}