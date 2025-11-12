using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 3f; 
    public float attackRange = 2f;      
    public float attackCooldown = 2f;   
    public int attackDamage = 10;       
    private Transform playerTransform; 
    private PlayerHealth playerHealth;  
    private Rigidbody rb; 
    private Animator animator;
    private float currentCooldown = 0f;
    private bool canMove = true;
    private bool playerIsAlive = true;
    void Awake() 
    {
        rb = GetComponent<Rigidbody>(); 
    }
    void Start() 
{
    playerHealth = FindFirstObjectByType<PlayerHealth>(); 
    if (playerHealth != null)
    {
        playerTransform = playerHealth.transform; 
    }
    else
    {
        Debug.LogError("Player is not in Scene.");
    }
}
    void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }
        if (playerTransform == null || playerHealth == null || playerHealth.currentHealth <= 0)
        {
            if (playerIsAlive) 
            {
                playerIsAlive = false;
                canMove = false; 
            }
            return;
        }
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange)
        {
            canMove = false; 
            TryAttack();    
        }
        else
        { 
            canMove = true; 
        }
        Vector3 lookDirection = playerTransform.position - transform.position;
        lookDirection.y = 0; 
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
    void FixedUpdate()
    {
        if (playerTransform == null || !playerIsAlive)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
            return;
        }
        if (canMove)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
    void MoveTowardsPlayer()
    {
        Vector3 targetDirection = playerTransform.position - transform.position;
        targetDirection.y = 0;
        Vector3 moveDirection = targetDirection.normalized;
        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; 
        rb.linearVelocity = targetVelocity; 
    }
    void TryAttack()
    {
        if (currentCooldown <= 0f)
        {
            currentCooldown = attackCooldown;
            Debug.Log("EnemyAI: Attacking!");
            AnimationTrigger_DealDamage();
        }
    }
    public void AnimationTrigger_DealDamage()
    {  
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange * 1.2f) // (Tolerans payı)
        {
            Debug.Log("EnemyAI: Attacked to Player");
            playerHealth.TakeDamage(attackDamage);
        }
        else
        {
            Debug.Log("EnemyAI: Attack missed.)");
        }
    }
}