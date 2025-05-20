using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif

public class CustomUIElement : MonoBehaviour
{
    [Header("UI Element Settings")] public Vector2 size = new Vector2(1, 1);
    public Vector2 pivot = new Vector2(0.5f, 0.5f); // 0,0=sol alt, 1,1=sağ üst
    public Color gizmoColor = new Color(0, 0.8f, 1f, 0.5f);

    [HideInInspector] public bool _isDirty = true;

    protected CustomCanvas parentCanvas;
    protected BoxCollider2D _collider;

    protected virtual void OnEnable()
    {
        //EnsureCollider();
        UpdateVisuals();
        UpdateCollider();
    }

    protected virtual void Awake()
    {
        //EnsureCollider();
        UpdateVisuals();
        UpdateCollider();
    }
    
    public void SetCanvas(CustomCanvas canvas)
    {
        parentCanvas = canvas;
    }

    protected virtual void Update()
    {
        if (_isDirty)
        {
            UpdateVisuals();
            UpdateCollider();
            _isDirty = false;
        }
    }

    public virtual void UpdateVisuals()
    {
        
    }

    public virtual void UpdateCollider()
    {
        if (_collider != null)
        {
            _collider.size = new Vector2(size.x, size.y);
        }

        transform.localScale = size;
    }

    public virtual void SetSize(Vector2 newSize)
    {
        size = newSize;
        _isDirty = true;
    }
    

}
#if UNITY_EDITOR
[CustomEditor(typeof(CustomUIElement), true)]
public class CustomUIElementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CustomUIElement element = (CustomUIElement)target;
        
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        
        if (EditorGUI.EndChangeCheck())
        {
            element._isDirty = true;
            element.UpdateVisuals();
            element.UpdateCollider();
            EditorUtility.SetDirty(element);
            SceneView.RepaintAll();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Left", GUILayout.Width(70)))
        {
            element.pivot = new Vector2(0, element.pivot.y);
            element._isDirty = true;
        }
        if (GUILayout.Button("Center", GUILayout.Width(70)))
        {
            element.pivot = new Vector2(0.5f, element.pivot.y);
            element._isDirty = true;
        }
        if (GUILayout.Button("Right", GUILayout.Width(70)))
        {
            element.pivot = new Vector2(1, element.pivot.y);
            element._isDirty = true;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Bottom", GUILayout.Width(70)))
        {
            element.pivot = new Vector2(element.pivot.x, 0);
            element._isDirty = true;
        }
        if (GUILayout.Button("Middle", GUILayout.Width(70)))
        {
            element.pivot = new Vector2(element.pivot.x, 0.5f);
            element._isDirty = true;
        }
        if (GUILayout.Button("Top", GUILayout.Width(70)))
        {
            element.pivot = new Vector2(element.pivot.x, 1);
            element._isDirty = true;
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Update UI Element"))
        {
            element.UpdateVisuals();
            element.UpdateCollider();
            EditorUtility.SetDirty(element);
            SceneView.RepaintAll();
        }
    }
}
#endif