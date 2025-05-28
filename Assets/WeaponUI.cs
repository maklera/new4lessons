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
            
            // Если идет перезарядка, показываем это
            if (weaponController.GetIsReloading())
            {
                ammoText.text = "Reloading...";
                ammoText.color = Color.yellow;
            }
            else
            {
                ammoText.color = Color.white;
            }
        }
        else if (ammoText != null)
        {
            ammoText.text = "∞";
        }
    }
    
    // Новый метод для показа сообщения об ошибке
    public void ShowErrorMessage(string message, float duration = 2f)
    {
        StartCoroutine(ShowMessageCoroutine(message, duration));
    }
    
    private System.Collections.IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        if (weaponNameText != null)
        {
            // Сохраняем текущий текст
            string originalText = weaponNameText.text;
            Color originalColor = weaponNameText.color;
            
            // Показываем сообщение об ошибке
            weaponNameText.text = message;
            weaponNameText.color = Color.red;
            
            yield return new WaitForSeconds(duration);
            
            // Возвращаем исходный текст
            weaponNameText.text = originalText;
            weaponNameText.color = originalColor;
        }
    }
}