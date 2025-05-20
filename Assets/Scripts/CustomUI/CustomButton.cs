using UnityEngine;
using System;
using TMPro;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider2D))]
public class CustomButton : CustomImage
{
    [Header("Button Settings")] public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
    public Color pressedColor = new Color(0.7f, 0.7f, 0.7f);

    [HideInInspector] public bool _isPressed = false;
    [HideInInspector] public bool _isHovered = false;


    public Action OnClick;

    protected override void Awake()
    {
        base.Awake();
        SpriteRenderer.color = normalColor;
        UpdateButtonState();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SpriteRenderer.color = normalColor;
        UpdateButtonState();
    }

    protected override void Update()
    {
        base.Update();
        HandleInput();
    }

    private void HandleInput()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z); 

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector2 rayOrigin = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);

        if (hit.transform != null)
        {
            Debug.Log(hit.transform.name);
        }

        if (hit.collider == _collider)
        {
            if (!_isHovered)
            {
                _isHovered = true;
                UpdateButtonState();
            }

            if (Input.GetMouseButtonDown(0))
            {
                _isPressed = true;
                UpdateButtonState();
            }
            else if (Input.GetMouseButtonUp(0) && _isPressed)
            {
                _isPressed = false;
                UpdateButtonState();
                OnClick?.Invoke();
            }
        }
        else
        {
            if (_isHovered)
            {
                _isHovered = false;
                _isPressed = false;
                UpdateButtonState();
            }
        }

        if (_isPressed && Input.GetMouseButtonUp(0))
        {
            _isPressed = false;
            _isHovered = false;
            UpdateButtonState();
        }
    }

    private void UpdateButtonState()
    {
        if (SpriteRenderer == null)
        {
            return;
        }

        if (_isPressed)
        {
            SpriteRenderer.color = pressedColor;
        }
        else if (_isHovered)
        {
            SpriteRenderer.color = hoverColor;
        }
        else
        {
            SpriteRenderer.color = normalColor;
        }

        _isDirty = true;
    }

}