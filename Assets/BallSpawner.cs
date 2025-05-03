using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject ballPrefab;          // Префаб шарика
    public float spawnWidth = 6f;          // Ширина зоны спавна
    public float spawnRate = 0.5f;         // Частота спавна (шариков в секунду)
    public int maxBalls = 50;              // Максимальное количество шариков
    
    [Header("Ball Settings")]
    public Vector2 ballSizeRange = new Vector2(0.3f, 0.8f);  // Мин и макс размер шариков
    public Color[] ballColors;             // Массив возможных цветов
    public float initialForceRange = 1f;   // Диапазон случайной начальной силы
    
    private float nextSpawnTime;           // Время следующего спавна
    private int ballCount = 0;             // Текущее количество шариков
    
    void Start()
    {
        nextSpawnTime = Time.time;
        
        // Если не заданы цвета, установим стандартные
        if (ballColors == null || ballColors.Length == 0)
        {
            ballColors = new Color[] 
            { 
                Color.red, Color.blue, Color.green, 
                Color.yellow, Color.magenta, Color.cyan 
            };
        }
    }
    
    void Update()
    {
        // Проверка времени для спавна нового шарика
        if (Time.time >= nextSpawnTime && ballCount < maxBalls)
        {
            SpawnBall();
            nextSpawnTime = Time.time + (1f / spawnRate);
        }
    }
    
    void SpawnBall()
    {
        // Рандомная позиция в пределах ширины зоны спавна
        float randomX = Random.Range(-spawnWidth/2, spawnWidth/2);
        Vector3 spawnPos = transform.position + new Vector3(randomX, 0, 0);
        
        // Создание шарика
        GameObject newBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        
        // Установка случайного размера
        float randomSize = Random.Range(ballSizeRange.x, ballSizeRange.y);
        newBall.transform.localScale = new Vector3(randomSize, randomSize, 1);
        
        // Установка случайного цвета
        SpriteRenderer renderer = newBall.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = ballColors[Random.Range(0, ballColors.Length)];
        }
        
        // Добавление случайной начальной силы
        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomForce = new Vector2(Random.Range(-initialForceRange, initialForceRange), 0);
            rb.AddForce(randomForce, ForceMode2D.Impulse);
        }
        
    }
    
    // Метод для уменьшения счетчика при уничтожении шарика
    public void DecreaseBallCount()
    {
        ballCount--;
    }
}