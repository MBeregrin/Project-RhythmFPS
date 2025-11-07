using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour 
{
    [Header("Hareket Ayarları")]
   
    [SerializeField]private float walkSpeed = 5f; 
   
    [SerializeField]private float sprintSpeed = 10f; 
   
    [SerializeField]private float jumpForce = 5f;     

    [Header("Zemin Kontrolü")]
   
    [SerializeField]public LayerMask groundLayer; 
   
    [SerializeField]public Transform groundCheck;  
    
    [SerializeField]private float groundDistance = 0.4f; 

    // YENİ AYARLAR
   
    [SerializeField]public float maxStamina = 100f;
   
    [SerializeField]public float currentStamina = 100f;
   
    [SerializeField]public float staminaConsumptionRate = 30f; // Saniyede tüketim hızı
   
    [SerializeField]public float staminaRegenRate = 15f;     // Saniyede yenilenme hızı

    private bool canSprint = true;           // Stamina bittiğinde sprinti kilitler
    
    public Image staminaBarImage; // StaminaBar_Fill nesnesini buraya sürükleyeceğiz

    // Girdi Değişkenleri
    private Vector2 moveInput; 
    private Rigidbody rb; 
    private bool isSprinting = false; 

    // Awake, Rigidbody referansını alır.
    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    // --- YENİ: Stamina Zamanlayıcısı için Update kullanılır ---
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
        // Basılı tutulma bilgisini günceller
        isSprinting = value.isPressed; 
    }
    
    public void OnJump(InputValue value)
    {
        // Tuşa sadece basıldığı anda ve YERDEYSE zıpla.
        if (value.isPressed && IsGrounded()) 
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); 
        }
    }

    // Stamina Yönetim Mantığı
    private void HandleStamina()
    {
        // 1. TÜKETİM (Koşuyorsak ve sprint yapabilirsek)
        if (isSprinting && canSprint)
        {
            currentStamina -= staminaConsumptionRate * Time.deltaTime;

            // Eğer stamina biterse, kilitlen.
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false;
            }
        }
        // 2. YENİLENME (Koşmuyorsak ve Full değilsek)
        else if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            // Stamina'nın maxHealth'i geçmesini engelle.
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

            // Eğer belirli bir eşiğin üstüne çıktıysak, tekrar sprint yapmasına izin ver.
            if (currentStamina > maxStamina * 0.1f) // Örn: %10 dolunca kilit açılır.
            {
                canSprint = true;
            }
        }

        // TODO: Buraya bir UI (Stamina Barı) güncelleme kodu eklenecek.
    }
    // --- YENİ FONKSİYON: Stamina Barını Güncelleme ---
    private void UpdateStaminaUI()
    
    {
        if (staminaBarImage == null)
    {
        return; 
    }
        
            
            // currentStamina / maxStamina oranını kullanarak barın doluluk oranını (0.0 ile 1.0 arası) ayarlar.
            staminaBarImage.fillAmount = currentStamina / maxStamina;
        
    }
    // ----------------------------------------------------


    // Zemin Kontrolü Fonksiyonu
    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer); 
    }


    // FixedUpdate, fizik işlemleri için kullanılır.
    private void FixedUpdate()
    {
        // --- KRİTİK DEĞİŞİKLİK: Sprint kontrolü ---
        // Sadece isSprinting TRUE VE canSprint TRUE ise sprintSpeed kullan.
        float currentSpeed = (isSprinting && canSprint && moveInput.magnitude > 0)? sprintSpeed : walkSpeed;

        // 1. Girdiyi al
        Vector3 localMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        // 2. Girdiyi bakış yönüne göre dönüştür
        Vector3 worldMoveDirection = (transform.forward * localMoveDirection.z) + (transform.right * localMoveDirection.x);

        worldMoveDirection.y = 0;

        // 3. Hareketi Rigidbody'e uygula
        Vector3 newPosition = rb.position + worldMoveDirection * currentSpeed * Time.fixedDeltaTime; 

        rb.MovePosition(newPosition); 
    }
}