using UnityEngine;

public class Pickup : MonoBehaviour
{
    // ENUM: Bu pickup'ın türünü Inspector'dan seçmemizi sağlar
    public enum PickupType 
    { 
        Health, 
        Ammo 
    }

    [Header("Pickup Ayarları")]
    public PickupType type = PickupType.Health; // Tür: Can mı, Mermi mi?
    public int amount = 25; // Vereceği miktar (25 Can veya 25 Mermi)

    [Header("Görsel Ayarlar")]
    public float rotationSpeed = 50f; // Havada dönme hızı

    private void Update()
    {
        // Havada yavaşça kendi etrafında dönsün
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    // Oyuncu bu objenin "içinden geçtiğinde" (tetiklendiğinde) çalışır
    private void OnTriggerEnter(Collider other)
    {
        // Çarpan obje "Player" etiketine sahip mi?
        if (other.CompareTag("Player"))
        {
            // Evet, bu oyuncu.
            Debug.Log($"Pickup alındı: Tür={type}, Miktar={amount}");

            // 1. Oyuncunun script'lerini bul
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();

            // 2. Türüne göre işlem yap
            switch (type)
            {
                case PickupType.Health:
                    // Eğer Can ise ve PlayerHealth script'i varsa
                    if (playerHealth != null)
                    {
                        // PlayerHealth'e can ekleme fonksiyonu lazım
                        // (Onu bir sonraki adımda ekleyeceğiz)
                        playerHealth.Heal(amount);
                    }
                    break;
                
                case PickupType.Ammo:
                    // Eğer Mermi ise ve PlayerShoot script'i varsa
                    if (playerShoot != null)
                    {
                        // PlayerShoot'taki 'AddAmmo' fonksiyonunu çağır (Bu zaten var)
                        playerShoot.AddAmmo(amount);
                    }
                    break;
            }

            // 3. Pickup objesini yok et
            // (İsteğe bağlı: 'Pickup' sesi çal)
            Destroy(gameObject);
        }
    }
}