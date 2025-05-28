// ArrowProjectile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Arrow", menuName = "Projectiles/Arrow")]
public class ArrowProjectile : ProjectileData
{
    [Header("Arrow Specific")]
    public bool pierceBalloons = true;
    public int maxPiercings = 1;
    
    public override void OnImpact(GameObject projectile, GameObject target)
    {
        if (target.CompareTag("Ball") && pierceBalloons)
        {
            Balloon balloon = target.GetComponent<Balloon>();
            if (balloon != null)
            {
                balloon.OnHit();
            }
        }
        
        base.OnImpact(projectile, target);
    }
}