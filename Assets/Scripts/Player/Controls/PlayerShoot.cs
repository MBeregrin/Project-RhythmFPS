using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Coroutine (Reload) için bu gerekli
using TMPro; // Mermi sayısını göstermek için bu gerekli

public class PlayerShoot : MonoBehaviour
{
    [Header("Silah Konfigürasyonu")]
    [SerializeField] private WeaponData currentWeapon; 
    [SerializeField] private Transform cameraTransform;

    [Header("Silah Tutucu")]
    [SerializeField] private Transform weaponHolder;
    private GameObject currentWeaponModel; 
    
    // --- YENİ EKLENDİ (Mermi İzi) ---
    [Header("Mermi İzi Ayarları")]
    [SerializeField] private LineRenderer bulletTrail; // Mermi izi için LineRenderer
    [SerializeField] private float trailDuration = 0.05f; // İz ekranda kaç saniye kalacak
    // ---

    [Header("Grenade Ayarları")]
    public GameObject grenadePrefab;
    public float throwForce = 15f;

    // --- YENİ MERMİ SİSTEMİ DEĞİŞKENLERİ ---
    private int currentAmmoInMag; // Şarjördeki mermi
    private int currentReserveAmmo; // Cepteki yedek mermi
    private bool isReloading = false; // Şarjör değiştiriyor mu?
    // --- EKSİK SATIR BUYDU (Atış hızı sayacı) ---
    private float nextFireTime = 0f;
    // --- YENİ EKLENDİ (FIRLATMA SESİ) ---
    [SerializeField] private AudioClip grenadeThrowSound;
    // ---

    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI ammoText; // Mermi sayısını gösterecek Text
    // ---

    // --- GİRDİ SİSTEMİ ABONELİĞİ ---
    private PlayerInput playerInput;
    private InputAction fireAction;
    private InputAction grenadeAction;
    private InputAction reloadAction; // <-- YENİ EKLENDİ
    // --- YENİ EKLENDİ (SES İÇİN) ---
    private AudioSource audioSource; // Adım sesleri için olanla aynı
    // ---
    // ---

    // Awake, PlayerInput ve eylemleri bulur
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerShoot: 'PlayerInput' bileşeni bulunamadı!");
        }

        fireAction = playerInput.actions["Fire"];
        grenadeAction = playerInput.actions["ThrowGrenade"];
        reloadAction = playerInput.actions["Reload"]; // <-- YENİ EKLENDİ

        // --- YENİ EKLENDİ ---
        // Player objesindeki (PlayerMovement'ın da kullandığı)
        // AudioSource bileşenini al
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) // Güvenlik önlemi
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // ---
    }

    // OnEnable, eylemlere abone olur
    private void OnEnable()
    {
        fireAction.performed += HandleFire;
        grenadeAction.performed += HandleThrowGrenade;
        reloadAction.performed += HandleReload; // <-- YENİ EKLENDİ
    }

    // OnDisable, abonelikten çıkar
    private void OnDisable()
    {
        fireAction.performed -= HandleFire;
        grenadeAction.performed -= HandleThrowGrenade;
        reloadAction.performed -= HandleReload; // <-- YENİ EKLENDİ
    }

    // Start, oyuna başlarken silahı kuşanır
    private void Start()
    {
        if (currentWeapon != null)
        {
            EquipWeapon(currentWeapon);
        }
        else
        {
            Debug.LogWarning("PlayerShoot: Başlangıçta 'Current Weapon' atanmamış!");
        }
    }

    // Silahı kuşanma fonksiyonu (GÜNCELLENDİ)
    private void EquipWeapon(WeaponData weaponToEquip)
    {
        currentWeapon = weaponToEquip;
        
        // --- MERMİ SİSTEMİ GÜNCELLENDİ ---
        // Yeni silahın mermi bilgilerini yükle
        currentAmmoInMag = currentWeapon.magazineSize;
        currentReserveAmmo = currentWeapon.startingAmmo;
        isReloading = false; // Silah değişince reload iptal
        UpdateAmmoUI(); // UI'ı güncelle
        // ---

        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }

        if (currentWeapon.weaponModelPrefab != null)
        {
            currentWeaponModel = Instantiate(
                currentWeapon.weaponModelPrefab, 
                weaponHolder.position, 
                weaponHolder.rotation,
                weaponHolder 
            );
            currentWeaponModel.transform.localPosition = Vector3.zero;
            currentWeaponModel.transform.localRotation = Quaternion.identity;
        }
    }


    // HandleFire (Ateş etme tetikleyicisi) (GÜNCELLENDİ)
    private void HandleFire(InputAction.CallbackContext context)
    {
        if (currentWeapon == null) return;

        // --- MERMİ KONTROLLERİ EKLENDİ ---
        // 1. Şarjör değiştiriyorsak ateş etme
        if (isReloading) return;

        // 2. Mermi bittiyse otomatik şarjör değiştir (opsiyonel)
        if (currentAmmoInMag <= 0)
        {
            Debug.Log("Mermi bitti, şarjör değiştiriliyor...");
            StartReload(); // Şarjör değiştir
            // (Buraya "boş şarjör" sesi eklenebilir)
            return; 
        }
        // ---

        // Atış hızı kontrolü
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentWeapon.fireRate;
            Shoot();
        }
    }

    // Shoot (Ateş etme mekaniği) (GÜNCELLENDİ)
    private void Shoot()
    {
        // --- MERMİ DÜŞÜRME EKLENDİ ---
        currentAmmoInMag--; // Mermiyi 1 azalt
        UpdateAmmoUI(); // UI'ı güncelle
        // ---
        // 'PlayClipAtPoint' kullanıyoruz ki ses 3D olsun ve kameradan gelsin
        if (currentWeapon.fireSound != null)
        {
            AudioSource.PlayClipAtPoint(currentWeapon.fireSound, cameraTransform.position, 0.7f); // (0.7f = ses seviyesi)
        }
        // ---

        if (currentWeapon.muzzleFlashEffect != null)
        {
            currentWeapon.muzzleFlashEffect.Play();
        }

        RaycastHit hitInfo;
        bool hasHit = Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out hitInfo,
            currentWeapon.fireRange
        );
