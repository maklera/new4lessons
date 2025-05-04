using UnityEngine;

[CreateAssetMenu(fileName = "Automatic", menuName = "Weapon Data/Automatic")]
public class AutomaticWeapon : WeaponData
{
    [Header("Automatic Specific")]
    public float spread = 5f;           // Разброс пуль
    public float recoilIncrease = 0.1f; // Увеличение отдачи при автоматической стрельбе
    
    private int continuousFireCount = 0;
    
    public override void OnShoot(WeaponController controller)
    {
        // Добавляем разброс для автоматического оружия
        Vector2 direction = controller.transform.up;
        
        if (continuousFireCount > 0)
        {
            float randomSpread = Random.Range(-spread, spread);
            direction = Quaternion.Euler(0, 0, randomSpread) * direction;
        }
        
        // Кастомный снаряд для автомата
        GameObject projectile = Object.Instantiate(projectilePrefab, controller.firePoint.position, controller.firePoint.rotation);
        
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction * projectileForce, ForceMode2D.Impulse);
        }
        
        // Увеличиваем отдачу при продолжительной стрельбе
        continuousFireCount++;
        if (continuousFireCount > 5)
        {
            controller.transform.localPosition -= controller.transform.up * (recoilAmount + recoilIncrease * continuousFireCount);
        }
        else
        {
            controller.transform.localPosition -= controller.transform.up * recoilAmount;
        }
        
        // Сбрасываем счетчик если игрок перестал стрелять
        if (!Input.GetButton("Fire1"))
        {
            continuousFireCount = 0;
        }
    }
}