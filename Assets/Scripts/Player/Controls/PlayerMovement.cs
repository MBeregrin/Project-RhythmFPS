using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;
using System.Collections; 
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction meleeAction;

    [Header("Movement Settings")]
    [SerializeField]private float walkSpeed = 5f; 
    [SerializeField]private float sprintSpeed = 10f; 
    [Header("Jump Settings")]
    [SerializeField]private float jumpForce = 5f;     
    [SerializeField] private float fallMultiplier = 2.5f; 
    [SerializeField] private float lowJumpMultiplier = 2f;
    private bool isJumpHeld = false;   
    [Header("Footstep Sound Settings")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepDelay = 0.4f;
    private float footstepTimer = 0f;
    private AudioSource audioSource;

    [Header("Ground Check")]
    public LayerMask groundLayer; 
    public Transform groundCheck;  
    private float groundDistance = 0.4f; 

    [Header("Stamina Settings")]
    [SerializeField]private float maxStamina = 100f;
    [SerializeField]private float currentStamina = 100f;
    [SerializeField]private float staminaConsumptionRate = 30f; 
    [SerializeField]private float staminaRegenRate = 15f;     
    [SerializeField]private float jumpStaminaCost = 10f; 
    [SerializeField]private float dashStaminaCost = 25f; 
    
    private bool canSprint = true;
    private bool isDashing = false;

    [Header("Melee Settings")]
    public float meleeRange = 1.5f;
    public float meleeStaminaCost = 15f;
    public int meleeDamage = 10;
    public float meleePushForce = 5f;
    private bool isMeleeAttacking = false;    

    [Header("UI References")]
    public Image staminaBarImage; 
    private Vector2 moveInput; 
    private Rigidbody rb; 
    private bool isSprinting = false; 

    [Header("Dash Settings")]
    [SerializeField]private float dashForce = 20f;      
    [SerializeField]private float dashDuration = 0.2f;  

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>(); 
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (playerInput == null)
        {
            return;
        }
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        meleeAction = playerInput.actions["Melee"];
    }

    private void OnEnable()
    {
        moveAction.performed += HandleMove;
        moveAction.canceled += HandleMove; 
        sprintAction.performed += HandleSprintPerformed;
        sprintAction.canceled += HandleSprintCanceled;
        jumpAction.performed += HandleJumpPerformed; 
        jumpAction.canceled += HandleJumpCanceled;   
        dashAction.performed += HandleDash;
        meleeAction.performed += HandleMelee;
    }
    private void OnDisable()
    {
        moveAction.performed -= HandleMove;
        moveAction.canceled -= HandleMove;
        sprintAction.performed -= HandleSprintPerformed;
        sprintAction.canceled -= HandleSprintCanceled;
        jumpAction.performed -= HandleJumpPerformed;
        jumpAction.canceled -= HandleJumpCanceled;
        dashAction.performed -= HandleDash;
        meleeAction.performed -= HandleMelee;
    }
    private void Update()
    {
        HandleStamina();
        UpdateStaminaUI(); 
    }
 
    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void HandleSprintPerformed(InputAction.CallbackContext context)
    {
        isSprinting = true;
    }

    private void HandleSprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }
    private void HandleJumpPerformed(InputAction.CallbackContext context)
    {
        if (IsGrounded() && !isDashing && currentStamina >= jumpStaminaCost) 
        {
            currentStamina -= jumpStaminaCost;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); 
            canSprint = true;
            isJumpHeld = true;
        }
    }
    private void HandleJumpCanceled(InputAction.CallbackContext context)
    {
        isJumpHeld = false;
    }

    private void HandleDash(InputAction.CallbackContext context)
    {
        if (!isDashing && currentStamina >= dashStaminaCost)
        {
            StartCoroutine(PerformDash());
        }
    }
private void HandleMelee(InputAction.CallbackContext context)
{
    if (isMeleeAttacking)
    {
        return;
    }

    if (currentStamina < meleeStaminaCost)
    {
        return;
    }
    isMeleeAttacking = true; 
    StartCoroutine(PerformMelee());
}
    private IEnumerator PerformDash()
    {
        isDashing = true;
        currentStamina -= dashStaminaCost; 
        canSprint = true; 
        Vector3 dashDirection = (moveInput.magnitude > 0) ? (transform.forward * moveInput.y + transform.right * moveInput.x).normalized : transform.forward;
        dashDirection.y = 0; 
        rb.linearVelocity = Vector3.zero; 
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange); 
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.1f, rb.linearVelocity.y, rb.linearVelocity.z * 0.1f);
    }
    private void HandleStamina()
    {
        if (isDashing || isMeleeAttacking) return; 

        bool isMoving = moveInput.magnitude > 0;
        bool isTryingToSprint = isSprinting && isMoving;

        if (isTryingToSprint && canSprint)
        {
            currentStamina -= staminaConsumptionRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false; 
            }
        }
        else if (currentStamina < maxStamina && !isDashing) 
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina); 
            if (!canSprint && currentStamina > maxStamina * 0.1f)
            {
                canSprint = true;
            }
        }
    }
    private void UpdateStaminaUI()
    {
        if (staminaBarImage == null) return; 
        staminaBarImage.fillAmount = currentStamina / maxStamina;
    }
    private bool IsGrounded()
    {
        if (groundCheck == null) return true; 
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer); 
    }
    private void FixedUpdate()
    {
        if (isDashing) return;
        if (moveInput.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
        else
        {
            float currentSpeed = (isSprinting && canSprint && moveInput.magnitude > 0)? sprintSpeed : walkSpeed;
            Vector3 localMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            Vector3 worldMoveDirection = (transform.forward * localMoveDirection.z) + (transform.right * localMoveDirection.x);
            worldMoveDirection.y = 0;
            Vector3 newPosition = rb.position + worldMoveDirection * currentSpeed * Time.fixedDeltaTime; 
            rb.MovePosition(newPosition);
        }
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isJumpHeld)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        HandleFootsteps();
    }

    private IEnumerator PerformMelee()
    {

        currentStamina -= meleeStaminaCost;

        List<EnemyHealth> enemiesHit = new List<EnemyHealth>();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, meleeRange);

        foreach (Collider enemyCollider in hitColliders)
        {
            EnemyHealth enemyHealth = enemyCollider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null && !enemiesHit.Contains(enemyHealth))
            {
                enemiesHit.Add(enemyHealth);
                Rigidbody enemyRb = enemyHealth.GetComponentInParent<Rigidbody>();
                enemyHealth.TakeDamage(meleeDamage, enemyCollider.transform.position, Quaternion.identity, "Torso");
                if (enemyRb != null)
                {
                    Vector3 pushDirection = (enemyCollider.transform.position - transform.position).normalized;
                    pushDirection.y = 0.5f;
                    enemyRb.AddForce(pushDirection * meleePushForce, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(0.4f);
        isMeleeAttacking = false;
    }
private void HandleFootsteps()
    {
        if (footstepSounds.Length == 0 || audioSource == null) return;
        if (footstepTimer > 0)
        {
            footstepTimer -= Time.fixedDeltaTime;
        }
        bool isMoving = moveInput.magnitude > 0.1f;
        if (isMoving && IsGrounded() && !isDashing)
        {
            if (footstepTimer <= 0f)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.PlayOneShot(clip, 0.3f);
                footstepTimer = isSprinting ? (footstepDelay / 1.5f) : footstepDelay;
            }
        }
    }
}