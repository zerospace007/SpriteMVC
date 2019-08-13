public class AStarDiagonalHeuristic : IAStarHeuristic
{
    public int Heuristic(int x1, int y1, int x2, int y2)
    {
        var dx = x1 > x2 ? x1 - x2 : x2 - x1;
        var dy = y1 > y2 ? y1 - y2 : y2 - y1;

        return dx > dy
            ? AStarUtils.DiagCost * dy + AStarUtils.StraightCost * (dx - dy)
            : AStarUtils.DiagCost * dx + AStarUtils.StraightCost * (dy - dx);
    }
}