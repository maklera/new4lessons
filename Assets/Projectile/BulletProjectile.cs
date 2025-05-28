// BulletProjectile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Bullet", menuName = "Projectiles/Bullet")]
public class BulletProjectile : ProjectileData
{
    [Header("Bullet Specific")]
    public float ricochets = 0;
    public float ricochctStrength = 0.8f;
    public LayerMask ricochetLayers;
    
    private int currentRicochets = 0;
    
    public override void OnImpact(GameObject projectile, GameObject target)
    {
        if (((1 << target.layer) & ricochetLayers) != 0 && currentRicochets < ricochets)
        {
            currentRicochets++;
            HandleRicochet(projectile);
        }
        else
        {
            base.OnImpact(projectile, target);
        }
    }
    
    private void HandleRicochet(GameObject projectile)
    {
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Меняем направление с уменьшением скорости
            Vector2 direction = Vector2.Reflect(rb.linearVelocity.normalized, Vector2.up);
            rb.linearVelocity = direction * speed * ricochctStrength;
        }
    }
}