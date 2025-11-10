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
    [SerializeField]private float jumpStaminaCost = 10f; 
    [SerializeField]private float dashStaminaCost = 25f; 
    
    private bool canSprint = true;
    private bool isDashing = false;

    // --- MELEE AYARLARI ---
    public float meleeRange = 1.5f;
    public float meleeStaminaCost = 15f;
    public int meleeDamage = 10;
    public float meleePushForce = 5f;
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
    
    // Input System Fonksiyonları
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnSprint(InputValue value)
{
    // Sadece tuşun durumunu kaydet.
    // Stamina kontrolünü HandleStamina fonksiyonu yapacak.
    isSprinting = value.isPressed;
}
    
    public void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded() && !isDashing && currentStamina >= jumpStaminaCost) 
        {
            currentStamina -= jumpStaminaCost;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); 
            canSprint = true;
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && currentStamina >= dashStaminaCost)
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
    
    // --- STAMINA YÖNETİMİ ---
    private void HandleStamina()
{
    // Dash veya Melee atağı sırasında stamina değişmez (Bu satır doğru, kalsın)
    if (isDashing || isMeleeAttacking) return;

    bool isMoving = moveInput.magnitude > 0;

    // DURUM 1: Sprint'e basıyorsan, hareket ediyorsan VE stamina varsa = TÜKET
    if (isSprinting && canSprint && isMoving && currentStamina > 0f) 
    {
        currentStamina -= staminaConsumptionRate * Time.deltaTime; 
        
        if (currentStamina <= 0)
        {
            currentStamina = 0;
            canSprint = false; // Stamina bitti, sprint yeteneğini kapat
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y, rb.linearVelocity.z * 0.1f);
        }
    }
    // DURUM 2: Sprint'e basmıyorsan VEYA hareket etmiyorsan = YENİLE
    // (Dash atmadığın sürece)
    else if ((!isSprinting || !isMoving) && !isDashing && currentStamina < maxStamina)
    {
        currentStamina += staminaRegenRate * Time.deltaTime; 
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina); 
        
        // Stamina %10'un üzerine çıkarsa (veya belirlediğin bir eşiğe)
        // tekrar sprint'e izin ver
        if (currentStamina > maxStamina * 0.1f) 
        {
            canSprint = true;
        }
    }
    
    // (Eğer isSprinting=true, isMoving=true ama canSprint=false ise
    // stamina 0'da kalır. Kullanıcı tuşu bırakınca veya durunca 
    // DURUM 2 tetiklenir ve yenilenme başlar. Bu mantık doğrudur.)
}

    private void UpdateStaminaUI()
    {
        if (staminaBarImage == null) return; 
        staminaBarImage.fillAmount = currentStamina / maxStamina;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return true; 
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer); 
    }

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

    public void OnMelee(InputValue value)
    {
        if (value.isPressed && !isMeleeAttacking && currentStamina >= meleeStaminaCost)
        {
            StartCoroutine(PerformMelee());
        }
    }

    private IEnumerator PerformMelee()
    {
        isMeleeAttacking = true;
        currentStamina -= meleeStaminaCost;

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, meleeRange);

        foreach (Collider enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = enemyCollider.GetComponent<EnemyHealth>();
                Rigidbody enemyRb = enemyCollider.GetComponent<Rigidbody>();

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(meleeDamage, enemyCollider.transform.position, Quaternion.identity, "Torso"); 

                    if (enemyRb != null)
                    {
                        Vector3 pushDirection = (enemyCollider.transform.position - transform.position).normalized;
                        pushDirection.y = 0.5f; 
                        enemyRb.AddForce(pushDirection * meleePushForce, ForceMode.Impulse); 
                    }
                }
            }
        }

        yield return null;
        isMeleeAttacking = false;
    }
}
