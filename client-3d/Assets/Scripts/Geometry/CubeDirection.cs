public enum CubeDirection
{
    BottomRight,
    Right,
    TopRight,
    TopLeft,
    Left,
    BottomLeft
}

public static class CubeDirectionExtensions
{
    private static CubeCoord BottomRight = new CubeCoord(0, -1, 1);
    private static CubeCoord Right = new CubeCoord(1, -1, 0);
    private static CubeCoord TopRight = new CubeCoord(1, 0, -1);
    private static CubeCoord TopLeft = new CubeCoord(0, 1, -1);
    private static CubeCoord Left = new CubeCoord(-1, 1, 0);
    private static CubeCoord BottomLeft = new CubeCoord(-1, 0, 1);

    public static CubeCoord[] directionToCoordMap = new CubeCoord[] {
        BottomRight, Right, TopRight,
        TopLeft, Left, BottomLeft
    };

    public static CubeCoord ToCoord(this CubeDirection direction)
    {
        return directionToCoordMap[(int)direction];
    }
}
