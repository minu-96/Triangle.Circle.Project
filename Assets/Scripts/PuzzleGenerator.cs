using System.Collections.Generic;
using UnityEngine;

public static class PuzzleGenerator
{
    private static ShapeType[] shapes = {
        ShapeType.Circle, ShapeType.Triangle, ShapeType.Square, ShapeType.Star
    };

    public static ShapeType[,] GeneratePuzzle()
    {
        ShapeType[,] fullBoard = new ShapeType[9, 9];
        GenerateFullBoard(fullBoard, 0, 0);

        ShapeType[,] puzzle = (ShapeType[,])fullBoard.Clone();
        List<(int x, int y)> positions = new();

        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 9; y++)
                positions.Add((x, y));

        positions.Shuffle();

        foreach (var (x, y) in positions)
        {
            ShapeType backup = puzzle[x, y];
            puzzle[x, y] = ShapeType.None;

            if (!PuzzleSolver.HasUniqueSolution(puzzle))
            {
                puzzle[x, y] = backup; // 복원
            }
        }

        return puzzle;
    }

    private static bool GenerateFullBoard(ShapeType[,] board, int x, int y)
    {
        if (x == 9) { x = 0; y++; if (y == 9) return true; }

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
