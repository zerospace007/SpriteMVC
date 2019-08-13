#region

using System.Collections.Generic;

#endregion

/// <summary>
/// 最小二叉堆
/// </summary>
public class BinaryHeapUtils
{
    /// <summary>
    /// 节点对象池(缓存节点) 
    /// </summary>
    private readonly IList<BinaryHeapNode> _cacheNodes = new List<BinaryHeapNode>();

    /// <summary>
    /// 头节点
    /// </summary>
    public BinaryHeapNode HeadNode;

    /// <summary>
    /// 数组中正在使用的元素数目
    /// </summary>
    private int _nodeLength;

    /// <summary>
    /// 数组,用于保持树的平衡
    /// </summary>
    public IList<BinaryHeapNode> Nodes;

    // 小二叉堆
    public BinaryHeapUtils(int cacheSize)
    {
        Nodes = new List<BinaryHeapNode>(cacheSize);
        for (var index = 0; index < cacheSize; index ++)
        {
            Nodes.Add(null);
            _cacheNodes.Add(new BinaryHeapNode(null, null));
        }
    }

    /// <summary>
    /// 获得一个节点
    /// </summary>
    /// <returns>The Node.</returns>
    /// <param name="data">Data.</param>
    /// <param name="parentNode">Parent Node.</param>
    private BinaryHeapNode GetNode(AStarNode data, BinaryHeapNode parentNode)
    {
        BinaryHeapNode binaryHeapNode;

        if (_cacheNodes.Count > 0)
        {
            binaryHeapNode = _cacheNodes[_cacheNodes.Count - 1];

            binaryHeapNode.Data = data;
            binaryHeapNode.ParentNode = parentNode;

            _cacheNodes.RemoveAt(_cacheNodes.Count - 1);
        }
        else
        {
            binaryHeapNode = new BinaryHeapNode(data, parentNode);
        }
        return binaryHeapNode;
    }

    /// <summary>
    /// 存储节点
    /// </summary>
    /// <param name="node">Node.</param>
    private void CacheNode(BinaryHeapNode node)
    {
        node.ParentNode = node.LeftNode = node.RightNode = null;
        node.Data = null;

        _cacheNodes.Add(node);
    }

    /// <summary>
    /// 向下修正节点(向树叶方向修正节点)
    /// </summary>
    /// <returns>The to leaf.</returns>
    /// <param name="node">Node.</param>
    private BinaryHeapNode ModifyToLeaf(BinaryHeapNode node)
    {
        var currentNodeData = node.Data;
        var currentNodeValue = currentNodeData.G;

        while (true)
        {
            var leftNode = node.LeftNode;
            var rightNode = node.RightNode;

            if (rightNode != null && leftNode != null && rightNode.Data.F < leftNode.Data.F)
            {
                if (currentNodeValue > rightNode.Data.F)
                {
                    node.Data = rightNode.Data;
                    node.Data.BinaryHeapNode = node;
                    node = rightNode;
                }
                else
                {
                    break;
                }
            }
            else if (leftNode != null && leftNode.Data.F < currentNodeValue)
            {
                node.Data = leftNode.Data;
                node.Data.BinaryHeapNode = node;
                node = leftNode;
            }
            else
            {
                break;
            }
        }
        node.Data = currentNodeData;
        node.Data.BinaryHeapNode = node;

        return node;
    }

    /// <summary>
    /// 向上修正节点(向树根方向修正节点)
    /// </summary>
    /// <returns>The to root.</returns>
    /// <param name="node">Node.</param>
    private BinaryHeapNode ModifyToRoot(BinaryHeapNode node)
    {
        var currentNodeData = node.Data;
        var currentNodeValue = currentNodeData.F;

        var parentNode = node.ParentNode;
        while (parentNode != null)
        {
            if (currentNodeValue < parentNode.Data.F)
            {
                node.Data = parentNode.Data;
                node.Data.BinaryHeapNode = node;

                node = node.ParentNode;
                parentNode = node.ParentNode;
            }
            else
            {
                break;
            }
        }
        node.Data = currentNodeData;
        node.Data.BinaryHeapNode = node;

        return node;
    }

    /// <summary>
    /// 修正节点
    /// </summary>
    /// <returns>The Node.</returns>
    /// <param name="node">Node.</param>
    public BinaryHeapNode ModifyNode(BinaryHeapNode node)
    {
        if (node.ParentNode != null && node.ParentNode.Data.F > node.Data.F)
        {
            return ModifyToRoot(node);
        }
        return ModifyToLeaf(node);
    }

    /// <summary>
    /// 添加新节点
    /// </summary>
    /// <returns>The Node.</returns>
    /// <param name="data">Data.</param>
    public BinaryHeapNode InsertNode(AStarNode data)
    {
        if (HeadNode != null)
        {
            var parentNode = Nodes[_nodeLength >> 1];
            var node = GetNode(data, parentNode);
            node.Data.BinaryHeapNode = node;

            if (parentNode.LeftNode == null)
            {
                parentNode.LeftNode = node;
            }
            else
            {
                parentNode.RightNode = node;
            }
            Nodes[_nodeLength] = node;
            _nodeLength ++;
            return ModifyToRoot(node);
        }
        Nodes[1] = HeadNode = GetNode(data, null);
        Nodes.Add(HeadNode);
        HeadNode.Data.BinaryHeapNode = HeadNode;

        _nodeLength = 2;
        return HeadNode;
    }

    /// <summary>
    /// 取出最小值
    /// </summary>
    /// <returns>The Node.</returns>
    public AStarNode PopNode()
    {
        var minValue = HeadNode.Data;

        var lastNode = Nodes[--_nodeLength];

        if (lastNode != HeadNode)
        {
            var parentNode = lastNode.ParentNode;
            if (parentNode.LeftNode == lastNode)
            {
                parentNode.LeftNode = null;
            }
            else
            {
                parentNode.RightNode = null;
            }
            HeadNode.Data = lastNode.Data;
            HeadNode.Data.BinaryHeapNode = HeadNode;

            ModifyToLeaf(HeadNode);
        }
        else
        {
            HeadNode = null;
        }
        CacheNode(Nodes[_nodeLength]);
        Nodes[_nodeLength] = null;

        return minValue;
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        for (var index = 1; index < _nodeLength; index++)
        {
            CacheNode(Nodes[index]);
            Nodes[index] = null;
        }
        _nodeLength = 1;
        HeadNode = null;
    }
}