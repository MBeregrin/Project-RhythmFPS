using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour // MonoBehaviour, temel Unity script sınıfıdır 
{
    [Header("Ayarlar")]
    public float mouseSensitivity = 100f; // Fare hassasiyeti, Inspector'dan ayarlanabilir.

   
    public Transform cameraTransform; // Kameramızın Transform'u  (Bunu Inspector'da sürükleyeceğiz).

    private Vector2 lookInput; // "Look" eyleminden gelen (X, Y) girdisini saklamak için.
    private float xRotation = 0f; // Kameranın dikey (aşağı/yukarı) açısını saklamak için.

    // Start, oyun başladığında bir kez çalışır.
    private void Start()
    {
        // Oyun başladığında fare imlecini gizle ve ekranın ortasına kilitle.
        // Bu, "Doom-umsu" FPS hissi için zorunludur.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Input System tarafından çağrılacak FONKSİYON.
    // Adı "On" + Eylem Adı ("Look") = "OnLook"
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    // Update, her karede (frame) bir kez çalışır.
    // Kamera hareketleri fiziksel olmadığı için Update kullanmak daha akıcıdır.
    private void Update()
    {
        // 1. Girdiyi al ve hassasiyetle çarp
        // Time.deltaTime kullanarak kare hızından (FPS) bağımsız hale getiriyoruz.
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        // 2. Dikey (Y-Ekseni) Rotasyon - SADECE KAMERA
        // Farenin Y hareketini (mouseY) X ekseninde rotasyon olarak uyguluyoruz (aşağı/yukarı bakış).
        // -= kullanıyoruz çünkü mouse Y ekseni varsayılan olarak terstir.
        xRotation -= mouseY; 

        // Kameranın 180 derece dönüp arkasına bakmasını engellemek için açıyı kilitliyoruz.
        // Genellikle -90 (tam aşağı) ve 90 (tam yukarı) derece arası kullanılır.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Hesaplanan dikey açıyı SADECE kameranın localRotation'ına uyguluyoruz.
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 3. Yatay (X-Ekseni) Rotasyon - TÜM OYUNCU BEDENİ
        // Farenin X hareketini (mouseX) kullanarak TÜM bedeni Y ekseninde (kendi etrafında) döndürüyoruz.
        // Bu script "Player" nesnesinin üzerinde olduğu için 'transform'  direkt olarak bedeni ifade eder.
        transform.Rotate(Vector3.up * mouseX);
    }
}