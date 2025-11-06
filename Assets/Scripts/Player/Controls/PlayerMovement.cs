// 1. Gerekli kütüphaneleri ekliyoruz
using UnityEngine;
using UnityEngine.InputSystem; // Yeni Input System'i kullanmak için bu satır ZORUNLUDUR.

// 2. Bu script'in çalışması için bir Rigidbody bileşeni ZORUNLUDUR.
// Bu satır, Rigidbody eklemeyi unutursak Unity'nin bizi uyarmasını sağlar.

public class PlayerMovement : MonoBehaviour // MonoBehaviour, Unity'deki temel script sınıfıdır 
{
    // 3. Değişkenler
    // Bu özellik, private bir değişkeni Inspector'da görünür yapar.
    [SerializeField] private float moveSpeed = 5f; // Karakterimizin hareket hızı.

    private Vector2 moveInput; // "Move" eyleminden gelen (X, Y) girdisini saklamak için.
    private Rigidbody rb; // Karakterimizin fizik bedenine (Rigidbody) referans.

    // 4. Awake, oyun başladığında Start'tan bile önce çalışan bir fonksiyondur.
    // Genellikle referansları (bileşenleri) çekmek için kullanılır.
    private void Awake()
    {
        // Script'in eklendiği GameObject  üzerindeki Rigidbody bileşenini bul ve 'rb' değişkenine ata.
        rb = GetComponent<Rigidbody>(); 
    }

    // 5. Input System tarafından çağrılacak FONKSİYON.
    // ÖNEMLİ: Fonksiyonun adı, Adım 1'de oluşturduğumuz Eylem'in (Action) adıyla 
    // ("Move") aynı olmalı ve başında "On" olmalıdır: "OnMove".
    // "InputValue" tipinde bir parametre alır.
    public void OnMove(InputValue value)
    {
        // Gelen girdinin (WASD) Vector2 değerini al ve 'moveInput' değişkenimize kaydet.
        moveInput = value.Get<Vector2>();
    }

    // 6. FixedUpdate, fizik hesaplamaları için en doğru yerdir. 
    // Her zaman sabit bir zaman aralığında (genellikle 0.02 saniye) çalışır.
    // FixedUpdate, fizik hesaplamaları için en doğru yerdir.
    private void FixedUpdate()
    {
        // 1. Girdimizi (moveInput) yerel (local) bir yön vektörü olarak alıyoruz.
        // X (A/D) sağ/sol, Z (W/S) ileri/geri
        Vector3 localMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        // 2. BU, EN ÖNEMLİ ADIMDIR:
        // Bu yerel yönü, karakterimizin mevcut baktığı yöne (dünya yönüne) dönüştürüyoruz.
        // 'transform.forward'  karakterin baktığı "ileri" (mavi ok) yönüdür.
        // 'transform.right'  karakterin baktığı "sağ" (kırmızı ok) yönüdür.
        Vector3 worldMoveDirection = (transform.forward * localMoveDirection.z) + (transform.right * localMoveDirection.x);

        // 3. Hareketi Rigidbody'e  uygula
        // Rigidbody'nin  mevcut pozisyonuna, hesapladığımız dünya yönünü ekliyoruz.
        rb.MovePosition(rb.position + worldMoveDirection * moveSpeed * Time.fixedDeltaTime);
    }
}