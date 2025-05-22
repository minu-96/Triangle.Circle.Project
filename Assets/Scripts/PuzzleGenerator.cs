using System.Collections.Generic;
using UnityEngine;

public static class PuzzleGenerator
{
    private static ShapeType[] shapes = {
        ShapeType.Circle, ShapeType.Triangle, ShapeType.Square, ShapeType.Star
    };

    public static void GenerateFullAnswer(ShapeType[,] board)
    {
        bool success = GenerateFullBoard(board, 0, 0);
        Debug.Log($"정답 퍼즐 생성 결과: {success}");

        // 디버깅: 생성된 정답 출력
        for (int y = 0; y < 9; y++)
        {
            string row = "";
            for (int x = 0; x < 9; x++)
            {
                row += board[x, y] + " ";
            }
            Debug.Log(row);
        }
    }

    public static ShapeType[,] MakePuzzleFromAnswer(ShapeType[,] answer, int revealCount)
    {
        ShapeType[,] puzzle = (ShapeType[,])answer.Clone();
        List<(int x, int y)> cells = new();

        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 9; y++)
                cells.Add((x, y));

        cells.Shuffle();

        int removeCount = 81 - revealCount;

        foreach (var (x, y) in cells)
        {
            ShapeType backup = puzzle[x, y];
            puzzle[x, y] = ShapeType.None;

            if (!PuzzleSolver.HasUniqueSolution(puzzle))
            {
                puzzle[x, y] = backup;  // 되돌림
            }
            else
            {
                removeCount--;
                if (removeCount <= 0) break;
            }
        }

        return puzzle;
    }

    private static bool GenerateFullBoard(ShapeType[,] board, int x, int y)
    {
        if (x == 9) { x = 0; y++; if (y == 9) return true; }

        List<ShapeType> shapes = new() {
        ShapeType.Circle, ShapeType.Triangle, ShapeType.Square,
        ShapeType.Hexagon, ShapeType.Star
    };
        shapes.Shuffle();

        foreach (var shape in shapes)
        {
            if (PuzzleSolver.IsValidMove(board, x, y, shape))
            {
                board[x, y] = shape;
                if (GenerateFullBoard(board, x + 1, y)) return true;
                board[x, y] = ShapeType.None;
            }
        }
        return false;
    }
}