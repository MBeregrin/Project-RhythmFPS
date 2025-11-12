using UnityEngine;

// Bu satır, Project penceresinde sağ tıklayarak yeni bir SongData varlığı oluşturmanızı sağlar.

// --- BU SATIRI EKLE ---
[CreateAssetMenu(fileName = "Yeni Şarkı", menuName = "Rhythm Game/Yeni Şarkı")]

public class SongData : ScriptableObject
{
    [Header("Müzik Kaynağı")]

    public AudioClip songClip; // Müzik dosyası

    public float songLength;   // Şarkının toplam süresi (opsiyonel)



    public float spawnThresholdOverride = 10f; // Bu şarkıya özel eşik değeri

    public float spawnCooldownOverride = 0.5f; // Bu şarkıya özel BPM/Ritmi (Örn: 120 BPM için 0.5f)

    [Header("Zorluk Ayarları")]

    public int baseEnemyHealth = 100; // Bu şarkıdaki düşmanların canı (isteğe bağlı)

    public GameObject easyEnemyPrefab; // Bu şarkıya özel kolay düşman (Enemy prefab'ınız)

    public GameObject hardEnemyPrefab; // Bu şarkıya özel zor düşman
}