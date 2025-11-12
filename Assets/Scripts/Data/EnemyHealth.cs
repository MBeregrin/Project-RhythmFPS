using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    private int maxHealth = 100; 
    private int currentHealth; 
    private float originalMoveSpeed;
    private EnemyAI enemyAI;
    private const float HeadshotMultiplier = 2.0f;
    private const float TorsoMultiplier = 1.0f;
    private const float ArmLegMultiplier = 0.6f; 
    [Header("Enemy Sounds")]
    [SerializeField] private AudioClip hitSound; 
    [SerializeField] private AudioClip deathSound; 
    [SerializeField] private AudioSource audioSource; 
    
    [Header("Item Drop Settings")]
    [SerializeField] private GameObject healthPickupPrefab; 
    [SerializeField] private GameObject ammoPickupPrefab;   
    [Tooltip("%change drop item")]
    [Range(0, 100)]
    [SerializeField] private float dropChance = 50f;
    [Header("Visual Effects")]
    [SerializeField]private GameObject bloodVFXPrefab;
    private void Awake()
    {
        currentHealth = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            originalMoveSpeed = enemyAI.moveSpeed;
        }
    }
    public void Initialize(int startingHealth)
    {
        maxHealth = startingHealth;
        currentHealth = maxHealth;
    }
    public void TakeDamage(int baseDamageAmount, Vector3 hitPoint, Quaternion hitRotation, string hitColliderName)
    {
      if (currentHealth <= 0) return;
        float damageMultiplier = 1.0f;
        string hitNameLower = hitColliderName.ToLower();
        if (hitNameLower.Contains("head"))
        {
            damageMultiplier = HeadshotMultiplier;
            Debug.Log("CRITICAL! HEADSHOT");
        }
        else if (hitNameLower.Contains("torso") || hitNameLower.Contains("chest") || hitNameLower.Contains("stomach"))
        {
            damageMultiplier = TorsoMultiplier;
            Debug.Log("Standart Damage");
        }
        else if (hitNameLower.Contains("arm") || hitNameLower.Contains("hand") || hitNameLower.Contains("leg") || hitNameLower.Contains("foot"))
        {
            damageMultiplier = ArmLegMultiplier;
            Debug.Log("Low Damage");
        }
        int finalDamage = Mathf.RoundToInt(baseDamageAmount * damageMultiplier);
        currentHealth -= finalDamage;

        Debug.Log($"{transform.name} damaged. Damage: {finalDamage}. Current Health: {currentHealth}"); 
if (hitSound != null && audioSource != null)
{
    audioSource.PlayOneShot(hitSound);
}
        if (bloodVFXPrefab!= null)
        {
            Instantiate(bloodVFXPrefab, hitPoint, hitRotation); 
        }
        ApplySlowdown();
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void ApplySlowdown()
    {
        if (enemyAI!= null && originalMoveSpeed > 0)
        {
            float healthRatio = Mathf.Max(0, (float)currentHealth / maxHealth); 
            enemyAI.moveSpeed = originalMoveSpeed * healthRatio;
        }
    }
    void Die()
    {
        Debug.Log(transform.name + " destroyed!");

        if (GameManager.Instance != null)
        {
            const int BaseScoreValue = 100;
            GameManager.Instance.EnemyKilled(BaseScoreValue);
        }
        float randomValue = Random.Range(0f, 100f);
        if (randomValue <= dropChance)
        {
            GameObject prefabToDrop = (Random.value > 0.5f) ? healthPickupPrefab : ammoPickupPrefab;
            if (prefabToDrop != null)
            {
                Vector3 spawnPosition = transform.position + (Vector3.up * 0.5f);
                Instantiate(prefabToDrop, spawnPosition, Quaternion.identity);
            }
        }
        if (deathSound != null)
    {
        AudioSource.PlayClipAtPoint(deathSound, transform.position);
    }
        Destroy(gameObject); 
    }
}