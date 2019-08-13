using Framework.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 长按等实现
/// </summary>
[AddComponentMenu("UI/OnImage", 101)]
[RequireComponent(typeof(EventTrigger))]
public class OnImage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    public float Ping;                                      //长按时间间隔
    public float Distance;                                  //按下抬起屏幕间隔距离
    public UnityEvent onDistance { get; set; }              //按下抬起鼠标移动距离不超过Distance
    public UnityEvent onLongPress { get; set; }             //长按事件
    public UnityEvent onPointerDown { get; set; }           //按下事件
    public UnityEvent onPointerUp { get; set; }             //抬起事件

    public UnityEvent onEnter { get; set; }                 //进入事件
    public UnityEvent onExit { get; set; }                  //离开事件

    public UnityEvent onDrag { get; set; }                  //拖拽事件
    public UnityEvent onBeginDrag { get; set; }             //开始拖拽事件
    public UnityEvent onEndDrag { get; set; }               //拖拽结束事件
    public UnityEvent onDrop { get; set; }                  //拖拽放下事件

    private bool IsLongPress = false;                       //按下，开始计时
    private float LastTime = 0;                             //按下时间

    private bool IsPointerDown = false;                     //是否按下
    private bool IsPointerUp = false;                       //是否抬起
    private Vector3 DownPos = Vector3.zero;                 //按下时坐标
    private Vector3 UpPos = Vector3.zero;                   //抬起时坐标

    void Awake()
    {
        onDistance = new UnityEvent();
        onLongPress = new UnityEvent();
        onPointerDown = new UnityEvent();
        onPointerUp = new UnityEvent();
        onEnter = new UnityEvent();
        onExit = new UnityEvent();

        onDrag = new UnityEvent();
        onBeginDrag = new UnityEvent();
        onEndDrag = new UnityEvent();
        onDrop = new UnityEvent();
    }

    void Update()
    {
        if (IsLongPress && Ping > 0 && Time.time - LastTime > Ping)
        {
            IsLongPress = false;
            if (null != onLongPress) onLongPress.Invoke(); 
        }

        if (IsPointerDown && IsPointerUp && Distance > 0 && Vector3.Distance(DownPos, UpPos) < Distance)
        {
            IsPointerDown = false;
            IsPointerUp = false;
            if (null != onDistance) onDistance.Invoke();
        }
    }

    /// <summary>
    /// 按下
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (null != onPointerDown) onPointerDown.Invoke();
        LongPress(true);
        IsPointerDown = true;
        DownPos = Input.mousePosition;
    }
    /// <summary>
    /// 抬起
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (null != onPointerUp) onPointerUp.Invoke();
        if (IsLongPress) LongPress(false);
        IsPointerUp = true;
        UpPos = Input.mousePosition;
    }

    /// <summary>
    /// 离开
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (null != onExit) onExit.Invoke();
    }

    /// <summary>
    /// 进入
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (null != onEnter) onEnter.Invoke();
    }
    
    /// <summary>
    /// 开始长按
    /// </summary>
    /// <param name="bStart"></param>
    public void LongPress(bool isStart)
    {
        IsLongPress = isStart;
        LastTime = Time.time;
    }

    /// <summary>
    /// 拖拽每帧
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (null != onDrag) onDrag.Invoke();
    }

    /// <summary>
    /// 开始拖拽
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (null != onBeginDrag) onBeginDrag.Invoke();
    }

    /// <summary>
    /// 结束拖拽
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (null != onEndDrag) onEndDrag.Invoke();
    }

    /// <summary>
    /// 放下拖拽
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrop(PointerEventData eventData)
    {
        if (null != onDrop) onDrop.Invoke();
    }

    private void OnDestroy()
    {
        if (null != onDistance)
        {
            onDistance.RemoveAllListeners();
            onDistance = null;
        }

        if (null != onLongPress)
        {
            onLongPress.RemoveAllListeners();
            onLongPress = null;
        }

        if (null != onPointerDown)
        {
            onPointerDown.RemoveAllListeners();
            onPointerDown = null;
        }

        if (null != onPointerUp)
        {
            onPointerUp.RemoveAllListeners();
            onPointerUp = null;
        }

        if (null != onEnter)
        {
            onEnter.RemoveAllListeners();
            onEnter = null;
        }

        if (null != onExit)
        {
            onExit.RemoveAllListeners();
            onExit = null;
        }

        if (null != onDrag)
        {
            onDrag.RemoveAllListeners();
            onDrag = null;
        }

        if (null != onBeginDrag)
        {
            onBeginDrag.RemoveAllListeners();
            onBeginDrag = null;
        }

        if (null != onEndDrag)
        {
            onEndDrag.RemoveAllListeners();
            onEndDrag = null;
        }

        if (null != onDrop)
        {
            onDrop.RemoveAllListeners();
            onDrop = null;
        }
    }
}
