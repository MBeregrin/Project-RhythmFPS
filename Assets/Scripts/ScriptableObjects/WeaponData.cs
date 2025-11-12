using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Rhythm Game/New Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Data")]
    public string weaponName = "Blaster-r"; 
    public GameObject weaponModelPrefab; 

    [Header("Shooting Settings")]
    public int damage = 10;               
    public float fireRange = 100f;         
    public float fireRate = 0.5f;          
    public AudioClip fireSound; 
    [Header("Ammo Settings")]
    public int magazineSize = 10;     
    public int startingAmmo = 30;     
    public float reloadTime = 1.5f;   
    [Header("Visual Effects")]
    public ParticleSystem muzzleFlashEffect;
    public GameObject impactEffectPrefab;
    public AudioClip reloadSound;
}