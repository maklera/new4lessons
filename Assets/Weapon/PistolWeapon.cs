using UnityEngine;

[CreateAssetMenu(fileName = "Pistol", menuName = "Weapon Data/Pistol")]
public class PistolWeapon : WeaponData
{
    // Пистолет можно быстро стрелять, но нужно целиться
    public override void OnShoot(WeaponController controller)
    {
        base.OnShoot(controller);
    }
}