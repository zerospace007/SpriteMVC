public class AStarManhattanHeuristic : IAStarHeuristic
{
    public int Heuristic(int x1, int y1, int x2, int y2)
    {
        return (
            (x1 > x2 ? x1 - x2 : x2 - x1)
            +
            (y1 > y2 ? y1 - y2 : y2 - y1)
            ) * AStarUtils.StraightCost;
    }
}