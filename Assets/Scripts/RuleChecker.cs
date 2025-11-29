using UnityEngine;
using System.Collections.Generic;

public class RuleChecker : MonoBehaviour
{
    // 행/열/블록에서 같은 도형이 3개 이상이면 규칙 위반
    private const int MAX_SAME_SHAPE = 2;

    public bool CheckRules(Cell[,] cells)
    {
        bool isValid = true;

        // 행 검사
        for (int row = 0; row < 9; row++)
        {
            if (!CheckRow(cells, row))
            {
                isValid = false;
            }
        }

        // 열 검사
        for (int col = 0; col < 9; col++)
        {
            if (!CheckColumn(cells, col))
            {
                isValid = false;
            }
        }

        // 3x3 블록 검사
        for (int block = 0; block < 9; block++)
        {
            if (!CheckBlock(cells, block))
            {
                isValid = false;
            }
        }

        return isValid;
    }

    bool CheckRow(Cell[,] cells, int row)
    {
        Dictionary<ShapeType, int> shapeCount = new Dictionary<ShapeType, int>();

        for (int col = 0; col < 9; col++)
        {
            ShapeType shape = cells[row, col].currentShape;
            if (shape == ShapeType.None) continue;

            if (!shapeCount.ContainsKey(shape))
                shapeCount[shape] = 0;
            
            shapeCount[shape]++;

            if (shapeCount[shape] > MAX_SAME_SHAPE)
            {
                Debug.LogWarning($"행 {row}에서 {shape} 도형이 {shapeCount[shape]}개 (규칙 위반)");
                return false;
            }
        }

        return true;
    }

    bool CheckColumn(Cell[,] cells, int col)
    {
        Dictionary<ShapeType, int> shapeCount = new Dictionary<ShapeType, int>();

        for (int row = 0; row < 9; row++)
        {
            ShapeType shape = cells[row, col].currentShape;
            if (shape == ShapeType.None) continue;

            if (!shapeCount.ContainsKey(shape))
                shapeCount[shape] = 0;
            
            shapeCount[shape]++;

            if (shapeCount[shape] > MAX_SAME_SHAPE)
            {
                Debug.LogWarning($"열 {col}에서 {shape} 도형이 {shapeCount[shape]}개 (규칙 위반)");
                return false;
            }
        }

        return true;
    }

    bool CheckBlock(Cell[,] cells, int blockIndex)
    {
        Dictionary<ShapeType, int> shapeCount = new Dictionary<ShapeType, int>();

        int startRow = (blockIndex / 3) * 3;
        int startCol = (blockIndex % 3) * 3;

        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                ShapeType shape = cells[startRow + r, startCol + c].currentShape;
                if (shape == ShapeType.None) continue;

                if (!shapeCount.ContainsKey(shape))
                    shapeCount[shape] = 0;
                
                shapeCount[shape]++;

                if (shapeCount[shape] > MAX_SAME_SHAPE)
                {
                    Debug.LogWarning($"블록 {blockIndex}에서 {shape} 도형이 {shapeCount[shape]}개 (규칙 위반)");
                    return false;
                }
            }
        }

        return true;
    }

    public bool IsComplete(Cell[,] cells)
    {
        // 1. 모든 칸이 채워졌는지 확인
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (cells[row, col].currentShape == ShapeType.None)
                {
                    return false;
                }
            }
        }

        // 2. 규칙을 만족하는지 확인
        return CheckRules(cells);
    }

    // 특정 셀에 도형을 놓을 수 있는지 미리 체크 (선택적 기능)
    public bool CanPlaceShape(Cell[,] cells, int row, int col, ShapeType shape)
    {
        // 임시로 배치
        ShapeType originalShape = cells[row, col].currentShape;
        cells[row, col].currentShape = shape;

        // 행, 열, 블록 체크
        bool canPlace = CheckRow(cells, row) && 
                        CheckColumn(cells, col) && 
                        CheckBlock(cells, cells[row, col].blockIndex);

        // 원래대로 복구
        cells[row, col].currentShape = originalShape;

        return canPlace;
    }
}