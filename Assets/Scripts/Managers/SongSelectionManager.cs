using UnityEngine;
using UnityEngine.SceneManagement;

public class SongSelectionManager : MonoBehaviour
{
    [Header("Sahne Ayarları")]
    [SerializeField] private int gameSceneIndex = 3; // Build Settings'teki 'GameScene'in sırası
    [SerializeField] private int mainMenuSceneIndex = 1; // Build Settings'teki 'MainMenu'nun sırası

    // Bu fonksiyonu, 'Şarkı 1' butonuna bağlayacağız
    // 'SongData' asset'ini doğrudan Inspector'dan alacağız
    public void SelectSong(SongData songToPlay)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager bulunamadı!");
            return;
        }

        // 1. Seçilen şarkıyı GameManager'a kaydet
        GameManager.Instance.selectedSong = songToPlay;

        // 2. Oyun sahnesini yükle
        SceneManager.LoadScene(gameSceneIndex);
    }

    // Bu fonksiyonu "Geri" butonuna bağlayacağız
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneIndex);
    }
}