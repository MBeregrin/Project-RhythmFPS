using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("Silah Konfigürasyonu")]
    // YENİ: Oyuncunun o an elinde tuttuğu silahın "Veri Kartı"
    [SerializeField] private WeaponData currentWeapon; 
    
    [SerializeField] private Transform cameraTransform;

    // YENİ: Atış hızı (fire rate) kontrolü için bir sayaç
    private float nextFireTime = 0f; 

    [Header("Grenade Ayarları")]
    // (Grenade kısmı aynı kalıyor)
    public GameObject grenadePrefab;
    public float throwForce = 15f;
    

    public void OnFire(InputValue value)
    {
        // 1. Silah atanmış mı?
        if (currentWeapon == null)
        {
            Debug.LogError("PlayerShoot: 'Current Weapon' (Silah) atanmamış!");
            return;
        }

        // 2. Atış hızı (Fire Rate) kontrolü
        // Şu anki oyun zamanı, bir sonraki ateş etme zamanından büyük veya eşitse ateş et
        if (Time.time >= nextFireTime)
        {
            // 3. Bir sonraki atış zamanını ayarla
            // (Örn: Time.time (10.0) + currentWeapon.fireRate (0.5) = 10.5)
            // Yani, 10.5 saniyesine kadar tekrar ateş edilemez.
            nextFireTime = Time.time + currentWeapon.fireRate;
            
            // 4. Ateş etme fonksiyonunu çağır
            Shoot();
        }
    }

    private void Shoot()
    {
        // 1. Namlu Ateşi (Efekti Silahtan Oku)
        if (currentWeapon.muzzleFlashEffect != null)
        {
            currentWeapon.muzzleFlashEffect.Play();
        }

        // 2. Işın Gönderme (Raycast) - Veriler WeaponData'dan okunuyor
        RaycastHit hitInfo;
        bool hasHit = Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out hitInfo,
            currentWeapon.fireRange // Menzili silahtan oku
        );

        // 3. Sonucu Değerlendirme
        if (hasHit)
        {
            // Düşmanı vurduk mu?
            EnemyHealth targetHealth = hitInfo.transform.GetComponent<EnemyHealth>();

            if (targetHealth != null)
            {
                // Hasar ver
                targetHealth.TakeDamage(
                    currentWeapon.damage, // Hasarı silahtan oku
                    hitInfo.point,
                    Quaternion.LookRotation(hitInfo.normal),
                    hitInfo.collider.name 
                );
            }

            // Çarpma efekti (Impact Effect) oluştur (Efekti silahtan oku)
            if (currentWeapon.impactEffectPrefab != null)
            {
                Instantiate(currentWeapon.impactEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            }
        }
    }
    
    // --- GRENADE KISMI (Değişiklik yok) ---
    public void OnThrowGrenade(InputValue value)
    {
        if (value.isPressed && grenadePrefab != null) 
        {
            ThrowGrenade();
        }
    }

    private void ThrowGrenade()
    {
        Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * 0.5f;

        GameObject grenade = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();

        if (grenadeRb != null)
        {
            grenadeRb.AddForce(cameraTransform.forward * throwForce, ForceMode.VelocityChange);
        }
    }
}