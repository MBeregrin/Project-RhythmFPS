using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
    [Header("Patlama Ayarları")]
    public float delay = 1.8f;            // Patlama süresi
    public float radius = 2f;           // Patlama etki alanı
    public int damage = 50;             // Patlama hasarı
    public float force = 600f;          // İtme kuvveti

    // --- YENİ EKLENDİ (PATLAMA SESİ) ---
    [Header("Ses")]
    [SerializeField] private AudioClip explosionSound;
    // ---

    // Yerde çıkacak siyah iz
    public GameObject scorchMarkPrefab;

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

   private void Explode()
    {
    // --- YENİ EKLENDİ (PATLAMA SESİ ÇAL) ---
        // 'PlayClipAtPoint', bu obje (bomba) yok olsa bile
        // sesin çalmaya devam etmesini sağlar.
        if (explosionSound != null)
        {
            // Sesi, patlamanın olduğu yerde (transform.position) çal
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 0.3f); // (1.0f = full ses)
        }
        // ---
    // 1. Etki Alanı İçindeki Nesneleri Tespit Et
    Collider[] hitObjects = Physics.OverlapSphere(transform.position, radius);

    foreach (Collider objCollider in hitObjects)
    {
        // Eğer vurduğumuz şeyin tag'i "Enemy" ise devam et.
        if (objCollider.CompareTag("Enemy"))
        {
            // Hasar uygula
            EnemyHealth enemyHealth = objCollider.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage, transform.position, Quaternion.identity, "Torso");
            }

            // Fiziksel itme uygula
            Rigidbody targetRb = objCollider.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                targetRb.AddExplosionForce(force, transform.position, radius, 1f, ForceMode.Impulse);
            }
        }

        // --- YENİ KONTROL: Oyuncuyu (Player) etkileme ---
        else if (objCollider.CompareTag("Player"))
        {
            // Oyuncuya hasar veya kuvvet uygulama.
            continue; // Bu objeyi atla ve bir sonraki objeye geç.
        }
    }
}

    }

