using UnityEngine;

// Bu satır, Project panelinde sağ tıklayarak yeni Silah Verileri oluşturmanı sağlar.
[CreateAssetMenu(fileName = "New Weapon", menuName = "Rhythm Game/New Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Silah Bilgileri")]
    public string weaponName = "Tabanca"; // Silahın adı (UI için)

    [Header("Ateş Etme Ayarları")]
    public int damage = 10;                // Merminin vereceği hasar
    public float fireRange = 100f;         // Merminin menzili
    public float fireRate = 0.5f;          // İki atış arası bekleme süresi (saniye)

    [Header("Görsel Efektler")]
    // PlayerShoot'taki referansları buraya taşıyoruz
    public ParticleSystem muzzleFlashEffect; // Namlu ateşi efekti
    public GameObject impactEffectPrefab;  // Mermi izi (çarpma) efekti
}