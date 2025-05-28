using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ProjectileData))]
public class ProjectileDataEditor : Editor
{
    private GameObject previewObject;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D triggerCollider;
    private CircleCollider2D physicsCollider;
    
    public override void OnInspectorGUI()
    {
        ProjectileData projectileData = (ProjectileData)target;
        
        // Отображаем стандартные поля инспектора
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        
        // Добавляем кнопку для предварительного просмотра
        if (GUILayout.Button("Preview Colliders"))
        {
            CreatePreviewObject(projectileData);
        }
        
        if (GUILayout.Button("Update Preview"))
        {
            UpdatePreviewObject(projectileData);
        }
        
        if (GUILayout.Button("Clear Preview"))
        {
            DestroyPreviewObject();
        }
    }
    
    private void CreatePreviewObject(ProjectileData data)
    {
        // Уничтожаем старый объект, если он существует
        DestroyPreviewObject();
        
        // Создаем новый объект предварительного просмотра
        previewObject = new GameObject("ProjectilePreview");
        previewObject.hideFlags = HideFlags.DontSave; // Объект не будет сохранен
        
        // Добавляем компоненты
        spriteRenderer = previewObject.AddComponent<SpriteRenderer>();
        triggerCollider = previewObject.AddComponent<CircleCollider2D>();
        physicsCollider = previewObject.AddComponent<CircleCollider2D>();
        
        // Настраиваем объект
        UpdatePreviewObject(data);
        
        // Выбираем созданный объект
        Selection.activeGameObject = previewObject;
        
        // Помещаем на сцену в видимом месте
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            previewObject.transform.position = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
        }
    }
    
    private void UpdatePreviewObject(ProjectileData data)
    {
        if (previewObject == null) return;
        
        // Обновляем спрайт и размер
        if (spriteRenderer != null && data.projectileSprite != null)
        {
            spriteRenderer.sprite = data.projectileSprite;
            previewObject.transform.localScale = Vector3.one * data.scale;
        }
        
        // Обновляем триггер-коллайдер
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            triggerCollider.radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale : 0.7f);
            triggerCollider.offset = Vector2.zero;
        }
        
        // Обновляем физический коллайдер
        if (physicsCollider != null)
        {
            physicsCollider.isTrigger = false;
            physicsCollider.radius = 0.5f * (data.colliderScale != 0 ? data.colliderScale * 0.7f : 0.5f);
            physicsCollider.offset = Vector2.zero;
        }
        
        // Визуально выделяем коллайдеры разными цветами в Scene окне
        EditorApplication.delayCall += () => 
        {
            if (triggerCollider != null)
            {
                EditorGUIUtility.PingObject(triggerCollider);
            }
        };
    }
    
    private void DestroyPreviewObject()
    {
        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
            previewObject = null;
            spriteRenderer = null;
            triggerCollider = null;
            physicsCollider = null;
        }
    }
    
    private void OnDisable()
    {
        DestroyPreviewObject();
    }
}
#endif