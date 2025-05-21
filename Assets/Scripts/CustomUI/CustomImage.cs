using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class CustomImage : CustomUIElement
{

    public SpriteRenderer SpriteRenderer { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

    }


    public void SetColor(Color color)
    {
        SpriteRenderer.color = color;
    }
}