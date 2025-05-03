using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField] private BalloonPopEffect popEffectPrefab;
    [SerializeField] private Color balloonColor = Color.red;
    
    private Renderer balloonRenderer;
    
    private void Start()
    {
        balloonRenderer = GetComponent<Renderer>();
        if (balloonRenderer != null)
        {
            // Set the balloon color
            balloonRenderer.material.color = balloonColor;
        }
    }
    
    // Call this when crossbow bolt hits the balloon
    public void OnHit()
    {
        // Create pop effect
        if (popEffectPrefab != null)
        {
            // Use the balloon's actual color from renderer if available
            Color actualColor = balloonRenderer != null ? balloonRenderer.material.color : balloonColor;
            
            BalloonPopEffect popEffect = Instantiate(popEffectPrefab);
            popEffect.PopBalloon(transform.position, actualColor);
        }
        
        // Destroy the balloon object
        Destroy(gameObject);
    }
    
    // If you're using colliders/triggers to detect hits
    private void OnCollisionEnter(Collision collision)
    {
        // Check if hit by crossbow bolt
        if (collision.gameObject.CompareTag("CrossbowBolt"))
        {
            OnHit();
        }
    }
    
    // Alternative trigger-based detection
    private void OnTriggerEnter(Collider other)
    {
        // Check if hit by crossbow bolt
        if (other.CompareTag("CrossbowBolt"))
        {
            OnHit();
        }
    }
}