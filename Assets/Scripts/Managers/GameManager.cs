using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } 
    
    [Header("Game Settings")]
    public SongData selectedSong; 
    private int currentScore = 0;
    private int currentCombo = 0;
    public float comboResetTime = 2f; 
    private float comboTimer;

    [Header("UI Panel References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI scoreText; 
    [SerializeField] private TextMeshProUGUI comboText; 

    private PlayerHealth playerHealth;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 3) //(0=Intro, 1=MainMenu, 2=SongSel, 3=GameScene)
        {
            FindUIReferences();
            ResetGameStats();
        }
        else
        {
            scoreText = null;
            comboText = null;
            victoryPanel = null;
            finalScoreText = null;
            playerHealth = null;
        }
    }
    private void FindUIReferences()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        try
        {
            scoreText = GameObject.Find("Score_Text").GetComponent<TextMeshProUGUI>();
            comboText = GameObject.Find("Combo_Text").GetComponent<TextMeshProUGUI>();
            Canvas gameCanvas = FindFirstObjectByType<Canvas>();
            if (gameCanvas != null)
            {
                victoryPanel = gameCanvas.transform.Find("Victory_Panel")?.gameObject; 
                if (victoryPanel != null)
                {
                    finalScoreText = victoryPanel.transform.Find("FinalSkor_Text").GetComponent<TextMeshProUGUI>();
                    victoryPanel.SetActive(false); 
                }
            }
            if (scoreText == null || comboText == null)
            {
                Debug.LogError("GameManager: 'Score_Text' or 'Combo_Text' not found!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameManager: Object Names Error: {e.Message}");
        }
    }
    void Start()
    {

    }

    private void ResetGameStats()
    {
        currentScore = 0;
        currentCombo = 0;
        comboTimer = comboResetTime;
        UpdateUI();
    }
    private void Update()
    {
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime; 
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }
    public void EnemyKilled(int baseScore)
    {
        currentCombo++;
        comboTimer = comboResetTime;

        int scoreGained = baseScore * currentCombo;
        currentScore += scoreGained;

        UpdateUI();

        Debug.Log($"SCORE: +{scoreGained} (Combo x{currentCombo}) | Total Score: {currentScore}");
    }

    void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"COMBO FINISHED Current Combo: {currentCombo}");
        }
        currentCombo = 0;
        UpdateUI();
    }
    void UpdateUI()
    {
        if (scoreText != null) 
        {
            scoreText.text = $"SCORE: {currentScore}";
        }

        if (comboText != null)
        {
            if (currentCombo > 1)
            {
                comboText.text = $"COMBO X{currentCombo}";
            }
            else
            {
                comboText.text = ""; 
            }
        }
    }
    public void PlayerTookDamage(int damage)
    {
        ResetCombo(); 
    }
    public void TriggerVictory()
    {
        if (playerHealth != null && playerHealth.isDead)
        {
            return;
        }
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (finalScoreText != null)
            {
                finalScoreText.text = $"SCORE: {currentScore}";
            }
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            AudioSource musicAudioSource = FindFirstObjectByType<AudioAnalyzer>()?.GetComponent<AudioSource>();
            if (musicAudioSource != null) musicAudioSource.Stop();
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }
}