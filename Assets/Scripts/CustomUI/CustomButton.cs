using UnityEngine;
using System;
using TMPro;
using Ui;
using UnityEditor;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider2D))]
public class CustomButton : CustomImage
{
    [Header("Button Settings")] public Color NormalColor = Color.white;
    public Color HoverColor = new Color(0.9f, 0.9f, 0.9f);
    public Color PressedColor = new Color(0.7f, 0.7f, 0.7f);

    [HideInInspector] public bool _isPressed = false;
    [HideInInspector] public bool _isHovered = false;

    protected BoxCollider2D _collider;

    private bool _isClickable;

    public void SetClickable(bool isClickable)
    {
        _isClickable = isClickable;
    }
    
    public override void Initialize(UiManager uiManager)
    {
        base.Initialize(uiManager);
        _collider = GetComponent<BoxCollider2D>();
        //SpriteRenderer.color = normalColor;
        UpdateButtonState();
        SetClickable(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //SpriteRenderer.color = normalColor;
        //UpdateButtonState();
    }

    protected override void Update()
    {
        base.Update();
        if (!_isClickable)
        {
            return;
        }
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
                UiManager.GameManager.EventManager.UpdateButtonClicked();
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
            SpriteRenderer.color = PressedColor;
        }
        else if (_isHovered)
        {
            SpriteRenderer.color = HoverColor;
        }
        else
        {
            SpriteRenderer.color = NormalColor;
        }

        _isDirty = true;
    }

}