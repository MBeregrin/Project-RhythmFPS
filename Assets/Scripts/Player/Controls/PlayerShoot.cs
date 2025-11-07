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
    // Raycast Hasar Değeri (Tek bir yerde tanımlanmalı)
        const int damage = 10;
    // 1. Namlu Ateşi (Görsel Geri Bildirim)
    if (muzzleFlashEffect!= null)
    {
       muzzleFlashEffect.Play(); // Namlu ateşini oynat
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
            // --- KRİTİK DÜZELTME BURADA ---
            // hitInfo'nun çarptığı objede EnemyHealth.cs bileşenini ara.
            EnemyHealth targetHealth = hitInfo.transform.GetComponent<EnemyHealth>(); 
            // ---------------------------------
            
            // Eğer EnemyHealth bileşeni bulunursa (yani bir düşmana çarptıysak)...
            if (targetHealth!= null)
            {
                //...o düşmanın TakeDamage fonksiyonunu çağır!
                targetHealth.TakeDamage(damage); 
            } 
            else
            {
                // Eğer Health script'i yoksa (örneğin zemine veya duvara vurduk)
                Debug.Log("ÇARPTI: " + hitInfo.transform.name + " (Hasar verilemedi - EnemyHealth yok)"); 
            }
        Debug.Log("ÇARPTI: " + hitInfo.transform.name); 

        // Çarpma efekti (mermi izi) oluştur
        if (impactEffectPrefab!= null)
        {
           // Çarpma noktasında (hitInfo.point) ve o yüzeyin baktığı
           // yöne (hitInfo.normal) doğru bir efekt oluştur (Instantiate). 
           Instantiate(impactEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal)); 
        }
    }
}
}