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
        
        // Создаем GameObject для снаряда
        GameObject projectile = new GameObject("AutomaticProjectile");
        projectile.transform.position = controller.firePoint.position;
        projectile.transform.rotation = controller.firePoint.rotation;
        
        // Добавляем необходимые компоненты
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        ProjectileController projectileController = projectile.AddComponent<ProjectileController>();
        
        // Устанавливаем спрайт, если он указан в projectileData
        if (projectileData != null && projectileData.projectileSprite != null)
        {
            sr.sprite = projectileData.projectileSprite;
            
            // Настраиваем базовые параметры снаряда
            rb.gravityScale = projectileData.gravity;
            col.radius = 0.1f;
            projectile.transform.localScale = Vector3.one * projectileData.scale;
            
            // Инициализируем контроллер снаряда с измененным направлением
            projectileController.Initialize(projectileData, direction);
        }
        else
        {
            // Если нет ProjectileData, импровизируем с базовыми настройками
            rb.AddForce(direction * projectileForce, ForceMode2D.Impulse);
            sr.color = Color.yellow; // Базовый цвет для пуль автомата
            Destroy(projectile, 5f); // Уничтожаем через 5 секунд если нет данных
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