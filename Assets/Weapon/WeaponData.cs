using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Settings")]
    public string weaponName;
    public Sprite weaponSprite;
    public float spriteScale = 1f;
    public ProjectileData projectileData; // Ссылка на тип снаряда
    
    [Header("Rotation & Animation")]
    public float rotationSpeed = 5f;
    public bool hasRecoil = true;
    public float recoilAmount = 0.1f;
    public float recoilDuration = 0.05f;
    
    [Header("Aim Settings")]
    public bool showAimLine = true;
    public float aimLineLength = 10f;
    public Color aimLineStartColor = new Color(1, 0, 0, 0.5f);
    public Color aimLineEndColor = new Color(1, 0, 0, 0.2f);
    public float aimLineWidth = 0.05f;
    
    [Header("Shooting Settings")]
    public float fireRate = 0.5f;
    public float projectileForce = 20f;
    public bool autoFire = false;
    public bool burstFire = false;
    public int burstCount = 3;
    public float burstDelay = 0.1f;
    
    [Header("Sound Effects")]
    public AudioClip shootSound;
    
    [Header("Ammo & Reload")]
    public bool usesAmmo = false;
    public int maxAmmo = 100;
    public float reloadTime = 2f;
    public bool autoReload = true;
    
    // Метод для кастомной логики стрельбы для каждого типа оружия
    public virtual void OnShoot(WeaponController controller)
    {
        // Базовая логика стрельбы
        controller.CreateProjectile();
    }
}