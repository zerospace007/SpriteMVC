public class AStarCallback
{
    // 
    public delegate void HeuristicCallback(AStarNode aStarNode);

    //
    public delegate void IsPassableChangeCallback();

    public event HeuristicCallback OnHeuristic;

    public event IsPassableChangeCallback OnIsPassableChange;

    public void InvokeHeuristic(AStarNode callAStarNode)
    {
        if (OnHeuristic != null)
        {
            OnHeuristic(callAStarNode);
        }
    }

    public void InvokeIsPassableChange()
    {
        if (OnIsPassableChange != null)
        {
            OnIsPassableChange();
        }
    }
}