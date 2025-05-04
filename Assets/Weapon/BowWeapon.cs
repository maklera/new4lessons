using UnityEngine;

[CreateAssetMenu(fileName = "Bow", menuName = "Weapon Data/Bow")]
public class BowWeapon : WeaponData
{
    [Header("Bow Specific")]
    public float chargeTime = 2f;      // Время зарядки для полной силы
    public AnimationCurve chargeCurve; // Кривая зарядки
    
    public override void OnShoot(WeaponController controller)
    {
        base.OnShoot(controller);
    }
}