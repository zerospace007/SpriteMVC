/// <summary>
/// 邻节点
/// </summary>
public class AStarLinkNode
{
    /// <summary>
    /// 花费代价
    /// </summary>
    public int Cost;

    /// <summary>
    /// 节点
    /// </summary>
    public AStarNode Node;

    public AStarLinkNode(AStarNode node, int cost)
    {
        Node = node;
        Cost = cost;
    }
}