using UnityEngine;
using UnityEngine.SceneManagement;

public class SongSelectionManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private int gameSceneIndex = 3;
    [SerializeField] private int mainMenuSceneIndex = 1;
    public void SelectSong(SongData songToPlay)
    {
        if (GameManager.Instance == null)
        {
            return;
        }
        GameManager.Instance.selectedSong = songToPlay;
        SceneManager.LoadScene(gameSceneIndex);
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneIndex);
    }
}