using UnityEngine;
using System.Collections;
using System.Collections.Generic; // <-- 1. YENİ EKLENEN SATIR (Liste için)

public class Grenade : MonoBehaviour
{
    [Header("Patlama Ayarları")]
    public float delay = 3f; 
    public float radius = 5f; 
    public int damage = 50; 
    public float force = 700f; 
    public GameObject scorchMarkPrefab;

    [Header("Ses")]
    [SerializeField] private AudioClip explosionSound;

    void Start()
    {
        StartCoroutine(ExplodeAfterDelay());
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        // Patlat
        Explode();

        // Yere siyah iz bırak
        if (scorchMarkPrefab != null)
        {
            GameObject scorch = Instantiate(
                scorchMarkPrefab,
                transform.position,
                Quaternion.identity
            );
            Destroy(scorch, 3f);
        }

        // Bombayı yok et
        Destroy(gameObject);
    }

    // --- 2. GÜNCELLENEN FONKSİYON (Explode) ---
    private void Explode()
    {
        // 'PlayClipAtPoint', bu obje (bomba) yok olsa bile
        // sesin çalmaya devam etmesini sağlar.
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1.0f);
        }

        // YENİ: Bu patlamada vurulan düşmanların listesi
        List<EnemyHealth> enemiesHit = new List<EnemyHealth>();

        // 1. Etki Alanı İçindeki Nesneleri Tespit Et
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider objCollider in hitObjects)
        {
            // A. Düşman mı diye bakar
            if (objCollider.CompareTag("Enemy"))
            {
                // Collider'ın kendisinde veya üst objesinde (parent) EnemyHealth ara
                EnemyHealth enemyHealth = objCollider.GetComponentInParent<EnemyHealth>();

                // Eğer bu bir düşmansa VE bu listede daha önce yoksa...
                if (enemyHealth != null && !enemiesHit.Contains(enemyHealth))
                {
                    // Listeye ekle (böylece bir daha vurulamaz)
                    enemiesHit.Add(enemyHealth);
                    
                    // Hasar uygula (SADECE 1 KEZ)
                    enemyHealth.TakeDamage(damage, objCollider.transform.position, Quaternion.identity, "Torso");

                    // Fiziksel itme uygula
                    Rigidbody targetRb = objCollider.GetComponentInParent<Rigidbody>();
                    if (targetRb != null)
                    {
                        targetRb.AddExplosionForce(force, transform.position, radius, 1f, ForceMode.Impulse);
                    }
                }
            }
            // B. Oyuncu mu diye bakar (Dost Ateşi Koruması)
            else if (objCollider.CompareTag("Player"))
            {
                continue; // Bu objeyi atla ve bir sonraki objeye geç.
            }
        }
    }
}