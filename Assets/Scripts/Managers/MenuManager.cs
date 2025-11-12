using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; // <-- Slider için bu GEREKLİ
// using TMPro; // <-- Artık Text göstermediğimiz için buna GEREK YOK

public class MainMenuManager : MonoBehaviour
{
    [Header("Sahne Ayarları")]
    [SerializeField] private int songSelectionSceneIndex = 2; 
    
    [Header("Ayarlar Paneli Referansları")]
    [SerializeField] private GameObject settingsPanel; // Hierarchy'den 'Settings_Panel'i buraya sürükle
    
    [Header("Hassasiyet Ayarları")]
    [SerializeField] private Slider sensitivitySlider; // 'Sensitivity_Slider'ı buraya sürükle
    
    // Bu 'string' (metin), ayarı kaydederken kullanacağımız "anahtardır".
    public const string SENSITIVITY_KEY = "MouseSensitivity";
    public const float DEFAULT_SENSITIVITY = 100f;

    private void Start()
    {
        // Başlangıçta ayarlar panelinin kapalı olduğundan emin ol
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Kayıtlı ayarları yükle
        LoadSettings();
    }

    // --- MEVCUT BUTON FONKSİYONLARI ---

    public void GoToSongSelection()
    {
        SceneManager.LoadScene(songSelectionSceneIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan Çıkılıyor...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // --- GÜNCELLENMİŞ AYARLAR FONKSİYONLARI ---

    // Bu, "Settings" butonuna basıldığında çalışır
    public void GoToSettings()
    {
        if (settingsPanel != null)
        {
            // Paneli açmadan önce, slider'ın GÜNCEL kayıtlı değeri göstermesini sağla
            LoadSettings(); 
            settingsPanel.SetActive(true);
        }
    }

    // Bu, "BackButton"a basıldığında çalışır
    public void SaveAndCloseSettings()
    {
        // 1. Ayarı kaydet
        // Slider'ın O ANKİ değerini al
        float newSensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, newSensitivity);
        PlayerPrefs.Save(); // Kaydı diske yaz

        Debug.Log($"Ayarlar kaydedildi: Hassasiyet = {newSensitivity}");

        // 2. Paneli kapat
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    // Bu, menü ilk açıldığında (veya Ayarlar'a girerken) çalışır
    private void LoadSettings()
    {
        // 'PlayerPrefs'ten kayıtlı hassasiyeti yükle.
        float savedSens = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENSITIVITY);
        
        // Slider'ı bu kayıtlı değere ayarla
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSens;
        }
    }
}