/// <summary>
/// 二叉堆节点
/// </summary>
public class BinaryHeapNode
{
    /// <summary>
    /// 节点数据
    /// </summary>
    public AStarNode Data;

    /// <summary>
    /// 左子节点
    /// </summary>
    public BinaryHeapNode LeftNode;

    /// <summary>
    /// 父节点
    /// </summary>
    public BinaryHeapNode ParentNode;

    /// <summary>
    /// 右子节点
    /// </summary>
    public BinaryHeapNode RightNode;

    public BinaryHeapNode(AStarNode data, BinaryHeapNode parentNode)
    {
        Data = data;
        ParentNode = parentNode;
    }
}