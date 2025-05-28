using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    public WeaponData currentWeapon;
    public Transform firePoint;
    public WeaponData[] availableWeapons;
    public int currentWeaponIndex = 0;
    
    private Camera mainCamera;
    private Vector3 targetDirection;
    private Quaternion targetRotation;
    private float nextFireTime = 0f;
    private AudioSource audioSource;
    private LineRenderer aimLine;
    private SpriteRenderer weaponSprite;
    
    private int currentAmmo;
    private bool isReloading = false;
    
    // Публичные методы для доступа к приватным полям
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;
    
    // Методы для WeaponUI
    public int GetCurrentAmmo() => currentAmmo;
    public bool GetIsReloading() => isReloading;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Создаем firePoint если его нет
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, 0.5f, 0);
            firePoint = fp.transform;
        }
        
        // Получаем компоненты
        weaponSprite = GetComponent<SpriteRenderer>();
        if (weaponSprite == null)
        {
            weaponSprite = gameObject.AddComponent<SpriteRenderer>();
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
        
        // Инициализация оружия
        if (availableWeapons != null && availableWeapons.Length > 0)
        {
            SwitchWeapon(0);
        }
    }
    
    void Update()
    {
        if (currentWeapon == null) return;
        
        RotateTowardsMouse();
        UpdateAimLine();
        HandleShooting();
        HandleWeaponSwitching();
    }
    
    void RotateTowardsMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        
        targetDirection = worldPosition - transform.position;
        
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        angle -= 90f;
        
        targetRotation = Quaternion.Euler(0, 0, angle);
        
        if (currentWeapon.rotationSpeed > 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentWeapon.rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }
    
    void UpdateAimLine()
    {
        if (currentWeapon.showAimLine)
        {
            if (aimLine == null)
            {
                CreateAimLine();
            }
            
            aimLine.enabled = true;
            Vector3 direction = transform.up;
            aimLine.SetPosition(0, firePoint.position);
            aimLine.SetPosition(1, firePoint.position + direction * currentWeapon.aimLineLength);
        }
        else if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }
    
    void CreateAimLine()
    {
        if (currentWeapon == null) return;
        
        GameObject lineObj = new GameObject("AimLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        
        aimLine = lineObj.AddComponent<LineRenderer>();
        aimLine.positionCount = 2;
        aimLine.startWidth = currentWeapon.aimLineWidth;
        aimLine.endWidth = currentWeapon.aimLineWidth;
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.startColor = currentWeapon.aimLineStartColor;
        aimLine.endColor = currentWeapon.aimLineEndColor;
    }
    
    void HandleShooting()
    {
        if (isReloading) return;
        
        if (Time.time >= nextFireTime)
        {
            if (Input.GetButtonDown("Fire1") || (currentWeapon.autoFire && Input.GetButton("Fire1")))
            {
                if (currentWeapon.usesAmmo && currentAmmo <= 0)
                {
                    if (currentWeapon.autoReload)
                    {
                        StartReload();
                    }
                    return;
                }
                
                Shoot();
                nextFireTime = Time.time + (1f / currentWeapon.fireRate);
            }
        }
        
        // Ручная перезарядка
        if (currentWeapon.usesAmmo && Input.GetKeyDown(KeyCode.R))
        {
            StartReload();
        }
    }
    
    void HandleWeaponSwitching()
    {
        // Блокировка переключения во время перезарядки
        if (isReloading)
        {
            // Попытка переключения оружия во время перезарядки
            if (Input.mouseScrollDelta.y != 0 || Input.GetKeyDown(KeyCode.Alpha1) || 
                Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || 
                Input.GetKeyDown(KeyCode.Alpha4))
            {
                // Показываем подсказку в UI
                WeaponUI ui = FindObjectOfType<WeaponUI>();
                if (ui != null)
                {
                    ui.ShowErrorMessage("Cannot switch during reload!", 1.5f);
                }
                else
                {
                    Debug.Log("Cannot switch weapons during reload!");
                }
            
                // Можно воспроизвести звук ошибки
                if (audioSource != null && currentWeapon.shootSound != null)
                {
                    // Используем звук выстрела на низкой громкости как звук ошибки
                    audioSource.volume = 0.3f;
                    audioSource.PlayOneShot(currentWeapon.shootSound);
                    audioSource.volume = 1.0f;
                }
            }
            return; // Выходим из метода, если идет перезарядка
        }
    
        if (availableWeapons == null || availableWeapons.Length <= 1) return;
    
        // Переключение оружия колесиком мыши
        if (Input.mouseScrollDelta.y > 0)
        {
            SwitchWeapon((currentWeaponIndex + 1) % availableWeapons.Length);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            SwitchWeapon((currentWeaponIndex - 1 + availableWeapons.Length) % availableWeapons.Length);
        }
    
        // Переключение цифровыми клавишами
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && availableWeapons.Length > 1) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && availableWeapons.Length > 2) SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4) && availableWeapons.Length > 3) SwitchWeapon(3);
    }
    
    public void SwitchWeapon(int index)
    {
        if (availableWeapons == null || index >= availableWeapons.Length || index < 0) return;
    
        currentWeaponIndex = index;
        currentWeapon = availableWeapons[index];
    
        // Обновляем спрайт оружия
        if (weaponSprite != null && currentWeapon.weaponSprite != null)
        {
            weaponSprite.sprite = currentWeapon.weaponSprite;
        
            // Используем индивидуальный масштаб из WeaponData
            float scale = currentWeapon.spriteScale;
        
            // Применяем масштаб
            weaponSprite.transform.localScale = Vector3.one * scale;
        }
    
        // Инициализация боеприпасов
        if (currentWeapon.usesAmmo)
        {
            currentAmmo = currentWeapon.maxAmmo;
        }
    
        // Обновляем линию прицеливания
        CreateAimLine();
    
        Debug.Log($"Switched to {currentWeapon.weaponName}");
    }
    
    public void Shoot()
    {
        if (currentWeapon == null) return;
        
        // Вызываем кастомный метод стрельбы оружия
        currentWeapon.OnShoot(this);
        
        // Воспроизводим звук
        if (audioSource != null && currentWeapon.shootSound != null)
        {
            audioSource.clip = currentWeapon.shootSound;
            audioSource.Play();
        }
        
        // Отдача
        if (currentWeapon.hasRecoil)
        {
            StartCoroutine(RecoilCoroutine());
        }
        
        // Уменьшаем патроны
        if (currentWeapon.usesAmmo)
        {
            currentAmmo--;
        }
    }
    
    public void CreateProjectile()
    {
        if (currentWeapon.projectileData == null) return;
        
        GameObject projectile;
        
        // Проверяем, есть ли шаблонный префаб
        if (currentWeapon.projectileData.templatePrefab != null)
        {
            // Создаем снаряд из шаблона
            projectile = Instantiate(currentWeapon.projectileData.templatePrefab, 
                                     firePoint.position, 
                                     firePoint.rotation);
            
            // Используем настройки из ProjectileData для переопределения свойств
            ApplyProjectileDataToTemplate(projectile, currentWeapon.projectileData);
        }
        else
        {
            // Создаем объект с нуля (как раньше)
            projectile = new GameObject("Projectile");
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation;
            
            // Добавляем основные компоненты
            Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
            SpriteRenderer sr = projectile.AddComponent<SpriteRenderer>();
            ProjectileController controller = projectile.AddComponent<ProjectileController>();
            
            // Начальные настройки
            rb.gravityScale = 0;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Настраиваем слой для правильной обработки столкновений
            int projectileLayer = LayerMask.NameToLayer("Projectile");
            if (projectileLayer != -1)
            {
                projectile.layer = projectileLayer;
            }
        }
        
        // Получаем контроллер (он должен быть или в шаблоне, или создан выше)
        ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
        if (projectileController == null)
        {
            projectileController = projectile.AddComponent<ProjectileController>();
        }
        
        // Инициализируем контроллер
        projectileController.Initialize(currentWeapon.projectileData, transform.up);
    }

    private void ApplyProjectileDataToTemplate(GameObject projectile, ProjectileData data)
    {
        // Применяем спрайт, если он указан
        if (data.projectileSprite != null)
        {
            SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = data.projectileSprite;
            }
        }
        
        // Применяем размер
        projectile.transform.localScale = Vector3.one * data.scale;
        
        // Настраиваем коллайдеры с учетом colliderScale
        CircleCollider2D[] colliders = projectile.GetComponents<CircleCollider2D>();
        if (colliders.Length >= 2)
        {
            // Первый коллайдер - триггер (больший)
            if (colliders[0].isTrigger)
            {
                colliders[0].radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale : 0.7f);
            }
            
            // Второй коллайдер - физический (меньший)
            if (!colliders[1].isTrigger)
            {
                colliders[1].radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale * 0.7f : 0.5f);
            }
        }
        else if (colliders.Length == 1)
        {
            // Если только один коллайдер, применяем к нему
            colliders[0].radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale : 0.7f);
        }
        
        // Настраиваем риджидбоди
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = data.gravity;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        // Другие настройки будут применены при Initialize
    }
        
    System.Collections.IEnumerator RecoilCoroutine()
    {
        Vector3 originalPosition = transform.localPosition;
        transform.localPosition -= transform.up * currentWeapon.recoilAmount;
        yield return new WaitForSeconds(currentWeapon.recoilDuration);
        transform.localPosition = originalPosition;
    }
    
    void StartReload()
    {
        if (!currentWeapon.usesAmmo || isReloading || currentAmmo >= currentWeapon.maxAmmo) return;
    
        isReloading = true;
    
        // Уведомляем UI о начале перезарядки - используем WeaponUI вместо WeaponUIAdvanced
        WeaponUI ui = FindObjectOfType<WeaponUI>();
        if (ui != null && ui.reloadSlider != null)
        {
            ui.reloadSlider.gameObject.SetActive(true);
            // Запускаем корутину для обновления слайдера
            StartCoroutine(UpdateReloadSlider(ui.reloadSlider));
        }
    
        StartCoroutine(ReloadCoroutine());
    }
    
    // Добавляем корутину для обновления слайдера
    System.Collections.IEnumerator UpdateReloadSlider(UnityEngine.UI.Slider slider)
    {
        float reloadTime = currentWeapon.reloadTime;
        float elapsedTime = 0f;
    
        slider.value = 0f;
    
        while (elapsedTime < reloadTime)
        {
            elapsedTime += Time.deltaTime;
            slider.value = elapsedTime / reloadTime;
            yield return null;
        }
    
        slider.value = 1f;
        slider.gameObject.SetActive(false);
    }
    
    System.Collections.IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(currentWeapon.reloadTime);
        currentAmmo = currentWeapon.maxAmmo;
        isReloading = false;
        Debug.Log($"{currentWeapon.weaponName} reloaded!");
    }
}