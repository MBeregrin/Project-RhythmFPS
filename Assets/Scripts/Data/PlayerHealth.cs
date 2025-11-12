using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 

public class PlayerHealth : MonoBehaviour
{
    [Header("Can Ayarları")]
    [SerializeField] private int maxHealth = 100;
    public int currentHealth;

    [Header("UI Referansları")]
    public TextMeshProUGUI healthText;
    [HideInInspector] public bool isDead = false;
    // YENİ EKLENEN SATIR:
    // Bu, Canvas içindeki "Oyun Bitti" panelimiz olacak.
    [SerializeField] private GameObject gameOverScreen; 
    

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Oyunun başında Game Over ekranının kapalı olduğundan emin ol
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false); 
        }
        
        // Oyunun çalıştığından emin ol (eğer bir önceki oyundan 0 kalmışsa)
        Time.timeScale = 1f; 
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        Debug.Log($"OYUNCU HASAR ALDI: Kalan Can = {currentHealth}");

        UpdateHealthUI();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerTookDamage(damageAmount);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"CAN: {currentHealth}";
        }
    }
    public void Heal(int healAmount)
    {
        // Eğer can zaten tam doluysa, hiçbir şey yapma
        if (currentHealth >= maxHealth) return;

        // Canı artır
        currentHealth += healAmount;
        
        // Canın 'maxHealth'i geçmediğinden emin ol
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"OYUNCU İYİLEŞTİ: Mevcut Can = {currentHealth}");

        // Can Text'ini güncelle
        UpdateHealthUI();
    }
    // --- DİE FONKSİYONU GÜNCELLENDİ ---
    void Die()
    {
        // --- YENİ EKLENEN SATIR ---
        isDead = true; // Oyuncu öldü olarak işaretle
        // ---
        Debug.LogError("OYUNCU ÖLDÜ!");
        
        // 1. "Oyun Bitti" panelini (varsa) aktifleştir
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        // 2. Oyunu durdur (Tüm fizik ve Update'ler durur)
        Time.timeScale = 0f;

        // 3. Fare imlecini geri getir ve kilidini aç
        // (Game Over ekranındaki butonlara basabilmek için)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Opsiyonel: Oyuncunun hareket etmesini engelle
        // (Eğer PlayerInput component'i varsa devre dışı bırakmak iyi fikirdir)
        // GetComponent<PlayerInput>().enabled = false;
        
        // Opsiyonel: Bu script'i devre dışı bırak
        // this.enabled = false;
    }
}