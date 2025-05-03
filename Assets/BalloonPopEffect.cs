using UnityEngine;

public class BalloonPopEffect : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem popParticleSystem;
    [SerializeField] private Color[] balloonColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow };
    
    [Header("Audio Effects")]
    [SerializeField] private AudioClip popSound;
    [SerializeField] private float volumeMin = 0.8f;
    [SerializeField] private float volumeMax = 1.0f;
    [SerializeField] private float pitchMin = 0.9f;
    [SerializeField] private float pitchMax = 1.1f;

    // Call this method when a balloon is hit
    public void PopBalloon(Vector3 position, Color balloonColor)
    {
        // Visual effect
        if (popParticleSystem != null)
        {
            ParticleSystem newPopEffect = Instantiate(popParticleSystem, position, Quaternion.identity);
            
            // Change particle color to match balloon color
            var mainModule = newPopEffect.main;
            mainModule.startColor = balloonColor;
            
            newPopEffect.Play();
            
            // Auto destroy after effect completes
            Destroy(newPopEffect.gameObject, newPopEffect.main.duration + 0.5f);
        }
        
        // Audio effect
        if (popSound != null)
        {
            // Create temporary AudioSource to play the pop sound
            GameObject audioObj = new GameObject("PopSound");
            audioObj.transform.position = position;
            AudioSource audioSource = audioObj.AddComponent<AudioSource>();
            
            // Configure the audio source
            audioSource.clip = popSound;
            audioSource.volume = Random.Range(volumeMin, volumeMax);
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.Play();
            
            // Destroy audio object when sound finishes playing
            Destroy(audioObj, popSound.length + 0.1f);
        }
    }
}