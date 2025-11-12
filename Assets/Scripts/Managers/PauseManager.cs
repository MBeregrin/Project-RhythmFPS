using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.InputSystem; // <-- Input için GEREKLİ
using UnityEngine.UI; // <-- Slider için GEREKLİ

public class PauseManager : MonoBehaviour
{
    [Header("UI Panel Referansları")]
    [SerializeField] private GameObject pausePanel; // 'Pause_Panel'i buraya sürükle
    [SerializeField] private GameObject settingsPanel; // 'Settings_Panel_InGame'i buraya sürükle

    [Header("Ayarlar Referansları")]
    [SerializeField] private Slider sensitivitySlider; // Settings panelindeki Slider

    // --- INPUT SİSTEMİ ABONELİĞİ ---
    private PlayerInput playerInput;
    private InputAction pauseAction;
    private bool isPaused = false;

    // --- YENİ EKLENDİ ---
    private AudioSource musicAudioSource; // Müziği bulmak için

    // Awake, PlayerInput'u ve 'Pause' eylemini bulur
    void Awake()
    {
        // Bu script'in 'Player' objesinde olduğunu varsayıyoruz
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PauseManager: 'PlayerInput' bileşeni bulunamadı!");
        }
        pauseAction = playerInput.actions["Pause"];
    }

    // OnEnable, eyleme abone olur
    private void OnEnable()
    {
        // 'Pause' tuşuna basıldığında 'TogglePause' fonksiyonunu çağır
        pauseAction.performed += TogglePause;
    }

    // OnDisable, abonelikten çıkar
    private void OnDisable()
    {
        pauseAction.performed -= TogglePause;
    }

    // Start, tüm panellerin kapalı olduğundan emin olur
    void Start()
    {
        // Güvenlik önlemi olarak oyunun başında her şeyi kapat
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f; // Oyunun çalıştığından emin ol
        isPaused = false;
        AudioAnalyzer audioAnalyzer = FindFirstObjectByType<AudioAnalyzer>();
        if (audioAnalyzer != null)
        {
            musicAudioSource = audioAnalyzer.GetComponent<AudioSource>();
        }
    }
    
    // --- ANA KONTROL FONKSİYONU ---

    // 'Esc' tuşuna basıldığında çağrılır
    private void TogglePause(InputAction.CallbackContext context)
    {
        // Eğer zaten 'Game Over' ekranındaysak (PlayerHealth script'i devre dışıysa)
        // duraklatma menüsünü açma. (PlayerHealth'e 'isDead' bayrağı eklemek daha iyi olur)
        
        // (PlayerHealth'teki 'Die()' fonksiyonu Time.timeScale=0f yapıyorsa,
        // bu kodun çalışmaması için 'if (Time.timeScale == 0f && !isPaused) return;'
        // gibi bir kontrol gerekebilir, ama şimdilik basit tutalım)
        
        if (isPaused)
        {
            ResumeGame(); // Zaten durmuşsa, devam ettir
        }
        else
        {
            PauseGame(); // Çalışıyorsa, durdur
        }
    }

    // --- BUTON FONKSİYONLARI ---

    // "ResumeButton" (Devam Et) tarafından çağrılır
    public void ResumeGame()
    {
        Debug.Log("Oyuna Devam Ediliyor...");
        isPaused = false;
        Time.timeScale = 1f; // Zamanı tekrar akıt

        // --- YENİ EKLENDİ ---
        if (musicAudioSource != null) musicAudioSource.UnPause();
        // ---
        
        pausePanel.SetActive(false);
        // ...
        
        // Tüm menüleri kapat
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        
        // Fareyi kilitle ve gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // "SettingsButton" (Ayarlar) tarafından çağrılır
    public void OpenSettings()
    {
        // Güncel kayıtlı ayarı slider'a yükle
        LoadSettings(); 
        settingsPanel.SetActive(true);
        pausePanel.SetActive(false); // Ana pause menüsünü kapat
    }

    // "ReturnToMenuButton" (Ana Menüye Dön) tarafından çağrılır
    public void GoToMainMenu()
    {
        // GameManager'daki fonksiyonu kullanıyoruz,
        // çünkü o Time.timeScale'i 1f'e çekmeyi zaten biliyor.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            // GameManager bulunamazsa (hata durumu)
            Time.timeScale = 1f;
            SceneManager.LoadScene(1); // MainMenu'nün index'i 1 ise
        }
    }

    // --- AYARLAR PANELİ FONKSİYONLARI ---

    // 'Settings_Panel_InGame' içindeki "BackButton" tarafından çağrılır
    public void SaveAndCloseSettings()
    {
        // 1. Ayarı kaydet
        float newSensitivity = sensitivitySlider.value;
        // 'MainMenuManager'daki 'const' anahtarı kullanıyoruz (KOD TUTARLILIĞI)
        PlayerPrefs.SetFloat(MainMenuManager.SENSITIVITY_KEY, newSensitivity);
        PlayerPrefs.Save(); 

        Debug.Log($"Ayarlar kaydedildi: Hassasiyet = {newSensitivity}");

        // 2. Panelleri ayarla (Ayarları kapat, ana Pause menüsünü aç)
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true); 
    }

    // Ayarlar paneli açılırken slider'ın değerini yükler
    private void LoadSettings()
    {
        float savedSens = PlayerPrefs.GetFloat(MainMenuManager.SENSITIVITY_KEY, MainMenuManager.DEFAULT_SENSITIVITY);
        
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSens;
        }
    }
    
    // 'PauseGame()' özel bir fonksiyondur, 'TogglePause' tarafından çağrılır
    private void PauseGame()
    {
        Debug.Log("Oyun Duraklatıldı...");
        isPaused = true;
        Time.timeScale = 0f; // Zamanı durdur

        if (musicAudioSource != null) musicAudioSource.Pause();
        pausePanel.SetActive(true); // Menüyü göster
        
        // Fareyi serbest bırak ve göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}