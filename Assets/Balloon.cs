using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField] private BalloonPopEffect popEffectPrefab;
    
    private Renderer balloonRenderer;
    private BallSpawner spawner;
    
    private void Start()
    {
        balloonRenderer = GetComponent<Renderer>();
        
        // Найти спаунер, чтобы уведомить его при уничтожении шарика
        spawner = FindObjectOfType<BallSpawner>();
    }
    
    // Call this when crossbow bolt hits the balloon
    public void OnHit()
    {
        // Create pop effect
        if (popEffectPrefab != null)
        {
            // Используем текущий цвет шарика из рендерера
            Color currentColor = balloonRenderer != null ? 
                balloonRenderer.material.color : Color.white;
            
            BalloonPopEffect popEffect = Instantiate(popEffectPrefab);
            popEffect.PopBalloon(transform.position, currentColor);
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