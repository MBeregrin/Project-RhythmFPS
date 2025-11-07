using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour // Temel MonoBehaviour sınıfı [2]
{
    [Header("Ateş Etme Ayarları")]
    [SerializeField]private float fireRange = 100f; // Işının ne kadar uzağa gideceği.

    [Header("Görsel Efektler")]

    [SerializeField]private ParticleSystem muzzleFlashEffect; // Bulduğunuz "Namlu Ateşi" Parçacık Sistemini buraya sürükleyeceğiz.


    [SerializeField]private GameObject impactEffectPrefab; // Bulduğunuz "Mermi İzi" Prefab'ını buraya sürükleyeceğiz.

   
    [SerializeField]private Transform cameraTransform; // Ateşin başlayacağı yer (kamera). [2]

    // Kenney kitinden gelen namlu ateşi (muzzle flash) parçacık efektini
    // daha sonra buraya sürükleyebiliriz.
    // private ParticleSystem muzzleFlash;
    
    // Yere çarptığında oluşacak mermi izi (impact effect) prefab'ı
    // private GameObject impactEffect;

    // Input System tarafından çağrılacak FONKSİYON.
    // Adı "On" + Eylem Adı ("Fire") = "OnFire"
    // Bu fonksiyon sadece tuşa "basıldığı an" bir kez tetiklenir.
    public void OnFire(InputValue value)
    {
        // Eğer value.isPressed (veya Get<float>() > 0) kontrolü yaparsak
        // basılı tutmayı da algılayabiliriz, ama şimdilik "Doom" gibi tekli
        // atışlar için bu event'in tetiklenmesi yeterli.
        
        Shoot();
    }

    private void Shoot()
    {
        const int damage = 10; 

        // 1. Namlu Ateşi (Görsel Geri Bildirim)
        if (muzzleFlashEffect!= null)
        {
           muzzleFlashEffect.Play(); 
        }

        // 2. Işın Gönderme (Raycast) 
        RaycastHit hitInfo;
        bool hasHit = Physics.Raycast(
            cameraTransform.position, 
            cameraTransform.forward, 
            out hitInfo, 
            fireRange
        );

        // 3. Sonucu Değerlendirme
        if (hasHit)
        {
            // --- KRİTİK DÜZELTME BURADA: EnemyHealth'i ara. ---
            EnemyHealth targetHealth = hitInfo.transform.GetComponent<EnemyHealth>(); 
            
           if (targetHealth!= null)
            {
                // YENİ VE DÜZELTİLMİŞ ÇAĞRI: LBH için gereken 4 parametreyi de gönderiyoruz.
                targetHealth.TakeDamage(
                    damage, 
                    hitInfo.point, 
                    Quaternion.LookRotation(hitInfo.normal), 
                    hitInfo.collider.name // KRİTİK EKSİK PARAMETRE
                ); 
            }

            // Çarpma efekti (Impact Effect) oluştur
            if (impactEffectPrefab!= null)
            {
               Instantiate(impactEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal)); 
            }
        }
    
}
}