using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Video; 
using UnityEngine.InputSystem; // <-- 1. YENİ EKLENEN SATIR

[RequireComponent(typeof(VideoPlayer))]
public class IntroManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    
    [Header("Settings")]
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
            LoadMainMenu(); 
        }
    }
    void Update()
    {
        if (videoHasStarted && 
            ( (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
              (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) )
           )
        {
            SkipIntro();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
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