public static class PuzzleSolver
{
    private static ShapeType[] shapes = {
        ShapeType.Circle, ShapeType.Triangle, ShapeType.Square, ShapeType.Star
    };

    private static int solutionCount;

    public static bool HasUniqueSolution(ShapeType[,] board)
    {
        solutionCount = 0;
        ShapeType[,] copy = (ShapeType[,])board.Clone();
        Solve(copy, 0, 0);
        return solutionCount == 1;
    }

    private static bool Solve(ShapeType[,] board, int x, int y)
    {
        if (x == 9) { x = 0; y++; if (y == 9) { solutionCount++; return solutionCount > 1; } }
        if (board[x, y] != ShapeType.None) return Solve(board, x + 1, y);

        foreach (var shape in shapes)
        {
            if (IsValidMove(board, x, y, shape))
            {
                board[x, y] = shape;
                if (Solve(board, x + 1, y)) return true;
                board[x, y] = ShapeType.None;
            }
        }
        return false;
    }

    public static bool IsValidMove(ShapeType[,] board, int x, int y, ShapeType shape)
    {
        int count = 0;

        // 행 검사
        for (int i = 0; i < 9; i++)
            if (board[i, y] == shape) count++;
        if (count >= 2) return false;

        // 열 검사
        count = 0;
        for (int i = 0; i < 9; i++)
            if (board[x, i] == shape) count++;
        if (count >= 2) return false;

        // 블록 검사
        count = 0;
        int blockX = (x / 3) * 3;
        int blockY = (y / 3) * 3;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[blockX + i, blockY + j] == shape) count++;
        if (count >= 2) return false;

        return true;
    }
}