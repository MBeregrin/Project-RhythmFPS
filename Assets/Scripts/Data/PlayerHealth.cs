using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health Settings")]
    [SerializeField] private int maxHealth = 100;
    public int currentHealth;

    [Header("UI References")]
    public TextMeshProUGUI healthText;
    [HideInInspector] public bool isDead = false;
    [SerializeField] private GameObject gameOverScreen; 
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false); 
        }
        Time.timeScale = 1f; 
    }
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; 

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        Debug.Log($"Player took damage. Current health: {currentHealth}");
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
            healthText.text = $"Health: {currentHealth}";
        }
    }
    public void Heal(int healAmount)
    {
        if (currentHealth >= maxHealth) return;
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"Player healed: Current health = {currentHealth}");
        UpdateHealthUI();
    }
    void Die()
    {
        isDead = true;
        // ---
        Debug.LogError("DEAD.");
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        this.enabled = false;
    }
    public void Call_RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogError("RestartButton: GameManager not found!");
        }
    }
}