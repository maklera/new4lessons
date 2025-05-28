using UnityEngine;

[CreateAssetMenu(fileName = "Pistol", menuName = "Weapon Data/Pistol")]
public class PistolWeapon : WeaponData
{
    // Пистолет можно быстро стрелять, но нужно целиться
    [Header("Pistol Specific")]
    public float accuracyRecoveryRate = 0.5f; // Скорость восстановления точности после выстрела
    
    private float currentAccuracy = 1.0f; // 1.0 = идеальная точность, <1.0 = хуже
    private float lastShotTime = 0f;
    
    public override void OnShoot(WeaponController controller)
    {
        // Базовая реализация использует CreateProjectile
        base.OnShoot(controller);
        
        // Обновляем точность
        currentAccuracy = Mathf.Max(0.7f, currentAccuracy - 0.1f);
        lastShotTime = Time.time;
    }
    
    // Метод обновления точности пистолета (должен вызываться внешним классом)
    public void UpdateAccuracy()
    {
        // Восстанавливаем точность со временем
        if (Time.time - lastShotTime > 0.5f)
        {
            currentAccuracy = Mathf.Min(1.0f, currentAccuracy + Time.deltaTime * accuracyRecoveryRate);
        }
    }
}