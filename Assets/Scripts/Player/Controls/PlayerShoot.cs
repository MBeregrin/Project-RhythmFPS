using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class PlayerShoot : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private WeaponData currentWeapon; 
    [SerializeField] private Transform cameraTransform;

    [Header("Weapon Holder")]
    [SerializeField] private Transform weaponHolder;
    private GameObject currentWeaponModel;

    [Header("Bullet Trail")]
    [SerializeField] private LineRenderer bulletTrail;
    [SerializeField] private float trailDuration = 0.05f;

    [Header("Grenade Settings")]
    public GameObject grenadePrefab;
    public float throwForce = 15f;
    private int currentAmmoInMag;
    private int currentReserveAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;
    [SerializeField] private AudioClip grenadeThrowSound;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI ammoText;
    private PlayerInput playerInput;
    private InputAction fireAction;
    private InputAction grenadeAction;
    private InputAction reloadAction;
    private AudioSource audioSource;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            
        }

        fireAction = playerInput.actions["Fire"];
        grenadeAction = playerInput.actions["ThrowGrenade"];
        reloadAction = playerInput.actions["Reload"];
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    private void OnEnable()
    {
        fireAction.performed += HandleFire;
        grenadeAction.performed += HandleThrowGrenade;
        reloadAction.performed += HandleReload;
    }
    private void OnDisable()
    {
        fireAction.performed -= HandleFire;
        grenadeAction.performed -= HandleThrowGrenade;
        reloadAction.performed -= HandleReload;
    }
    private void Start()
    {
        if (currentWeapon != null)
        {
            EquipWeapon(currentWeapon);
        }
        else
        {
        }
    }
    private void EquipWeapon(WeaponData weaponToEquip)
    {
        currentWeapon = weaponToEquip;
        currentAmmoInMag = currentWeapon.magazineSize;
        currentReserveAmmo = currentWeapon.startingAmmo;
        isReloading = false;
        UpdateAmmoUI();
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
    private void HandleFire(InputAction.CallbackContext context)
    {
        if (currentWeapon == null) return;
        if (isReloading) return;
        if (currentAmmoInMag <= 0)
        {
            StartReload();
            return; 
        }
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + currentWeapon.fireRate;
            Shoot();
        }
    }
    private void Shoot()
    {
        currentAmmoInMag--;
        UpdateAmmoUI();
        if (currentWeapon.fireSound != null)
        {
            AudioSource.PlayClipAtPoint(currentWeapon.fireSound, cameraTransform.position, 0.7f);
        }

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
            Vector3 endPoint = cameraTransform.position + cameraTransform.forward * currentWeapon.fireRange;
            StartCoroutine(ShowBulletTrail(trailStartPoint, endPoint));
        }
        }
    }
    private void HandleReload(InputAction.CallbackContext context)
    {
        StartReload();
    }

    private void StartReload()
    {
        if (isReloading || currentAmmoInMag == currentWeapon.magazineSize || currentReserveAmmo <= 0)
        {
            return;
        }
        if (currentWeapon.reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(currentWeapon.reloadTime);
        int bulletsNeeded = currentWeapon.magazineSize - currentAmmoInMag;
        int bulletsToMove = Mathf.Min(bulletsNeeded, currentReserveAmmo);
        currentAmmoInMag += bulletsToMove;
        currentReserveAmmo -= bulletsToMove;
        isReloading = false;
        UpdateAmmoUI();
    }
    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            // Örn: "10 / 30"
            ammoText.text = $"{currentAmmoInMag} / {currentReserveAmmo}";
        }
    }
    public void AddAmmo(int amount)
    {
        currentReserveAmmo += amount;
        UpdateAmmoUI();
    }
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
        if (grenadeThrowSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(grenadeThrowSound, 0.8f);
        }
        if (grenadeRb != null)
        {
            grenadeRb.AddForce(cameraTransform.forward * throwForce, ForceMode.VelocityChange);
        }
    }
    private IEnumerator ShowBulletTrail(Vector3 startPoint, Vector3 endPoint)
    {
        if (bulletTrail == null) yield break;

        // 'LineRenderer'ı ayarla
        bulletTrail.SetPosition(0, startPoint);
        bulletTrail.SetPosition(1, endPoint); 
        
        bulletTrail.enabled = true;
        yield return new WaitForSeconds(trailDuration);

        bulletTrail.enabled = false;
    }
}