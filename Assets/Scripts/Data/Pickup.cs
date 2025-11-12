using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType 
    { 
        Health, 
        Ammo 
    }
    [Header("Pickup Settings")]
    public PickupType type = PickupType.Health;
    public int amount = 25;

    [Header("Visual Settings")]
    public float rotationSpeed = 50f;
    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"equiped: Type={type}, Amount={amount}");
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            PlayerShoot playerShoot = other.GetComponent<PlayerShoot>();
            switch (type)
            {
                case PickupType.Health:
                    if (playerHealth != null)
                    {
                        playerHealth.Heal(amount);
                    }
                    break;
                
                case PickupType.Ammo:
                    if (playerShoot != null)
                    {
                        playerShoot.AddAmmo(amount);
                    }
                    break;
            }
            Destroy(gameObject);
        }
    }
}