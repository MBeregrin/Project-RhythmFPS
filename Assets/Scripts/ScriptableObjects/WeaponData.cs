using UnityEngine;

// Bu satır, Project panelinde sağ tıklayarak yeni Silah Verileri oluşturmanı sağlar.
[CreateAssetMenu(fileName = "New Weapon", menuName = "Rhythm Game/New Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Silah Bilgileri")]
    public string weaponName = "Tabanca"; // Silahın adı (UI için)
    public GameObject weaponModelPrefab; // Buraya silahın 3D model prefab'ını sürükleyeceğiz

    [Header("Ateş Etme Ayarları")]
    public int damage = 10;                // Merminin vereceği hasar
    public float fireRange = 100f;         // Merminin menzili
    public float fireRate = 0.5f;          // İki atış arası bekleme süresi (saniye)
    public AudioClip fireSound; // Ateş etme sesi

    // --- YENİ EKLENEN MERMİ AYARLARI ---
    [Header("Mermi Ayarları")]
    public int magazineSize = 10;     // Şarjör kapasitesi (örn: 10)
    public int startingAmmo = 30;     // Başlangıçta verilecek yedek mermi (örn: 30)
    public float reloadTime = 1.5f;   // Şarjör değiştirme süresi (saniye)
    // ---

    [Header("Görsel Efektler")]
    // PlayerShoot'taki referansları buraya taşıyoruz
    public ParticleSystem muzzleFlashEffect; // Namlu ateşi efekti
    public GameObject impactEffectPrefab;  // Mermi izi (çarpma) efekti
    // --- YENİ EKLENEN SATIR ---
    public AudioClip reloadSound; // Şarjör değiştirme sesi
    // ---
}