// --- YENİ EKLENDİ (Mermi İzi Başlatma) ---
        // Mermi izinin başlayacağı yer (namlu ucu veya kamera)
        // Eğer 'muzzleFlashEffect' varsa onun pozisyonunu al, yoksa kamerayı kullan
        Vector3 trailStartPoint = (currentWeapon.muzzleFlashEffect != null) ? 
                                  currentWeapon.muzzleFlashEffect.transform.position : 
                                  cameraTransform.position;
        if (hasHit)
        {
            StartCoroutine(ShowBulletTrail(trailStartPoint, hitInfo.point));
            EnemyHealth targetHealth = hitInfo.transform.GetComponentInParent<EnemyHealth>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(
                    currentWeapon.damage,
                    hitInfo.point,
                    Quaternion.LookRotation(hitInfo.normal),
                    hitInfo.collider.name 
                );
            }
            if (currentWeapon.impactEffectPrefab != null)
            {
                Instantiate(currentWeapon.impactEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            }
            else
        {
            // 2. BOŞA ATTIK
            // Mermi izini 'namlu ucundan' 'maksimum menzile' kadar çiz
            Vector3 endPoint = cameraTransform.position + cameraTransform.forward * currentWeapon.fireRange;
            StartCoroutine(ShowBulletTrail(trailStartPoint, endPoint));
        }
        }
    }

    // --- YENİ EKLENEN RELOAD (ŞARJÖR DEĞİŞTİRME) FONKSİYONLARI ---
    
    // 'R' tuşuna basıldığında tetiklenir
    private void HandleReload(InputAction.CallbackContext context)
    {
        StartReload();
    }

    private void StartReload()
    {
        // Şu durumlarda şarjör DEĞİŞTİRME:
        // 1. Zaten şarjör değiştiriyorsak
        // 2. Şarjör zaten doluysa
        // 3. Hiç yedek mermimiz yoksa
        if (isReloading || currentAmmoInMag == currentWeapon.magazineSize || currentReserveAmmo <= 0)
        {
            return;
        }
        // --- YENİ EKLENDİ (Şarjör Sesi) ---
        if (currentWeapon.reloadSound != null && audioSource != null)
        {
            // 'PlayOneShot' kullanıyoruz ki diğer seslerle karışmasın
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }
        // ---
        
        // Coroutine'i (Zamanlayıcıyı) başlat
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        Debug.Log("Şarjör değiştiriliyor...");
        isReloading = true;
        
        // (Buraya şarjör değiştirme animasyonu/sesi tetikleme kodu gelir)

        // Silahın 'reloadTime' değeri (örn: 1.5 saniye) kadar bekle
        yield return new WaitForSeconds(currentWeapon.reloadTime);

        // Hesaplama
        int bulletsNeeded = currentWeapon.magazineSize - currentAmmoInMag; // Şarjörde kaç mermi eksiği var?
        int bulletsToMove = Mathf.Min(bulletsNeeded, currentReserveAmmo); // Yedekten kaç mermi alabiliriz?

        // Mermileri aktar
        currentAmmoInMag += bulletsToMove;
        currentReserveAmmo -= bulletsToMove;

        isReloading = false;
        UpdateAmmoUI(); // UI'ı güncelle
    }

    // Mermi UI'ını güncelleyen fonksiyon
    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            // Örn: "10 / 30"
            ammoText.text = $"{currentAmmoInMag} / {currentReserveAmmo}";
        }
    }

    // (Gelecekte düşman drop'ları için) Dışarıdan mermi ekleme fonksiyonu
    public void AddAmmo(int amount)
    {
        currentReserveAmmo += amount;
        UpdateAmmoUI();
    }
    
    // --- GRENADE KISMI (Değişiklik yok) ---
    private void HandleThrowGrenade(InputAction.CallbackContext context)
    {
        if (grenadePrefab != null) 
        {
            ThrowGrenade();
        }
    }

    private void ThrowGrenade()
    {
        Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * 0.5f;
        GameObject grenade = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();

        // --- YENİ EKLENDİ (FIRLATMA SESİ ÇAL) ---
        // (PlayerShoot.cs'teki 'audioSource' referansını kullanıyoruz)
        if (grenadeThrowSound != null && audioSource != null)
        {
            // 'PlayOneShot' kullanıyoruz ki diğer seslerle karışmasın
            audioSource.PlayOneShot(grenadeThrowSound, 0.8f); // (0.8f = ses seviyesi)
        }
        
        // ---

        if (grenadeRb != null)
        {
            grenadeRb.AddForce(cameraTransform.forward * throwForce, ForceMode.VelocityChange);
        }
    }
    private IEnumerator ShowBulletTrail(Vector3 startPoint, Vector3 endPoint)
    {
        if (bulletTrail == null) yield break; // Mermi izi atanmamışsa çık

        // 'LineRenderer'ı ayarla
        bulletTrail.SetPosition(0, startPoint); // Başlangıç (Namlu ucu)
        bulletTrail.SetPosition(1, endPoint);   // Bitiş (Vurulan yer / Boşluk)
        
        bulletTrail.enabled = true; // İzi görünür yap

        // 'trailDuration' (örn: 0.05 saniye) kadar bekle
        yield return new WaitForSeconds(trailDuration);

        bulletTrail.enabled = false; // İzi gizle
    }
}