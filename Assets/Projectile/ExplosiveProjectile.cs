// ExplosiveProjectile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "Explosive", menuName = "Projectiles/Explosive")]
public class ExplosiveProjectile : ProjectileData
{
    [Header("Explosive Specific")]
    public float fuseTime = 3f;
    public bool explodeOnImpact = true;
    public AnimationCurve explosionCurve;
    
    public void InitializeExplosive(GameObject projectile)
    {
        if (!explodeOnImpact)
        {
            // Запускаем отложенный взрыв через ProjectileController
            ProjectileController controller = projectile.GetComponent<ProjectileController>();
            if (controller != null)
            {
                projectile.SendMessage("StartFuseCountdown", fuseTime, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    public override void OnImpact(GameObject projectile, GameObject target)
    {
        if (explodeOnImpact)
        {
            Explode(projectile);
        }
    }
    
    private void Explode(GameObject projectile)
    {
        // Воспроизведение взрыва
        CreateExplosion(projectile.transform.position);
        Object.Destroy(projectile);
    }
}