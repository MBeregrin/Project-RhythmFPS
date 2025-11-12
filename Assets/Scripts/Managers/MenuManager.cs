using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; // <-- Slider için bu GEREKLİ
// using TMPro; // <-- Artık Text göstermediğimiz için buna GEREK YOK

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private int songSelectionSceneIndex = 2; 
    
    [Header("Settings Panel References")]
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Sensitivity Settings")]
    [SerializeField] private Slider sensitivitySlider;
    
    public const string SENSITIVITY_KEY = "MouseSensitivity";
    public const float DEFAULT_SENSITIVITY = 100f;

    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        LoadSettings();
    }

    public void GoToSongSelection()
    {
        SceneManager.LoadScene(songSelectionSceneIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    public void GoToSettings()
    {
        if (settingsPanel != null)
        {
            LoadSettings(); 
            settingsPanel.SetActive(true);
        }
    }
    public void SaveAndCloseSettings()
    {
        float newSensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, newSensitivity);
        PlayerPrefs.Save(); 
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    private void LoadSettings()
    {
        float savedSens = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENSITIVITY);
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSens;
        }
    }
}