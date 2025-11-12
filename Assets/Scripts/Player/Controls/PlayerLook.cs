using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public const string SENSITIVITY_KEY = "MouseSensitivity";
    public const float DEFAULT_SENSITIVITY = 100f;
    // ---
    [Header("Settings")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;
    private PlayerInput playerInput;
    private InputAction lookAction;
    // ---

    private Vector2 lookInput;
    private float xRotation = 0f;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>(); 
        if (playerInput == null)
        {
            Debug.LogError("PlayerLook: 'PlayerInput' compenent not found!");
        }
        lookAction = playerInput.actions["Look"];
    }
    private void OnEnable()
    {
        lookAction.performed += HandleLook;
        lookAction.canceled += HandleLook;
    }
    private void OnDisable()
    {
        lookAction.performed -= HandleLook;
        lookAction.canceled -= HandleLook;
    }
    private void Start()
    {
        mouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENSITIVITY);
        // ---
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void HandleLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    private void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY; 
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}