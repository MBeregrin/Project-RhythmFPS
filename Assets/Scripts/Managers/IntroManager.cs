using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Video; 
using UnityEngine.InputSystem; // <-- 1. YENİ EKLENEN SATIR

[RequireComponent(typeof(VideoPlayer))]
public class IntroManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private VideoPlayer videoPlayer;
    
    [Header("Ayarlar")]
    [SerializeField] private string nextSceneName = "MainMenu"; 

    private bool videoHasStarted = false;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (videoPlayer.clip != null)
        {
            videoPlayer.Play();
            videoHasStarted = true;
        }
        else
        {
            Debug.LogError("IntroManager: Video Clip atanmamış! Ana menüye atlanıyor.");
            LoadMainMenu(); 
        }
    }

    // --- 2. GÜNCELLENEN FONKSİYON ---
    void Update()
    {
        // 'Input.anyKeyDown' (ESKİ SİSTEM) yerine
        // 'Keyboard.current' (YENİ SİSTEM) kullanıyoruz.
        if (videoHasStarted && 
            ( (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
              (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) )
           )
        {
            Debug.Log("Intro atlandı!");
            SkipIntro();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Intro videosu bitti, ana menü yükleniyor...");
        LoadMainMenu();
    }

    public void SkipIntro()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
        LoadMainMenu();
    }
    
    private void LoadMainMenu()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}