using UnityEngine;
using TMPro; // UI için TextMeshPro kullanacağımızı varsayıyoruz (Daha modern)
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // --- SINGLETON TANIMLAMA ---
    // Bu, diğer script'lerin bu sınıfa kolayca erişmesini sağlar (GameManager.Instance.Skor gibi).
    public static GameManager Instance { get; private set; } 
    // ---------------------------
    // --- YENİ EKLENEN SATIR ---
    [Header("Oyun Ayarları")]
    public SongData selectedSong; // Seçilen şarkıyı sahneler arası taşımak için
   
   
    private int currentScore = 0;
   
    private int currentCombo = 0;
   
    public float comboResetTime = 2f; // Kombonun kaç saniye içinde sıfırlanacağı.

    private float comboTimer;
    
    // --- YENİ EKLENEN DEĞİŞKENLER ---
    [Header("UI Panel Referansları")]
    [SerializeField] private GameObject victoryPanel; // 'Victory_Panel'i buraya sürükle
    [SerializeField] private TextMeshProUGUI finalScoreText; // 'FinalSkor_Text'i buraya sürükle
    
    // (PlayerHealth'i bulmak için referans)
    private PlayerHealth playerHealth;
    // ---

   
   
    public TextMeshProUGUI scoreText; // Skoru gösterecek UI Text bileşeni
   
    public TextMeshProUGUI comboText; // Kombo sayısını gösterecek UI Text bileşeni

    // Awake, Start'tan önce çalışır ve Singleton kurulumu için idealdir.
    private void Awake()
    {
        // Singleton Kontrolü: Sahnede bu objeden sadece bir tane olduğundan emin ol.
        if (Instance!= null && Instance!= this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // Sahne değiştikten sonra bile GameManager'ın yok olmamasını sağla (Multiplayer için kritik).
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Start
    void Start()
    {
        currentScore = 0;
        currentCombo = 0;
        comboTimer = comboResetTime;
        UpdateUI();
        // Oyuncuyu bul ve ölme ihtimaline karşı referansını al
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        
        // Zafer ekranını oyunun başında kapat
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    // Update, kombo zamanlayıcısını yönetir.
    private void Update()
    {
        // Eğer kombo sayacı sıfırdan büyükse
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime; 
            if (comboTimer <= 0)
            {
                // Kombo süresi doldu, sıfırla.
                ResetCombo();
            }
        }
    }

    // --- DIŞARIDAN ÇAĞRILACAK ANA FONKSİYON ---
    // EnemyHealth.cs'in Die() fonksiyonundan burayı çağıracağız.
    public void EnemyKilled(int baseScore)
    {
        // Kombo sayısını artır ve zamanlayıcıyı yenile.
        currentCombo++;
        comboTimer = comboResetTime;

        // Puan hesaplaması: Taban puan * Kombo çarpanı
        // (Kombo çarpanı, 1'den başlar: 1, 2, 3, 4,... gibi)
        int scoreGained = baseScore * currentCombo;
        currentScore += scoreGained;

        // UI'ı güncelle
        UpdateUI();

        // Konsol Log'u
        Debug.Log($"SKOR: +{scoreGained} (Kombo x{currentCombo}) | Toplam Skor: {currentScore}");
    }

    void ResetCombo()
    {
        if (currentCombo > 0)
        {
            Debug.Log($"KOMBO BİTTİ! Kombo Sayısı: {currentCombo}");
        }
        currentCombo = 0;
        UpdateUI();
    }

    // Arayüzü güncelleyen merkezi fonksiyon.
    void UpdateUI()
    {
        if (scoreText!= null)
        {
            scoreText.text = $"SKOR: {currentScore}";
        }

        if (comboText!= null)
        {
            if (currentCombo > 1)
            {
                // Kombo aktifse sayıyı ve çarpanı göster
                comboText.text = $"KOMBO X{currentCombo}";
            }
            else
            {
                // Kombo yoksa boş göster
                comboText.text = ""; 
            }
        }
    }

    // Oyuncunun hasar aldığında çağıracağı fonksiyon (İleride kullanılacak)
    public void PlayerTookDamage(int damage)
    {
        ResetCombo(); // DOOM kuralı: Hasar alırsan kombon sıfırlanır!
    }
    public void RestartGame()
    {
        // 1. Oyunu tekrar çalışır hale getir (zamanı 1'e ayarla)
        Time.timeScale = 1f;

        // 2. O an aktif olan sahneyi (level'ı) yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    // --- BU, PAUSEMANAGER'IN ARADIĞI EKSİK FONKSİYON ---
    public void ReturnToMainMenu()
    {
        // 1. Oyunu tekrar çalışır hale getir (zamanı 1'e ayarla)
        Time.timeScale = 1f;

        // 2. "MainMenu" sahnesini yükle (Index 1)
        // (Build Settings: 0=Intro, 1=MainMenu)
        SceneManager.LoadScene(1);
        // Veya adıyla: SceneManager.LoadScene("MainMenu");
    }
    public void TriggerVictory()
    {
        // 1. Oyuncu ölmüş mü diye kontrol et
        if (playerHealth != null && playerHealth.isDead)
        {
            Debug.Log("Şarkı bitti ama oyuncu ölü, zafer ekranı gösterilmedi.");
            return; // Oyuncu ölmüşse, "Game Over" ekranı kalır, zafer ekranı açılmaz.
        }

        Debug.Log("ZAFER! Şarkı bitti.");

        // 2. Zafer ekranı atanmış mı?
        if (victoryPanel != null)
        {
            // 3. Paneli aç
            victoryPanel.SetActive(true);

            // 4. Skoru güncelle
            if (finalScoreText != null)
            {
                finalScoreText.text = $"SKOR: {currentScore}";
            }

            // 5. Oyunu durdur
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // (Müziği de durduralım)
            AudioSource musicAudioSource = FindFirstObjectByType<AudioAnalyzer>()?.GetComponent<AudioSource>();
            if (musicAudioSource != null) musicAudioSource.Stop();
        }
    }
}