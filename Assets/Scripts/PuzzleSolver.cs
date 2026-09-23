using System;
using System.Diagnostics;

/// <summary>Shared rule engine. A solution is any full board with at most two of each shape per unit.</summary>
public static class PuzzleSolver
{
    public static bool CanPlace(ShapeType[,] board, int row, int col, ShapeType shape)
    {
        if ((int)shape < 1 || (int)shape > 5) return false;
        int rows = 0, columns = 0, blocks = 0;
        for (int i = 0; i < 9; i++)
        {
            if (i != col && board[row, i] == shape) rows++;
            if (i != row && board[i, col] == shape) columns++;
            int r = row / 3 * 3 + i / 3, c = col / 3 * 3 + i % 3;
            if ((r != row || c != col) && board[r, c] == shape) blocks++;
        }
        return rows < 2 && columns < 2 && blocks < 2;
    }

    public static bool IsValid(ShapeType[,] board, bool requireComplete = false)
    {
        if (board == null || board.GetLength(0) != 9 || board.GetLength(1) != 9) return false;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board[r, c] == ShapeType.None ? requireComplete : !CanPlace(board, r, c, board[r, c])) return false;
        return true;
    }

    // Bounded MRV search: a difficult hint must never lock up the game indefinitely.
    public static bool TrySolve(ShapeType[,] source, out ShapeType[,] solution, out bool timedOut, int milliseconds = 200)
    {
        solution = null;
        timedOut = false;
        if (!IsValid(source)) return false;
        var board = (ShapeType[,])source.Clone();
        var counts = new int[27, 6];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board[r, c] != ShapeType.None) Add(counts, r, c, (int)board[r, c], 1);
        var watch = Stopwatch.StartNew();
        bool timeout = false;
        bool Search()
        {
            if (watch.ElapsedMilliseconds >= milliseconds) { timeout = true; return false; }
            int row = -1, col = -1, mask = 0, minimum = 6;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] != ShapeType.None) continue;
                    int choices = 0, length = 0;
                    for (int v = 1; v <= 5; v++)
                        if (counts[r, v] < 2 && counts[9 + c, v] < 2 && counts[18 + r / 3 * 3 + c / 3, v] < 2)
                        { choices |= 1 << v; length++; }
                    if (length == 0) return false;
                    if (length < minimum) { row = r; col = c; mask = choices; minimum = length; }
                }
            }
            if (row < 0) return true;
            for (int v = 1; v <= 5; v++)
            {
                if ((mask & (1 << v)) == 0) continue;
                board[row, col] = (ShapeType)v;
                Add(counts, row, col, v, 1);
                if (Search()) return true;
                Add(counts, row, col, v, -1);
                board[row, col] = ShapeType.None;
                if (timeout) return false;
            }
            return false;
        }
        bool solved = Search();
        timedOut = timeout;
        if (solved) solution = board;
        return solved;
    }

    static void Add(int[,] counts, int r, int c, int v, int amount)
    {
        counts[r, v] += amount;
        counts[9 + c, v] += amount;
        counts[18 + r / 3 * 3 + c / 3, v] += amount;
    }
}
