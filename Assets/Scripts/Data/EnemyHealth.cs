using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
   
    public int maxHealth = 100; // Maksimum can değeri

    private int currentHealth; 
    // Bu, düşmanın temel puan değeridir. (Örn: 100 puan)
private const int BaseScoreValue = 100;

    // Start, oyun başladığında bir kez çalışır.
    void Start()
    {
        // Başlangıçta canı maksimum cana eşitleriz.
        currentHealth = maxHealth;
    }

    // Bu, dışarıdan (PlayerShoot script'i gibi) çağrılacak hasar alma fonksiyonudur.
    public void TakeDamage(int damageAmount)
    {
        // 1. Hasarı Uygula
        currentHealth -= damageAmount;

        // Konsola bilgi yazdır
        Debug.Log(transform.name + " hasar aldı. Kalan Can: " + currentHealth); 

        // 2. Canı Kontrol Et
        if (currentHealth <= 0)
        {
            Die();
        }

        // TODO: Hasar alındığına dair görsel geri bildirim (Kızarma, kan efekti) eklenecek.
    }

    // Can 0'a düştüğünde çağrılan fonksiyondur.
    void Die()
    {
        Debug.Log(transform.name + " yok edildi!"); 

        // --- İLERİ KODLAMA NOKTASI ---
        // Burası, Kombo ve Puanlama sistemimizin (GameManager.cs) çağrılacağı yerdir.
        // Örneğin: GameManager.Instance.EnemyKilled(gameObject);
        // -----------------------------
        // --- ÖNEMLİ DEĞİŞİKLİK BURADA ---
    // GameManager'a (Singleton) düşmanın öldüğünü ve temel puanı bildir.
    if (GameManager.Instance!= null)
    {
        GameManager.Instance.EnemyKilled(BaseScoreValue);
    }


        // GameObject'i sahneden kalıcı olarak kaldır.
        Destroy(gameObject); 
    }
}