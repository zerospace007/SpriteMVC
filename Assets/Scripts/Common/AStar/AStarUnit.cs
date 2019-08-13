public class AStarUnit : IAStarUnit
{
    /// <summary>
    /// 是否可以通过
    /// </summary>
    private bool _isPassable;

    private readonly AStarCallback _aStarCallback = new AStarCallback();

    /// <summary>
    /// 添加通过回调函数
    /// </summary>
    /// <param name="callback">Callback.</param>
    public void AddIsPassableChange(AStarCallback.IsPassableChangeCallback callback)
    {
        _aStarCallback.OnIsPassableChange += callback;
    }

    /// <summary>
    /// 移除通过回调函数
    /// </summary>
    /// <param name="callback">Callback.</param>
    public void RemoveIsPassableChange(AStarCallback.IsPassableChangeCallback callback)
    {
        _aStarCallback.OnIsPassableChange -= callback;
    }

    /// <summary>
    /// 是否可以通过
    /// </summary>
    /// <value>true</value>
    /// <c>false</c>
    public bool IsPassable
    {
        get { return _isPassable; }
        set
        {
            if (_isPassable == value) return;
            _isPassable = value;
            _aStarCallback.InvokeIsPassableChange();
        }
    }
}