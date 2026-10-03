using UnityX.HexGrid;

public static class FogTestPatterns {
    // A revealed area that encloses an unrevealed pocket. Tracing only the outer outline of each revealed island
    // used to fill the pocket in; it should stay fogged.
    public static readonly HexCoord[] revealedAreaWithHole = {
        new HexCoord(0, 0), new HexCoord(1, 0), new HexCoord(2, 0), new HexCoord(2, 1), new HexCoord(3, 1),
        new HexCoord(3, 2), new HexCoord(4, 2), new HexCoord(5, 2), new HexCoord(5, 3), new HexCoord(4, 4),
        new HexCoord(4, 3), new HexCoord(3, 3), new HexCoord(2, 3), new HexCoord(1, 4), new HexCoord(0, 4),
        new HexCoord(0, 3), new HexCoord(1, 3), new HexCoord(1, 2), new HexCoord(1, 1), new HexCoord(0, 1),
        new HexCoord(-1, 2), new HexCoord(-1, 3), new HexCoord(-2, 4), new HexCoord(-3, 5), new HexCoord(-2, 3),
        new HexCoord(-2, 2), new HexCoord(-3, 2), new HexCoord(-1, 1), new HexCoord(5, 1), new HexCoord(6, 1),
        new HexCoord(5, 0), new HexCoord(5, -1), new HexCoord(6, -1), new HexCoord(6, -2), new HexCoord(5, -2),
        new HexCoord(4, -1), new HexCoord(3, -1), new HexCoord(2, -1), new HexCoord(1, -1), new HexCoord(0, -1),
        new HexCoord(-1, -1), new HexCoord(-2, 0), new HexCoord(-3, 0), new HexCoord(-2, -1), new HexCoord(-2, -2),
        new HexCoord(-1, -2), new HexCoord(0, -2), new HexCoord(0, -3), new HexCoord(1, -3), new HexCoord(2, -3),
        new HexCoord(3, -3), new HexCoord(4, -3), new HexCoord(5, -3), new HexCoord(6, -3), new HexCoord(6, -4),
        new HexCoord(5, -4), new HexCoord(4, -4), new HexCoord(3, -4), new HexCoord(2, -4), new HexCoord(1, -4),
        new HexCoord(2, -5), new HexCoord(4, -2), new HexCoord(3, -2),
    };
}
