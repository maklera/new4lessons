using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    private ProjectileData data;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D projectileCollider;
    private TrailRenderer trail;
    private AudioSource audioSource;
    
    private bool stuck = false;
    private bool hasImpacted = false;
    private float destroyTime;
    private int penetrationCount = 0;
    private Vector2 currentDirection;
    
    private void Awake()
    {
        // Компоненты получаем сразу при создании
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        projectileCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
    }
    
    // Публичный метод для инициализации снаряда из WeaponController
    public void Initialize(ProjectileData projectileData, Vector2 direction)
    {
        data = projectileData;
        currentDirection = direction.normalized;
        
        // Настройка физики
        EnsureComponents();
        SetupPhysics();
        SetupVisuals();
        SetupAudio();
        
        // Запуск снаряда
        Launch();
    }
    
    private void EnsureComponents()
    {
        // Добавляем компоненты если их нет
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Создаем два коллайдера: внешний (триггер) и внутренний (физический)
        SetupColliders();
        
        if (data.trailEffect != null && trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            SetupTrail();
        }
        
        if (audioSource == null && (data.flyingSound != null || data.impactSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }
    
    private void SetupColliders()
    {
        // Создаем внешний триггер-коллайдер для взаимодействия с шариками
        CircleCollider2D triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale : 0.7f);
        
        // Создаем внутренний физический коллайдер для взаимодействия со стенами
        CircleCollider2D physicsCollider = gameObject.AddComponent<CircleCollider2D>();
        physicsCollider.isTrigger = false;
        physicsCollider.radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale * 0.7f : 0.5f); // Меньше чем триггер
        
        // Сохраняем ссылку на физический коллайдер
        projectileCollider = physicsCollider;
    }
    
    private void SetupPhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = data.gravity;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        destroyTime = Time.time + data.lifetime;
    }
    
    private void SetupVisuals()
    {
        if (data.projectileSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = data.projectileSprite;
        }
        
        // Применяем масштаб из ProjectileData
        float scale = data.scale;
        transform.localScale = Vector3.one * scale;
        
        // Обновляем коллайдеры если они уже созданы
        UpdateCollidersScale();
    }
    
    private void UpdateCollidersScale()
    {
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        
        if (colliders.Length >= 2)
        {
            // Находим триггер и физический коллайдеры
            CircleCollider2D triggerCollider = null;
            CircleCollider2D physicsCollider = null;
            
            foreach (var col in colliders)
            {
                if (col.isTrigger && triggerCollider == null)
                    triggerCollider = col;
                else if (!col.isTrigger && physicsCollider == null)
                    physicsCollider = col;
            }
            
            // Применяем размеры
            if (triggerCollider != null)
            {
                triggerCollider.radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale : 0.7f);
            }
            
            if (physicsCollider != null)
            {
                physicsCollider.radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale * 0.7f : 0.5f);
            }
        }
        else if (colliders.Length == 1)
        {
            // Если только один коллайдер
            colliders[0].radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale : 0.7f);
        }
    }
    private void SetupTrail()
    {
        if (trail != null)
        {
            trail.time = data.trailLifetime;
            trail.startWidth = data.trailWidth;
            trail.endWidth = data.trailWidth * 0.1f;
            trail.startColor = data.trailColor;
            trail.endColor = new Color(data.trailColor.r, data.trailColor.g, data.trailColor.b, 0);
            trail.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
    
    private void SetupAudio()
    {
        if (data.flyingSound != null && audioSource != null)
        {
            audioSource.clip = data.flyingSound;
            audioSource.loop = data.loopFlyingSound;
            audioSource.Play();
        }
    }
    
    private void Launch()
    {
        rb.linearVelocity = currentDirection * data.speed;
        UpdateRotation();
        
        if (data.launchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(data.launchSound);
        }
        
        // Для взрывных снарядов с отложенным взрывом
        if (data is ExplosiveProjectile explosiveData && !explosiveData.explodeOnImpact)
        {
            StartCoroutine(FuseCountdown(explosiveData.fuseTime));
        }
    }
    
    // Метод для внешнего вызова запуска таймера
    public void StartFuseCountdown(float fuseTime)
    {
        StartCoroutine(FuseCountdown(fuseTime));
    }
    
    private System.Collections.IEnumerator FuseCountdown(float fuseTime)
    {
        yield return new WaitForSeconds(fuseTime);
        if (data != null && gameObject != null)
        {
            // Вызываем взрыв
            data.OnImpact(gameObject, null);
        }
    }
    
    private void Update()
    {
        if (stuck) return;
        
        // Обновление вращения по направлению движения
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            UpdateRotation();
        }
        
        // Самонаведение
        if (data != null && data.hasHoming && data.homingTarget != null)
        {
            ApplyHoming();
        }
        
        // Уничтожение по времени
        if (Time.time > destroyTime && !stuck)
        {
            Destroy(gameObject);
        }
    }
    
    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
    
    private void ApplyHoming()
    {
        if (data.homingTarget == null) return;
        
        float distance = Vector2.Distance(transform.position, data.homingTarget.position);
        if (distance <= data.homingRange)
        {
            Vector2 direction = (data.homingTarget.position - transform.position).normalized;
            currentDirection = Vector2.Lerp(currentDirection, direction, Time.deltaTime * data.homingStrength).normalized;
            rb.linearVelocity = currentDirection * data.speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (stuck || hasImpacted) return;
        
        // Обрабатываем только триггер-столкновения с шариками (воздушными шарами)
        if (other.CompareTag("Ball"))
        {
            HandleBalloonImpact(other.gameObject);
        }
        
        // Другие триггер-объекты можно обрабатывать здесь
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (stuck) return;
        
        // Обрабатываем столкновения со стенами и другими твердыми объектами
        if (collision.collider.CompareTag("Wall") || collision.collider.CompareTag("Ground"))
        {
            HandleWallImpact(collision.gameObject);
        }
        else if (!collision.collider.CompareTag("Ball")) // Для всех остальных объектов, кроме шариков
        {
            // Проверяем, нужно ли обрабатывать столкновение как с окружением
            HandleGenericImpact(collision.gameObject);
        }
    }
    
    // Обработка попадания в шарик
    private void HandleBalloonImpact(GameObject balloon)
    {
        Balloon balloonComponent = balloon.GetComponent<Balloon>();
        if (balloonComponent != null)
        {
            balloonComponent.OnHit();
            
            // Если снаряд не проникающий, он должен застрять или уничтожиться
            if (!data.penetrateTargets)
            {
                // Звук попадания
                if (data.impactSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(data.impactSound);
                }
                
                // Считаем попадание, но не останавливаем полет снаряда
                hasImpacted = true;
            }
        }
    }
    
    // Обработка столкновения со стеной или полом
    private void HandleWallImpact(GameObject wall)
    {
        // Вызываем кастомную логику удара
        data.OnImpact(gameObject, wall);
        
        // Застреваем в стене
        StickToSurface();
    }
    
    // Обработка других столкновений
    private void HandleGenericImpact(GameObject target)
    {
        // Проверка на целевой слой
        if (data != null && ((1 << target.gameObject.layer) & data.targetLayers) == 0) return;
        
        // Проникновение сквозь цели
        if (data.penetrateTargets && penetrationCount < data.maxPenetrations)
        {
            penetrationCount++;
            data.OnImpact(gameObject, target);
            return;
        }
        
        hasImpacted = true;
        
        // Звук удара
        if (data.impactSound != null && audioSource != null)
        {
            if (data.flyingSound != null)
            {
                audioSource.Stop();
            }
            audioSource.PlayOneShot(data.impactSound);
        }
        
        // Эффект удара
        if (data.impactEffect != null)
        {
            Instantiate(data.impactEffect, transform.position, Quaternion.identity);
        }
        
        // Вызов кастомной логики удара
        data.OnImpact(gameObject, target);
        
        // Обработка столкновения
        if (data.stickToSurfaces && !data.destroyOnImpact)
        {
            StickToSurface();
        }
        else if (data.destroyOnImpact)
        {
            Destroy(gameObject);
        }
    }
    
    private void StickToSurface()
    {
        stuck = true;
        rb.bodyType = RigidbodyType2D.Static;
        
        // ИСПРАВЛЕНИЕ: Отключаем ВСЕ коллайдеры, а не только физический
        Collider2D[] allColliders = GetComponents<Collider2D>();
        foreach (var collider in allColliders)
        {
            collider.enabled = false;
        }
        
        if (data.stickDuration > 0)
        {
            Destroy(gameObject, data.stickDuration);
        }
    }
}