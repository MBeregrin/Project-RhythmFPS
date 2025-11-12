using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panel References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings References")]
    [SerializeField] private Slider sensitivitySlider;

    private PlayerInput playerInput;
    private InputAction pauseAction;
    private bool isPaused = false;
    private AudioSource musicAudioSource;
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PauseManager: 'PlayerInput' component not found!");
        }
        pauseAction = playerInput.actions["Pause"];
    }
    private void OnEnable()
    {
        pauseAction.performed += TogglePause;
    }
    private void OnDisable()
    {
        pauseAction.performed -= TogglePause;
    }
    void Start()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f; 
        isPaused = false;
        AudioAnalyzer audioAnalyzer = FindFirstObjectByType<AudioAnalyzer>();
        if (audioAnalyzer != null)
        {
            musicAudioSource = audioAnalyzer.GetComponent<AudioSource>();
        }
    }
    private void TogglePause(InputAction.CallbackContext context)
    {  
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (musicAudioSource != null) musicAudioSource.UnPause();
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void OpenSettings()
    {
        LoadSettings(); 
        settingsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }
    public void GoToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(1);
        }
    }
    public void SaveAndCloseSettings()
    {
        float newSensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat(MainMenuManager.SENSITIVITY_KEY, newSensitivity);
        PlayerPrefs.Save(); 
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true); 
    }
    private void LoadSettings()
    {
        float savedSens = PlayerPrefs.GetFloat(MainMenuManager.SENSITIVITY_KEY, MainMenuManager.DEFAULT_SENSITIVITY);
        
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSens;
        }
    }
    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (musicAudioSource != null) musicAudioSource.Pause();
        pausePanel.SetActive(true); 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}