using UnityEngine;

public class CrossbowController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    [Header("Aim Settings")]
    public bool showAimLine = true;
    public LineRenderer aimLine;
    public float aimLineLength = 10f;

    [Header("Shooting Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    public float arrowForce = 20f;
    public bool autoFire = false;
    public AudioClip shootSound;

    private Camera mainCamera;
    private Vector3 targetDirection;
    private Quaternion targetRotation;
    private float nextFireTime = 0f;
    private AudioSource audioSource;

    void Start()
    {
        mainCamera = Camera.main;

        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0, 0.5f, 0); // теперь спереди
            firePoint = fp.transform;
        }

        if (showAimLine && aimLine == null)
        {
            aimLine = GetComponentInChildren<LineRenderer>();

            if (aimLine == null)
            {
                GameObject lineObj = new GameObject("AimLine");
                lineObj.transform.SetParent(transform);
                lineObj.transform.localPosition = Vector3.zero;

                aimLine = lineObj.AddComponent<LineRenderer>();
                aimLine.positionCount = 2;
                aimLine.startWidth = 0.05f;
                aimLine.endWidth = 0.05f;
                aimLine.material = new Material(Shader.Find("Sprites/Default"));
                aimLine.startColor = new Color(1, 0, 0, 0.5f);
                aimLine.endColor = new Color(1, 0, 0, 0.2f);
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    void Update()
    {
        RotateTowardsMouse();
        UpdateAimLine();
        HandleShooting();
    }

    void RotateTowardsMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        targetDirection = worldPosition - transform.position;

        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        angle -= 90f; // теперь мы поворачиваем ВВЕРХ

        targetRotation = Quaternion.Euler(0, 0, angle);

        if (rotationSpeed > 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    void UpdateAimLine()
    {
        if (showAimLine && aimLine != null)
        {
            Vector3 direction = transform.up;
            aimLine.SetPosition(0, firePoint.position);
            aimLine.SetPosition(1, firePoint.position + direction * aimLineLength);
        }
    }

    void HandleShooting()
    {
        if (Time.time >= nextFireTime)
        {
            if (Input.GetButtonDown("Fire1") || (autoFire && Input.GetButton("Fire1")))
            {
                Shoot();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }
    }

    void Shoot()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("Arrow prefab is not assigned to CrossbowController!");
            return;
        }

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);

        Vector2 direction = transform.up;

        ArrowController arrowController = arrow.GetComponent<ArrowController>();
        if (arrowController != null)
        {
            arrowController.Launch(direction);
        }
        else
        {
            Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(direction * arrowForce, ForceMode2D.Impulse);
            }
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.clip = shootSound;
            audioSource.Play();
        }

        AddRecoilEffect();
    }

    void AddRecoilEffect()
    {
        StartCoroutine(RecoilCoroutine());
    }

    System.Collections.IEnumerator RecoilCoroutine()
    {
        Vector3 originalPosition = transform.localPosition;
        transform.localPosition -= transform.up * 0.1f;
        yield return new WaitForSeconds(0.05f);
        transform.localPosition = originalPosition;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position, transform.position + targetDirection.normalized * 2);
        }

        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + transform.up * 1.5f);
        }
    }
}
