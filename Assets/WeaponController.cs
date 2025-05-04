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
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWeapon(3);
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
        if (currentWeapon.projectilePrefab == null) return;
        
        GameObject projectile = Instantiate(currentWeapon.projectilePrefab, firePoint.position, firePoint.rotation);
        Vector2 direction = transform.up;
        
        ArrowController arrowController = projectile.GetComponent<ArrowController>();
        if (arrowController != null)
        {
            arrowController.Launch(direction);
        }
        else
        {
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(direction * currentWeapon.projectileForce, ForceMode2D.Impulse);
            }
        }
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
        StartCoroutine(ReloadCoroutine());
    }
    
    System.Collections.IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(currentWeapon.reloadTime);
        currentAmmo = currentWeapon.maxAmmo;
        isReloading = false;
        Debug.Log($"{currentWeapon.weaponName} reloaded!");
    }
}