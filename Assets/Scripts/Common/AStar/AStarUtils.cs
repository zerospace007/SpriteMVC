#region

using System.Collections.Generic;
using UnityEngine;

#endregion

/// <summary>
/// A 星算法，公式：f = g + h;
/// </summary>
public class AStarUtils
{
    /// <summary>
    /// 直角移动的 g 值
    /// </summary>
    public const int StraightCost = 10;

    /// <summary>
    /// 对角移动的 g 值
    /// </summary>
    public const int DiagCost = 14;

    /// <summary>
    /// 存放 "OpenList" 的最小二叉堆
    /// </summary>
    private readonly BinaryHeapUtils _binaryHeapUtils;

    /// <summary>
    /// 当前节点到结束节点的估价函数
    /// </summary>
    private readonly IAStarHeuristic _iAStarHeuristic;

    /// <summary>
    /// 是否是四向寻路，默认为八向寻路
    /// </summary>
    private readonly bool _isFourWay;

    /// <summary>
    /// 地图节点
    /// </summary>
    private readonly Dictionary<string, AStarNode> _nodes;

    /// <summary>
    /// 地图的宽度(列数)
    /// </summary>
    private readonly int _numCols;

    /// <summary>
    /// 地图的高度(行数)
    /// </summary>
    private readonly int _numRows;

    /// <summary>
    /// 当前的寻路编号 
    /// </summary>
    private int _searchPathCheckNum;

    /// <summary>
    /// 当前查找可移动范围的编号
    /// </summary>
    private int _walkableRangeCheckNum;

    public AStarUtils(int numCols, int numRows, bool isFourWay = false)
    {
        _numCols = numCols;
        _numRows = numRows;
        _isFourWay = isFourWay;
        _iAStarHeuristic = new AStarManhattanHeuristic();
        //_iAStarHeuristic = new AStarDiagonalHeuristic ();

        _nodes = new Dictionary<string, AStarNode>();
        for (var i = 0; i < _numCols; i++)
        {
            for (var j = 0; j < _numRows; j++)
            {
                var node = new AStarNode(i, j);
                node.AddHeuristic(RefreshLinksOfAdjacentNodes, node);
                _nodes.Add(GetNodeKey(i, j), node);
            }
        }
        RefreshLinksOfAllNodes();
        _binaryHeapUtils = new BinaryHeapUtils(numCols*numRows/2);
    }

    /// <summary>
    /// 获取节点
    /// </summary>
    /// <returns>The Node.</returns>
    /// <param name="nodeX">Node x.</param>
    /// <param name="nodeY">Node y.</param>
    public AStarNode GetNode(int nodeX, int nodeY)
    {
        var nodeKey = GetNodeKey(nodeX, nodeY);
        return _nodes.ContainsKey(nodeKey) ? _nodes[nodeKey] : null;
    }

    /// <summary>
    /// 组装 Star Key
    /// </summary>
    /// <returns>The Node key.</returns>
    /// <param name="nodeX">Node x.</param>
    /// <param name="nodeY">Node y.</param>
    private string GetNodeKey(int nodeX, int nodeY)
    {
        return nodeX + ":" + nodeY;
    }

    /// <summary>
    /// 获取节点的相邻节点
    /// </summary>
    /// <returns>The adjacent _nodes.</returns>
    /// <param name="node">Node.</param>
    private IList<AStarNode> GetAdjacentNodes(AStarNode node)
    {
        IList<AStarNode> adjacentNodes = new List<AStarNode>();

        var startX = Mathf.Max(0, node.NodeX - 1);
        var endX = Mathf.Min(_numCols - 1, node.NodeX + 1);

        var startY = Mathf.Max(0, node.NodeY - 1);
        var endY = Mathf.Min(_numRows - 1, node.NodeY + 1);

        for (var i = startX; i <= endX; i++)
        {
            for (var j = startY; j <= endY; j++)
            {
                var varNode = _nodes[GetNodeKey(i, j)];
                if (varNode == node) continue;
                if (_isFourWay)
                {
                    if (!(i == node.NodeX || j == node.NodeY))
                    {
                        continue;
                    }
                }
                adjacentNodes.Add(varNode);
            }
        }
        return adjacentNodes;
    }

    /// <summary>
    /// 刷新节点的 Links 属性
    /// </summary>
    /// <param name="node">Node.</param>
    private void RefreshNodeLinks(AStarNode node)
    {
        var adjacentNodes = GetAdjacentNodes(node);

        var links = new List<AStarLinkNode>();
        foreach (var nodeItem in adjacentNodes)
        {
            if (!nodeItem.Walkable) continue;
            int cost;
            if (node.NodeX != nodeItem.NodeX && node.NodeY != nodeItem.NodeY)
            {
                if (!_nodes[GetNodeKey(node.NodeX, nodeItem.NodeY)].Walkable ||
                    !_nodes[GetNodeKey(nodeItem.NodeX, node.NodeY)].Walkable)
                {
                    continue;
                }
                cost = DiagCost;
            }
            else
            {
                cost = StraightCost;
            }
            links.Add(new AStarLinkNode(nodeItem, cost));
        }

        node.Links = links;
    }

