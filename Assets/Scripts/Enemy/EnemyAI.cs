using UnityEngine;

// Bu script'in çalışması için bir Rigidbody bileşeni ZORUNLUDUR.

public class EnemyAI : MonoBehaviour
{
    [Header("AI Ayarları")]
    public float moveSpeed = 3f; 

    private Transform playerTransform; 
    private Rigidbody rb; 

    // Awake, Rigidbody referansını alır.
    void Awake() 
    {
        rb = GetComponent<Rigidbody>(); 
        if (rb == null)
        {
            Debug.LogError("EnemyAI: Rigidbody bileşeni bulunamadı!"); 
        }
    }
    
    // Start, oyuncuyu bulur.
    void Start() 
    {
        // Player'ı Tag ile bul
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject!= null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("EnemyAI: Sahnede 'Player' etiketli bir obje bulunamadı!");
        }
    }

    // Update, sadece dönme için kullanılır.
    void Update()
    {
        if (playerTransform!= null)
        {
            // Düşmanın yüzünü sadece oyuncuya doğru döndür
            transform.LookAt(playerTransform); 
        }
    }

    // FixedUpdate, fizik motoru ile hareketi sağlar (birbirine girmeyi engeller).
    void FixedUpdate()
    {
        if (playerTransform!= null && rb!= null)
        {
            // 1. Hedef Pozisyonu Hesapla: Y pozisyonunu kilitler
            Vector3 targetPosition = playerTransform.position;
            targetPosition.y = transform.position.y; 

            // 2. Yönü Hesapla:
            Vector3 moveDirection = (targetPosition - transform.position).normalized;

            // 3. Yeni Pozisyonu Hesapla:
            Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime; 

            // 4. Rigidbody'ye yeni pozisyonu bildir.
            rb.MovePosition(newPosition); 
        }
    }
}