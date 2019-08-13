using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 行军路线
/// </summary>
public class MapLine : MonoBehaviour
{ 
    private Rect uvRect;
    private TiledImage tiledImage;
    private RectTransform rectTransform;

    void Awake()
    {
        tiledImage = GetComponent<TiledImage>();
        rectTransform = GetComponent<RectTransform>();
        uvRect = tiledImage.uvRect;
    }

    /// <summary>
    /// 设置材质的Offset的属性，让箭头移动起来
    /// </summary>
    private void Update()
    {
        if (uvRect.y <= 0) uvRect.y = 100;
        uvRect.y -= 0.1f;
        tiledImage.uvRect = uvRect;
    }

    /// <summary>
    /// 赋值线条
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    public void SetLine(Vector3 startPos, Vector3 endPos)
    {
        var rect = rectTransform.sizeDelta;
        rect.y = Vector2.Distance(startPos, endPos);//线条长度

        var tempVec = endPos - startPos;
        var angle = Vector3.Angle(tempVec, Vector3.right);
        angle = 180 + angle;
        if (tempVec.y < 0) angle = 360 - angle;
        angle = 90 + angle;
        var quat = Quaternion.AngleAxis(angle, Vector3.forward);//线条角度
        rectTransform.localPosition = startPos;
        rectTransform.sizeDelta = rect;
        rectTransform.localRotation = quat;
        tiledImage.ChangeRect();
        uvRect = tiledImage.uvRect;
    }
}