    /// <summary>
    /// 刷新节点的相邻节点的 Links 属性
    /// </summary>
    /// <param name="node">Node.</param>
    private void RefreshLinksOfAdjacentNodes(AStarNode node)
    {
        var adjacentNodes = GetAdjacentNodes(node);
        foreach (var adjacentNode in adjacentNodes)
        {
            RefreshNodeLinks(adjacentNode);
        }
    }

    /// <summary>
    /// 刷新所有节点的 Links 属性
    /// </summary>
    private void RefreshLinksOfAllNodes()
    {
        for (var i = 0; i < _numCols; i++)
        {
            for (var j = 0; j < _numRows; j++)
            {
                RefreshNodeLinks(_nodes[GetNodeKey(i, j)]);
            }
        }
    }

    /// <summary>
    /// 搜索路径
    /// </summary>
    /// <returns><c>true</c>, if base binary heap was searched, <c>false</c> otherwise.</returns>
    /// <param name="startNode">Start Node.</param>
    /// <param name="endNode">End Node.</param>
    /// <param name="nowCheckNum">Now check number.</param>
    private bool SearchBaseBinaryHeap(AStarNode startNode, AStarNode endNode, int nowCheckNum)
    {
        var statusClosed = nowCheckNum + 1;

        _binaryHeapUtils.Reset();

        startNode.G = 0;
        startNode.F = startNode.G +
                      _iAStarHeuristic.Heuristic(startNode.NodeX, startNode.NodeY, endNode.NodeX, endNode.NodeY);
        startNode.SearchPathCheckNum = statusClosed;

        var node = startNode;

        while (node != endNode)
        {
            var links = node.Links;
            foreach (var link in links)
            {
                var nodeItem = link.Node;
                var g = node.G + link.Cost;

                // 如果已被检查过
                if (nodeItem.SearchPathCheckNum >= nowCheckNum)
                {
                    if (nodeItem.G <= g) continue;
                    nodeItem.F = g +
                                 _iAStarHeuristic.Heuristic(nodeItem.NodeX, nodeItem.NodeY, endNode.NodeX,
                                     endNode.NodeY);
                    nodeItem.G = g;
                    nodeItem.ParentNode = node;
                    if (nodeItem.SearchPathCheckNum == nowCheckNum)
                    {
                        _binaryHeapUtils.ModifyNode(nodeItem.BinaryHeapNode);
                    }
                }
                else
                {
                    nodeItem.F = g +
                                 _iAStarHeuristic.Heuristic(nodeItem.NodeX, nodeItem.NodeY, endNode.NodeX, endNode.NodeY);
                    nodeItem.G = g;
                    nodeItem.ParentNode = node;

                    nodeItem.BinaryHeapNode = _binaryHeapUtils.InsertNode(nodeItem);
                    nodeItem.SearchPathCheckNum = nowCheckNum;
                }
            }
            if (_binaryHeapUtils.HeadNode != null)
            {
                node = _binaryHeapUtils.PopNode();

                node.SearchPathCheckNum = statusClosed;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 寻路
    /// </summary>
    /// <returns>The path.</returns>
    /// <param name="startNode">Start Node.</param>
    /// <param name="endNode">End Node.</param>
    public IList<AStarNode> FindPath(AStarNode startNode, AStarNode endNode)
    {
        _searchPathCheckNum += 2;
        if (!SearchBaseBinaryHeap(startNode, endNode, _searchPathCheckNum)) return null;
        var currentNode = endNode;
        IList<AStarNode> pathList = new List<AStarNode>
        {
            startNode
        };
        while (currentNode != startNode)
        {
            currentNode = currentNode.ParentNode;
            pathList.Add(currentNode);
        }

        return pathList;
    }

    /// <summary>
    /// 返回节点在指定的代价内可移动的范围
    /// </summary>
    /// <returns>The range.</returns>
    /// <param name="startNode">Start Node.</param>
    /// <param name="costLimit">Cost limit.</param>
    public IList<AStarNode> WalkableRange(AStarNode startNode, int costLimit)
    {
        _walkableRangeCheckNum ++;

        var maxStep = costLimit/StraightCost;

        var startX = Mathf.Max(startNode.NodeX - maxStep, 0);
        var endX = Mathf.Min(startNode.NodeX + maxStep, _numCols - 1);
        var startY = Mathf.Max(startNode.NodeY - maxStep, 0);
        var endY = Mathf.Min(startNode.NodeY + maxStep, _numRows - 1);

        IList<AStarNode> rangeList = new List<AStarNode>();
        for (var i = startX; i <= endX; i++)
        {
            for (var j = startY; j <= endY; j++)
            {
                var nodeItem = _nodes[GetNodeKey(i, j)];
                if (!nodeItem.Walkable || nodeItem.WalkableRangeCheckNum == _walkableRangeCheckNum) continue;
                var pathList = FindPath(startNode, nodeItem);
                if (pathList == null || pathList[pathList.Count - 1].F > costLimit) continue;
                foreach (var node in pathList)
                {
                    if (node.WalkableRangeCheckNum == _walkableRangeCheckNum) continue;
                    node.WalkableRangeCheckNum = _walkableRangeCheckNum;
                    rangeList.Add(node);
                }
            }
        }
        return rangeList;
    }
}