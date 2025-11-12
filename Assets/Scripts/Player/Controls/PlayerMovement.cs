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
    [Header("Zıplama Ayarları")]
    [SerializeField]private float jumpForce = 5f;     
    
    // --- YENİ EKLENEN DEĞİŞKENLER ---
    [Tooltip("Düşerken uygulanacak ekstra yerçekimi çarpanı")]
    [SerializeField] private float fallMultiplier = 2.5f; 
    
    [Tooltip("Zıplama tuşu erken bırakılırsa uygulanacak yerçekimi çarpanı")]
    [SerializeField] private float lowJumpMultiplier = 2f;

    // Zıplama tuşunun basılı tutulup tutulmadığını kontrol eder
    private bool isJumpHeld = false;   
    // --- YENİ EKLENDİ (Adım Sesi) ---
    [Header("Adım Sesi Ayarları")]
    [SerializeField] private AudioClip[] footstepSounds; // Birden fazla ses için dizi
    [SerializeField] private float footstepDelay = 0.4f; // İki adım arası bekleme süresi
    private float footstepTimer = 0f;
    private AudioSource audioSource; // Oyuncunun ses kaynağı
    // ---  

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
        
        // --- YENİ EKLENDİ ---
        // (Player'a 'AudioSource' eklediğinden emin ol)
        audioSource = GetComponent<AudioSource>(); 
        if (audioSource == null) // Güvenlik önlemi
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

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
        jumpAction.performed += HandleJumpPerformed; // 'HandleJump' adını değiştirdik
        jumpAction.canceled += HandleJumpCanceled;   // <-- YENİ EKLENDİ
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
        jumpAction.performed -= HandleJumpPerformed;
        jumpAction.canceled -= HandleJumpCanceled;   // <-- YENİ EKLENDİ
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

    // Zıplama tuşuna BASILDIĞINDA çalışır
    private void HandleJumpPerformed(InputAction.CallbackContext context)
    {
        // Yerdeysek, dash yapmıyorsak ve staminamız varsa zıpla
        if (IsGrounded() && !isDashing && currentStamina >= jumpStaminaCost) 
        {
            currentStamina -= jumpStaminaCost;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); 
            canSprint = true;
            
            // YENİ: Tuşun basılı olduğunu işaretle
            isJumpHeld = true;
        }
    }

    // Zıplama tuşu BIRAKILDIĞINDA çalışır
    private void HandleJumpCanceled(InputAction.CallbackContext context)
    {
        // YENİ: Tuşun bırakıldığını işaretle
        isJumpHeld = false;
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

// 1. HandleMelee (SPAM'İ TESPİT EDECEK OLAN KAPI)
private void HandleMelee(InputAction.CallbackContext context)
{
    // Bu log, sağ tıka bastığın an HER ÇAĞRIDA tetiklenir
    Debug.Log("HandleMelee: GİRDİ! (Spam Testi 1)", this);

    // 1. KORUMA: Zaten bir saldırı yapılıyorsa, spam'i engelle.
    if (isMeleeAttacking)
    {
        Debug.LogWarning("HandleMelee: Engellendi (Zaten saldırıyor)", this);
        return;
    }

    // 2. KORUMA: Stamina yoksa engelle.
    if (currentStamina < meleeStaminaCost)
    {
        Debug.LogWarning("HandleMelee: Engellendi (Stamina yok)", this);
        return;
    }

    // --- KİLİT DÜZELTME BURADA ---
    // Bayrağı 'true' yap.
    isMeleeAttacking = true; 
    
    // Bu log, SADECE bayrak 'false' ise tetiklenir
    Debug.Log("HandleMelee: KİLİT GEÇİLDİ! Coroutine başlıyor! (Spam Testi 2)", this);

    // 4. Coroutine'i SADECE BİR KEZ başlat.
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
        if (isDashing) return; // Dash sırasında diğer fizik işlemleri durdur

        // --- 1. BÖLÜM: YATAY HAREKET (Senin kodun) ---
        if (moveInput.magnitude < 0.1f)
        {
            // Hareket etmiyorsak YATAY hızı sıfırla (kaymayı önle)
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
        else
        {
            // Hareket ediyorsak (Rigidbody.MovePosition daha iyi çalışır)
            float currentSpeed = (isSprinting && canSprint && moveInput.magnitude > 0)? sprintSpeed : walkSpeed;
            Vector3 localMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 worldMoveDirection = (transform.forward * localMoveDirection.z) + (transform.right * localMoveDirection.x);
            worldMoveDirection.y = 0;
            Vector3 newPosition = rb.position + worldMoveDirection * currentSpeed * Time.fixedDeltaTime; 
            rb.MovePosition(newPosition);
        }

        // --- 2. BÖLÜM: DAHA İYİ YERÇEKİMİ (YENİ EKLENDİ) ---

        // EĞER DÜŞÜYORSAK (Y hızı negatifse)
        if (rb.linearVelocity.y < 0)
        {
            // Düşüşü hızlandır (fallMultiplier)
            // (Physics.gravity.y zaten negatif, bu yüzden (fallMultiplier - 1) ile çarpmak doğru)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // EĞER YÜKSELİYORSAK (Y hızı pozitifse) AMA Zıplama Tuşunu BIRAKTIYSAK
        else if (rb.linearVelocity.y > 0 && !isJumpHeld)
        {
            // Yükselişi durdur ve düşüşü başlat (lowJumpMultiplier)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        HandleFootsteps();
    }

    private IEnumerator PerformMelee()
    {
        Debug.Log("PerformMelee: Coroutine BAŞLADI!", this);

        currentStamina -= meleeStaminaCost;

        List<EnemyHealth> enemiesHit = new List<EnemyHealth>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, meleeRange);

        foreach (Collider enemyCollider in hitColliders)
        {
            EnemyHealth enemyHealth = enemyCollider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null && !enemiesHit.Contains(enemyHealth))
            {
                Debug.Log($"PerformMelee: Düşman bulundu ve vuruldu: {enemyHealth.name}", this);

                enemiesHit.Add(enemyHealth);
                Rigidbody enemyRb = enemyHealth.GetComponentInParent<Rigidbody>();

                // Hasar ver
                enemyHealth.TakeDamage(meleeDamage, enemyCollider.transform.position, Quaternion.identity, "Torso");

                // İtme kuvveti uygula
                if (enemyRb != null)
                {
                    Debug.Log($"PerformMelee: Güç uygulanıyor! Force: {meleePushForce}", this);
                    Vector3 pushDirection = (enemyCollider.transform.position - transform.position).normalized;
                    pushDirection.y = 0.5f;
                    enemyRb.AddForce(pushDirection * meleePushForce, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(0.4f);

        Debug.Log("PerformMelee: Cooldown bitti, kilit açıldı.", this);
        isMeleeAttacking = false;
    }
private void HandleFootsteps()
    {
        if (footstepSounds.Length == 0 || audioSource == null) return; // Ses yoksa çık

        // 1. Zamanlayıcıyı azalt
        if (footstepTimer > 0)
        {
            footstepTimer -= Time.fixedDeltaTime;
        }

        // 2. Hareket ediyor mu VE Yerde mi?
        bool isMoving = moveInput.magnitude > 0.1f;
        if (isMoving && IsGrounded() && !isDashing)
        {
            // 3. Zamanlayıcı sıfırlandıysa ses çal
            if (footstepTimer <= 0f)
            {
                // Rastgele bir adım sesi seç
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.PlayOneShot(clip, 0.3f); // (0.3f = ses seviyesi)
                
                // Zamanlayıcıyı sıfırla (eğer koşuyorsa daha hızlı adım atsın)
                footstepTimer = isSprinting ? (footstepDelay / 1.5f) : footstepDelay;
            }
        }
    }
}