using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string scorePrefix = "Счет: ";
    
    [Header("Scoring")]
    [SerializeField] private int pointsPerBalloon = 10;
    
    private int currentScore = 0;
    private int balloonsPopped = 0;
    
    // Singleton instance
    public static ScoreManager Instance { get; private set; }
    
    private void Awake()
    {
        // Simple singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        UpdateScoreDisplay();
    }
    
    // Called when a balloon is popped
    public void OnBalloonPopped()
    {
        balloonsPopped++;
        currentScore += pointsPerBalloon;
        UpdateScoreDisplay();
    }
    
    // Update the UI display
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{scorePrefix}{currentScore}";
        }
    }
    
    // Public getters for score values
    public int GetCurrentScore() => currentScore;
    public int GetBalloonsPopped() => balloonsPopped;
}