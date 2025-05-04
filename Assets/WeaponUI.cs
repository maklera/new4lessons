using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoText;
    public Image weaponIcon;
    public Slider reloadSlider;
    
    private WeaponController weaponController;
    
    void Start()
    {
        weaponController = FindObjectOfType<WeaponController>();
        if (reloadSlider != null)
        {
            reloadSlider.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (weaponController == null || weaponController.currentWeapon == null) return;
        
        UpdateWeaponInfo();
        UpdateAmmoInfo();
    }
    
    void UpdateWeaponInfo()
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponController.currentWeapon.weaponName;
        }
        
        if (weaponIcon != null && weaponController.currentWeapon.weaponSprite != null)
        {
            weaponIcon.sprite = weaponController.currentWeapon.weaponSprite;
        }
    }
    
    void UpdateAmmoInfo()
    {
        if (ammoText != null && weaponController.currentWeapon.usesAmmo)
        {
            // Используем публичный метод GetCurrentAmmo()
            ammoText.text = $"Ammo: {weaponController.GetCurrentAmmo()}/{weaponController.currentWeapon.maxAmmo}";
        }
        else if (ammoText != null)
        {
            ammoText.text = "∞";
        }
    }
}