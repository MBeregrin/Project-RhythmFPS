using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    // --- YENİ EKLENEN ANAHTARLAR ---
    public const string SENSITIVITY_KEY = "MouseSensitivity";
    public const float DEFAULT_SENSITIVITY = 100f;
    // ---
    [Header("Ayarlar")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform; // Kameranın kendisi

    // --- YENİ GİRDİ SİSTEMİ DEĞİŞKENLERİ ---
    private PlayerInput playerInput;
    private InputAction lookAction;
    // ---

    private Vector2 lookInput; // "Look" eyleminden gelen (X, Y) girdisi
    private float xRotation = 0f; // Kameranın dikey açısı

    // YENİ: Awake, PlayerInput ve 'Look' eylemini bulur
    private void Awake()
    {
        // PlayerInput'u ana Player objesinden al
        // (Bu script ana Player objesindeyse GetComponent yeterli)
        playerInput = GetComponent<PlayerInput>(); 
        if (playerInput == null)
        {
            Debug.LogError("PlayerLook: 'PlayerInput' bileşeni bulunamadı!");
        }
        
        // Eylem haritasından "Look" eylemini adıyla bul
        lookAction = playerInput.actions["Look"];
    }

    // YENİ: OnEnable, eyleme abone olur
    private void OnEnable()
    {
        // 'Look' eylemi tetiklendiğinde (performed) VEYA durduğunda (canceled)
        // 'HandleLook' fonksiyonunu çağır
        lookAction.performed += HandleLook;
        lookAction.canceled += HandleLook; // Bu, durduğunda (0,0) vektörü almak için önemlidir
    }

    // YENİ: OnDisable, abonelikten çıkar
    private void OnDisable()
    {
        lookAction.performed -= HandleLook;
        lookAction.canceled -= HandleLook;
    }

    // Start, fare imlecini kilitler (Aynı kaldı)
    private void Start()
    {
        // --- YENİ EKLENEN AYAR YÜKLEME SATIRI ---
        mouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENSITIVITY);
        // ---
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // YENİ: HandleLook, C# aboneliği tarafından çağrılır
    // (Eski 'OnLook' fonksiyonunun yerini aldı)
    private void HandleLook(InputAction.CallbackContext context)
    {
        // Gelen Vector2 değerini (Mouse Delta) oku ve sakla
        lookInput = context.ReadValue<Vector2>();
    }

    // Update, kamerayı döndürür (Hiçbir değişiklik yok, bu mantık doğruydu)
    private void Update()
    {
        // 1. Girdiyi al ve hassasiyetle çarp
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // 2. Dikey (Y-Ekseni) Rotasyon - SADECE KAMERA
        xRotation -= mouseY; 
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 3. Yatay (X-Ekseni) Rotasyon - TÜM OYUNCU BEDENİ
        // Bu script "Player" nesnesinin üzerinde olduğu için 'transform' bedeni ifade eder
        transform.Rotate(Vector3.up * mouseX);
    }
}