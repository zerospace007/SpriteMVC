/// <summary>
/// 单位接口
/// </summary>
public interface IAStarUnit
{
    /// <summary>
    /// 是否可以通过
    /// </summary>
    /// <value><c>true</c> if is passable; otherwise, <c>false</c>.</value>
    bool IsPassable { get; set; }

    /// <summary>
    /// 添加通过回调函数
    /// </summary>
    /// <param name="callback">Callback.</param>
    void AddIsPassableChange(AStarCallback.IsPassableChangeCallback callback);

    /// <summary>
    /// 移除通过回调函数
    /// </summary>
    /// <param name="callback">Callback.</param>
    void RemoveIsPassableChange(AStarCallback.IsPassableChangeCallback callback);
}