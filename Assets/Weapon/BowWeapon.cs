using UnityEngine;

[CreateAssetMenu(fileName = "Bow", menuName = "Weapon Data/Bow")]
public class BowWeapon : WeaponData
{
    [Header("Bow Specific")]
    public float chargeTime = 2f;      // Время зарядки для полной силы
    public AnimationCurve chargeCurve = new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 1.5f)); // Кривая зарядки
    
    private float currentCharge = 0f;
    private bool isCharging = false;
    
    public override void OnShoot(WeaponController controller)
    {
        // Создаем GameObject для снаряда
        GameObject projectile = new GameObject("BowProjectile");
        projectile.transform.position = controller.firePoint.position;
        projectile.transform.rotation = controller.firePoint.rotation;
        
        // Добавляем необходимые компоненты
        Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
        SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
        CircleCollider2D col = projectile.AddComponent<CircleCollider2D>();
        ProjectileController projectileController = projectile.AddComponent<ProjectileController>();
        
        // Применяем множитель силы выстрела в зависимости от зарядки
        float chargeMultiplier = isCharging ? chargeCurve.Evaluate(currentCharge / chargeTime) : 1f;
        
        // Устанавливаем спрайт и настраиваем ProjectileController
        if (projectileData != null)
        {
            sr.sprite = projectileData.projectileSprite;
            
            // Создаем модифицированную копию ProjectileData с измененной скоростью
            ProjectileData modifiedData = Instantiate(projectileData);
            modifiedData.speed *= chargeMultiplier;
            
            rb.gravityScale = modifiedData.gravity;
            col.radius = 0.1f;
            projectile.transform.localScale = Vector3.one * modifiedData.scale;
            
            // Инициализируем с модифицированными данными
            projectileController.Initialize(modifiedData, controller.transform.up);
        }
        
        // Сбрасываем зарядку
        currentCharge = 0f;
        isCharging = false;
    }
    
    // Метод для обработки зарядки лука (должен вызываться из внешнего класса)
    public void UpdateCharge(float deltaTime)
    {
        if (Input.GetButton("Fire1"))
        {
            isCharging = true;
            currentCharge = Mathf.Min(currentCharge + deltaTime, chargeTime);
        }
        else if (Input.GetButtonUp("Fire1") && isCharging)
        {
            isCharging = false;
        }
    }
}