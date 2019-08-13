
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/TiledImage")]
public class TiledImage : RawImage
{
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        ChangeRect();
    }

    public void ChangeRect()
    {
        Vector2 size = rectTransform.sizeDelta;
        uvRect = new Rect(0, 0, size.x / texture.width, size.y / texture.height);
    }
}