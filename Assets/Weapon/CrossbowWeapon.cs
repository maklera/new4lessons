using UnityEngine;

[CreateAssetMenu(fileName = "Crossbow", menuName = "Weapon Data/Crossbow")]
public class CrossbowWeapon : WeaponData
{
    // Арбалет стреляет медленно, но точно
    public override void OnShoot(WeaponController controller)
    {
        // Используем стандартный метод создания снаряда, так как арбалет стреляет прямо
        base.OnShoot(controller);
    }
}