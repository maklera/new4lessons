using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 5f;

    private Rigidbody2D rb;
    private bool stuck = false;
    private float destroyTime;
    
    [Header("Sound Settings")]
    public AudioClip hitSound;            // Звуковой эффект при попадании
    public AudioClip flySound;
    
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null && (hitSound != null || flySound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D звук
        }
        destroyTime = Time.time + lifetime;
    }

    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    void Update()
    {
        if (!stuck && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }

        if (Time.time > destroyTime && !stuck)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            // Получаем компонент Balloon из объекта, в который попали
            Balloon balloon = other.GetComponent<Balloon>();
        
            // Проверяем, есть ли компонент Balloon на объекте
            if (balloon != null)
            {
                // Вызываем метод OnHit у объекта шарика
                balloon.OnHit();
            }
        
            // Уничтожаем шарик (хотя это лучше делать в самом OnHit)
            // Balloon.OnHit уже должен содержать Destroy(gameObject), поэтому эта строка не нужна
            // Destroy(other.gameObject);
        
            Debug.Log("Ball hit — destroyed.");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!stuck && collision.collider.CompareTag("Wall"))
        {
            StickToSurface();
        }
    }

    private void StickToSurface()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
        if (audioSource != null && hitSound != null)
        {
            audioSource.Stop(); // Останавливаем звук полета
            audioSource.clip = hitSound;
            audioSource.Play();
        }
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        Destroy(gameObject, 10f);
    }

}