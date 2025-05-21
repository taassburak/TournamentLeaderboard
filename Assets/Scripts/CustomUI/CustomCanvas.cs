using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif
public class CustomCanvas : MonoBehaviour
{
    [Header("Canvas Settings")]
    public Camera targetCamera;
    [Range(0, 10)] public int sortingOrder = 0;
    public Color gizmoColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);

    private List<CustomUIElement> _uiElements = new List<CustomUIElement>();

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        RefreshUIElements();
    }

    private void OnEnable()
    {
        RefreshUIElements();
    }

    public void RefreshUIElements()
    {
        _uiElements.Clear();
        foreach (Transform child in transform)
        {
            CustomUIElement element = child.GetComponent<CustomUIElement>();
            if (element != null)
            {
                _uiElements.Add(element);
                element.SetCanvas(this);
            }
        }
    }
    
    public void RefreshAllElements()
    {
        foreach (var element in _uiElements)
        {
            element.UpdateVisuals();
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
            
        if (targetCamera != null)
        {
            Gizmos.color = gizmoColor;
            
            float distance = targetCamera.nearClipPlane + 0.01f;
            Vector3 center = targetCamera.transform.position + targetCamera.transform.forward * distance;
            
            float height = 2.0f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * targetCamera.aspect;
            
            Vector3 topLeft = center + targetCamera.transform.up * (height * 0.5f) - targetCamera.transform.right * (width * 0.5f);
            Vector3 topRight = center + targetCamera.transform.up * (height * 0.5f) + targetCamera.transform.right * (width * 0.5f);
            Vector3 bottomLeft = center - targetCamera.transform.up * (height * 0.5f) - targetCamera.transform.right * (width * 0.5f);
            Vector3 bottomRight = center - targetCamera.transform.up * (height * 0.5f) + targetCamera.transform.right * (width * 0.5f);
            
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
            
            Handles.Label(center, "Custom Canvas");
        }
    }
    
    private void OnValidate()
    {
        RefreshUIElements();
        RefreshAllElements();
    }
    #endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomCanvas))]
public class CustomCanvasEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CustomCanvas canvas = (CustomCanvas)target;
        
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        
        if (EditorGUI.EndChangeCheck())
        {
            canvas.RefreshUIElements();
            canvas.RefreshAllElements();
        }
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Refresh All UI Elements"))
        {
            canvas.RefreshUIElements();
            canvas.RefreshAllElements();
            EditorUtility.SetDirty(canvas);
            SceneView.RepaintAll();
        }
    }
}
#endif