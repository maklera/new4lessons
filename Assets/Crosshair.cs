using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public bool hideCursor = true;      // Скрывать ли системный курсор
    public float smoothing = 0.0f;      // 0 для мгновенного следования, >0 для плавного
    
    private Camera mainCamera;
    private Vector3 targetPosition;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Скрываем системный курсор
        if (hideCursor)
        {
            Cursor.visible = false;
        }
    }
    
    void Update()
    {
        // Получаем позицию мыши в мировых координатах
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        targetPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        
        // Устанавливаем Z позицию в 0 (для 2D игры)
        targetPosition.z = 0;
        
        // Плавно или мгновенно перемещаем прицел
        if (smoothing > 0)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);
        }
        else
        {
            transform.position = targetPosition;
        }
    }
    
    // Вернуть системный курсор при выходе
    void OnDestroy()
    {
        Cursor.visible = true;
    }
}