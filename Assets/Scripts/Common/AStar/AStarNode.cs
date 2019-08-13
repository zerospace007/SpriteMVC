#region

using System.Collections.Generic;

#endregion

public class AStarNode
{
    /// <summary>
    /// 通过回调函数
    /// </summary>
    private readonly AStarCallback _aStarCallback = new AStarCallback();

    /// <summary>
    /// 回调函数参数
    /// </summary>
    private AStarNode _aStarNodeParam;

    /// <summary>
    /// 二叉堆节点
    /// </summary>
    public BinaryHeapNode BinaryHeapNode;

    /// <summary>
    /// 从此节点到目标节点的代价(A星算法使用)
    /// </summary>
    public int F;

    /// <summary>
    /// 从起点到此节点的代价
    /// </summary>
    public int G;

    /// <summary>
    /// 与此节点相邻的可通过的邻节点
    /// </summary>
    public IList<AStarLinkNode> Links;

    /// <summary>
    /// 坐标 x
    /// </summary>
    public int NodeX;

    /// <summary>
    /// 坐标 y
    /// </summary>
    public int NodeY;

    /// <summary>
    /// 父节点
    /// </summary>
    public AStarNode ParentNode;

    /// <summary>
    /// 搜索路径的检查编号(确定是否被检查过)
    /// </summary>
    public int SearchPathCheckNum;

    /// <summary>
    /// 在此节点上的单位
    /// </summary>
    private readonly IList<IAStarUnit> _units;

    /// <summary>
    /// 是否能被穿越
    /// </summary>
    public bool Walkable;

    /// <summary>
    /// 可移动范围的检查编号(确定是否被检查过)
    /// </summary>
    public int WalkableRangeCheckNum;

    /// <summary>
    /// 地图节点
    /// </summary>
    /// <param name="nodeX">Node x.</param>
    /// <param name="nodeY">Node y.</param>
    public AStarNode(int nodeX, int nodeY)
    {
        this.NodeX = nodeX;
        this.NodeY = nodeY;

        Walkable = true;
        _units = new List<IAStarUnit>();
    }

    public int UnitCount
    {
        get { return _units.Count; }
    }

    /// <summary>
    /// 添加穿越代价被修改后的回调函数
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="aStarNodeParam">A star Node parameter.</param>
    public void AddHeuristic(AStarCallback.HeuristicCallback callback, AStarNode aStarNodeParam)
    {
        this._aStarNodeParam = aStarNodeParam;
        _aStarCallback.OnHeuristic += callback;
    }

    /// <summary>
    /// 移除穿越代价被修改后的回调函数
    /// </summary>
    /// <param name="callback">Callback.</param>
    public void RemoveHeuristic(AStarCallback.HeuristicCallback callback)
    {
        _aStarCallback.OnHeuristic -= callback;
    }

    /// <summary>
    /// 刷新穿越代价
    /// </summary>
    private void RefreshPassCost()
    {
        foreach (var unit in _units)
        {
            if (unit.IsPassable) continue;
            if (!Walkable) return;
            Walkable = false;
            _aStarCallback.InvokeHeuristic(_aStarNodeParam);
            return;
        }
    }

    /// <summary>
    /// 单位的 IsPassable 属性被改变
    /// </summary>
    /// <returns><c>true</c> if this instance is passable change; otherwise, <c>false</c>.</returns>
    /*private void IsPassableChange()
	{
		this.RefreshPassCost();
	}*/
    /// <summary>
    /// 添加单位
    /// </summary>
    /// <returns><c>true</c>, if unit was added, <c>false</c> otherwise.</returns>
    /// <param name="unit">Unit.</param>
    public bool AddUnit(IAStarUnit unit)
    {
        if (!Walkable) return false;
        if (_units.IndexOf(unit) != -1) return false;
        //unit.AddIsPassableChange(this.IsPassableChange);
        _units.Add(unit);
        RefreshPassCost();
        return true;
    }

    /// <summary>
    /// 移除单位
    /// </summary>
    /// <returns><c>true</c>, if unit was removed, <c>false</c> otherwise.</returns>
    /// <param name="unit">Unit.</param>
    public bool RemoveUnit(IAStarUnit unit)
    {
        var index = _units.IndexOf(unit);
        if (index == -1) return false;
        //unit.RemoveIsPassableChange(this.IsPassableChange);
        _units.RemoveAt(index);
        RefreshPassCost();
        return true;
    }
}