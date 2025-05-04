using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField] private GameObject popEffectPrefab;
    
    private SpriteRenderer balloonRenderer;
    private BallSpawner spawner;
    
    private void Start()
    {
        balloonRenderer = GetComponent<SpriteRenderer>();
        
        // Найти спаунер, чтобы уведомить его при уничтожении шарика
        spawner = FindObjectOfType<BallSpawner>();
    }
    
    // Call this when crossbow bolt hits the balloon
    public void OnHit()
    {
        // Create pop effect
        if (popEffectPrefab != null)
        {
            // Используем текущий цвет шарика из SpriteRenderer
            Color currentColor = balloonRenderer != null ? 
                balloonRenderer.color : Color.white;
            
            // Создаем эффект на позиции шарика
            GameObject popEffectObj = Instantiate(popEffectPrefab, transform.position, Quaternion.identity);
            
            // Получаем компонент BalloonPopEffect
            BalloonPopEffect popEffect = popEffectObj.GetComponent<BalloonPopEffect>();
            
            // Запускаем эффект с правильным цветом
            if (popEffect != null)
            {
                popEffect.PopBalloon(transform.position, currentColor);
                
                // Удаляем эффект через время
                Destroy(popEffectObj, 2f); // 2 секунды должно хватить для любого эффекта
            }
            else
            {
                // Если компонент BalloonPopEffect не найден, просто удаляем объект
                Destroy(popEffectObj, 1f);
            }
        }
        
        // Увеличиваем счет
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnBalloonPopped();
        }
        
        // Уведомляем спаунер об уничтожении шарика
        if (spawner != null)
        {
            spawner.DecreaseBallCount();
        }
        
        // Destroy the balloon object
        Destroy(gameObject);
    }
}