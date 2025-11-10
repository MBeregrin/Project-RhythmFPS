using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("Silah Konfigürasyonu")]
    [SerializeField] private WeaponData currentWeapon; 
    [SerializeField] private Transform cameraTransform;
    private float nextFireTime = 0f; 

    [Header("Grenade Ayarları")]
    public GameObject grenadePrefab;
    public float throwForce = 15f;

    // --- YENİ GİRDİ SİSTEMİ DEĞİŞKENLERİ ---
    private PlayerInput playerInput;
    private InputAction fireAction;
    private InputAction grenadeAction;
    // ---

    // YENİ: Awake, PlayerInput ve eylemleri bulur
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerShoot: 'PlayerInput' bileşeni bulunamadı!");
        }

        fireAction = playerInput.actions["Fire"];
        grenadeAction = playerInput.actions["ThrowGrenade"];
    }

    // YENİ: OnEnable, eylemlere abone olur
    private void OnEnable()
    {
        // Bu eylemler "Button" tipi olduğu için sadece 'performed' (basılma anı)
        // olayını dinlememiz yeterli.
        fireAction.performed += HandleFire;
        grenadeAction.performed += HandleThrowGrenade;
    }

    // YENİ: OnDisable, abonelikten çıkar
    private void OnDisable()
    {
        fireAction.performed -= HandleFire;
        grenadeAction.performed -= HandleThrowGrenade;
    }

    // YENİ: HandleFire (Eski 'OnFire' fonksiyonunun yerini aldı)
    private void HandleFire(InputAction.CallbackContext context)
    {
        // 1. Silah atanmış mı?
        if (currentWeapon == null)
        {
            Debug.LogError("PlayerShoot: 'Current Weapon' (Silah) atanmamış!");
            return;
        }

        // 2. Atış hızı (Fire Rate) kontrolü
        if (Time.time >= nextFireTime)
        {
            // 3. Bir sonraki atış zamanını ayarla
            nextFireTime = Time.time + currentWeapon.fireRate;
            
            // 4. Ateş etme fonksiyonunu çağır
            Shoot();
        }
    }

    // Shoot() fonksiyonu aynı kalır (Değişiklik yok)
    private void Shoot()
    {
        // 1. Namlu Ateşi
        if (currentWeapon.muzzleFlashEffect != null)
        {
            currentWeapon.muzzleFlashEffect.Play();
        }

        // 2. Raycast
        RaycastHit hitInfo;
        bool hasHit = Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out hitInfo,
            currentWeapon.fireRange
        );

        // 3. Sonuç
        if (hasHit)
        {
            EnemyHealth targetHealth = hitInfo.transform.GetComponent<EnemyHealth>();
            if (targetHealth != null)
            {
                // Hasar ver
                targetHealth.TakeDamage(
                    currentWeapon.damage,
                    hitInfo.point,
                    Quaternion.LookRotation(hitInfo.normal),
                    hitInfo.collider.name 
                );
            }

            // Çarpma efekti
            if (currentWeapon.impactEffectPrefab != null)
            {
                Instantiate(currentWeapon.impactEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            }
        }
    }
    
    // YENİ: HandleThrowGrenade (Eski 'OnThrowGrenade' fonksiyonunun yerini aldı)
    private void HandleThrowGrenade(InputAction.CallbackContext context)
    {
        // 'context.performed' (basılma anı) zaten kontrol edildi
        if (grenadePrefab != null) 
        {
            ThrowGrenade();
        }
    }

    // ThrowGrenade() fonksiyonu aynı kalır (Değişiklik yok)
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