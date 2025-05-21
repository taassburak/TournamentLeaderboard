using Ui;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class CustomImage : CustomUIElement
{

    public SpriteRenderer SpriteRenderer { get; private set; }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void Initialize(UiManager uiManager)
    {
        base.Initialize(uiManager);
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color color)
    {
        SpriteRenderer.color = color;
    }
}