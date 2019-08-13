using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 新手引导动画
/// </summary>
public class Guide : MonoBehaviour, ICanvasRaycastFilter
{
    private const float SmoothTime = 0.3f;      //平滑
    private Vector4 center = Vector4.zero;      //Mask 中心位置
    private Material material;                  //Mask 材质（镂空一个圆）
    private float current = 0f;                 //当前直径     
    private float diameter = 1000f;             //直径
    private Rect rect;                          //可点击区域
    private float yVelocity = 0f;
    private Canvas uiCanvas;                    //UI界面画布
    private Canvas mapCanvas;                   //地图画布

    public Canvas UICanvas
    {
        get
        {
            if (null == uiCanvas)
            {
                GameObject gameObj = GameObject.FindWithTag("UICanvas");
                if (gameObj != null) uiCanvas = gameObj.GetComponent<Canvas>();
            }
            return uiCanvas;
        }
    }

    public Canvas MapCanvas
    {
        get
        {
            if (null == mapCanvas)
            {
                GameObject gameObj = GameObject.FindWithTag("MapCanvas");
                if (gameObj != null) mapCanvas = gameObj.GetComponent<Canvas>();
            }
            return mapCanvas;
        }
    }

    void Awake()
    {
        material = GetComponent<Image>().material;
    }

    /// <summary>
    /// 设置引导目标
    /// </summary>
    /// <param name="target"></param>
    /// <param name="isUICanvas"></param>
    public Vector2 SetTarget(RectTransform target, bool isUICanvas = true)
    {
        var canvas = isUICanvas ? UICanvas : MapCanvas;
        Camera camera = canvas.GetComponentInChildren<Camera>();
        var targetPos = target.position;
        if (!isUICanvas) targetPos = targetPos * 5;   //UI摄像机size为5，所以Map转化UI坐标时乘以5
        Vector2 rector = RectTransformUtility.WorldToScreenPoint(Camera.main, targetPos);
        CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
        float radio = Screen.width / canvasScaler.referenceResolution.x;    //适配
        Vector2 position = WorldToCanvasPos(canvas, camera, rector);
        diameter = target.sizeDelta.x * radio;
        float x = rector.x - target.sizeDelta.x * target.pivot.x * radio;
        float y = rector.y - target.sizeDelta.y * target.pivot.x * radio;
        center = new Vector4(position.x, position.y, 0f, 0f);
        rect = new Rect(x, y, target.sizeDelta.x * radio, target.sizeDelta.y * radio);
        Vector3[] corners = new Vector3[4];
        (canvas.transform as RectTransform).GetWorldCorners(corners);
        for (int node = 0, length = corners.Length; node < length; node++)
        {
            var distance = Vector3.Distance(WorldToCanvasPos(canvas, camera, corners[node]), center);
            current = Mathf.Max(distance, current);
        }
        material.SetVector("_Center", center);
        material.SetFloat("_Silder", current);
        return center;
    }

    void Update()
    {
        float value = Mathf.SmoothDamp(current, diameter, ref yVelocity, SmoothTime);
        if (!Mathf.Approximately(value, current))
        {
            current = value;
            material.SetFloat("_Silder", current);
        }
    }

    private Vector2 WorldToCanvasPos(Canvas canvas, Camera camera, Vector2 world)
    {
        Vector2 position = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, world, camera, out position);
        return position;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        bool isHas = rect.Contains(sp);
        //Debug.Log(sp);
        //Debug.Log(isHas);
        return !isHas;
    }
}