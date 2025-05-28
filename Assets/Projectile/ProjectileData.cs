using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile", menuName = "Projectiles/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("Basic Settings")]
    public string projectileName;
    public Sprite projectileSprite;  // Добавлено для исправления ошибки
    public float scale = 1f;        // Добавлено для исправления ошибки
    public float colliderScale = 0.7f;
    
    [Header("Template Prefab (Optional)")]
    [Tooltip("Если указан, используется как шаблон для создания снаряда вместо создания с нуля")]
    public GameObject templatePrefab; // Опциональный шаблон префаба для визуальной настройки
    
    [Header("Physics")]
    public float speed = 20f;
    public float damage = 10f;
    public float lifetime = 5f;
    public float gravity = 0f;    // Добавлено для исправления ошибки, 0 для прямой траектории, >0 для гравитации
    public LayerMask targetLayers;
    
    [Header("Collision")]
    public bool stickToSurfaces = true;
    public float stickDuration = 10f;
    public bool destroyOnImpact = false;
    public bool penetrateTargets = false;
    public int maxPenetrations = 1;
    
    [Header("Visual Effects")]
    public GameObject trailEffect;
    public GameObject impactEffect;
    public Color trailColor = Color.white;
    public float trailWidth = 0.1f;
    public float trailLifetime = 0.5f;
    
    [Header("Audio")]
    public AudioClip launchSound;
    public AudioClip impactSound;
    public AudioClip flyingSound; // Для свистящих снарядов
    public bool loopFlyingSound = false;
    
    [Header("Special Effects")]
    public bool hasExplosion = false;
    public float explosionRadius = 0f;
    public float explosionDamage = 0f;
    public GameObject explosionEffect;
    
    [Header("Projectile Behavior")]
    public bool hasHoming = false;
    public float homingStrength = 5f;
    public float homingRange = 10f;
    public Transform homingTarget;
    
    // Метод для кастомной логики при столкновении
    public virtual void OnImpact(GameObject projectile, GameObject target)
    {
        // Базовая логика столкновения
        if (hasExplosion && explosionEffect != null)
        {
            CreateExplosion(projectile.transform.position);
        }
    }
    
    protected void CreateExplosion(Vector3 position)
    {
        // Создаем эффект взрыва
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, position, Quaternion.identity);
        }
        
        // Наносим урон всем объектам в радиусе
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(position, explosionRadius, targetLayers);
        foreach (var obj in hitObjects)
        {
            Balloon balloon = obj.GetComponent<Balloon>();
            if (balloon != null)
            {
                balloon.OnHit();
            }
        }
    }
}