using UnityEngine;
using System.Collections.Generic;

public class PuzzleGenerator : MonoBehaviour
{
    private RuleChecker ruleChecker;
    public DifficultySettings settings; // Inspector에서 연결

    void Awake()
    {
        ruleChecker = GetComponent<RuleChecker>();
    }

    public ShapeType[,] GeneratePuzzle(GameDifficulty difficulty)
    {
        ShapeType[,] board = new ShapeType[9, 9];
        
        // 1. 완전한 보드 생성
        FillBoard(board);
        
        // 2. 난이도에 따라 셀 제거
        int emptyCells = settings != null 
            ? settings.GetEmptyCellCount(difficulty)
            : GetEmptyCellCount(difficulty);
        
        // 디버그 로그로 확인
        Debug.Log($"[PuzzleGenerator] 난이도: {difficulty}, 빈칸 개수: {emptyCells}");
        
        RemoveCells(board, emptyCells);
        
        return board;
    }

    void FillBoard(ShapeType[,] board)
    {
        // 백트래킹을 사용한 보드 채우기
        FillBoardRecursive(board, 0, 0);
    }

    bool FillBoardRecursive(ShapeType[,] board, int row, int col)
    {
        // 모든 칸을 채웠으면 완료
        if (row == 9) return true;

        // 다음 칸 계산
        int nextRow = (col == 8) ? row + 1 : row;
        int nextCol = (col == 8) ? 0 : col + 1;

        // 5가지 도형을 랜덤 순서로 시도
        List<ShapeType> shapes = new List<ShapeType> 
        { 
            ShapeType.Triangle, 
            ShapeType.Circle, 
            ShapeType.Square, 
            ShapeType.Pentagon, 
            ShapeType.Star 
        };
        
        Shuffle(shapes);

        foreach (ShapeType shape in shapes)
        {
            if (CanPlaceShape(board, row, col, shape))
            {
                board[row, col] = shape;

                if (FillBoardRecursive(board, nextRow, nextCol))
                {
                    return true;
                }

                board[row, col] = ShapeType.None;
            }
        }

        return false;
    }

    bool CanPlaceShape(ShapeType[,] board, int row, int col, ShapeType shape)
    {
        // 행 체크
        Dictionary<ShapeType, int> rowCount = new Dictionary<ShapeType, int>();
        for (int c = 0; c < 9; c++)
        {
            if (board[row, c] != ShapeType.None)
            {
                if (!rowCount.ContainsKey(board[row, c]))
                    rowCount[board[row, c]] = 0;
                rowCount[board[row, c]]++;
            }
        }
        if (rowCount.ContainsKey(shape) && rowCount[shape] >= 2) return false;

        // 열 체크
        Dictionary<ShapeType, int> colCount = new Dictionary<ShapeType, int>();
        for (int r = 0; r < 9; r++)
        {
            if (board[r, col] != ShapeType.None)
            {
                if (!colCount.ContainsKey(board[r, col]))
                    colCount[board[r, col]] = 0;
                colCount[board[r, col]]++;
            }
        }
        if (colCount.ContainsKey(shape) && colCount[shape] >= 2) return false;

        // 블록 체크
        int blockStartRow = (row / 3) * 3;
        int blockStartCol = (col / 3) * 3;
        Dictionary<ShapeType, int> blockCount = new Dictionary<ShapeType, int>();
        
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                ShapeType blockShape = board[blockStartRow + r, blockStartCol + c];
                if (blockShape != ShapeType.None)
                {
                    if (!blockCount.ContainsKey(blockShape))
                        blockCount[blockShape] = 0;
                    blockCount[blockShape]++;
                }
            }
        }
        if (blockCount.ContainsKey(shape) && blockCount[shape] >= 2) return false;

        return true;
    }

    void RemoveCells(ShapeType[,] board, int count)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                positions.Add(new Vector2Int(row, col));
            }
        }
        
        // 난이도에 따라 제거 전략 변경
        GameDifficulty difficulty = GameManager.Instance.currentDifficulty;
        
        if (difficulty == GameDifficulty.Easy)
        {
            // 쉬움: 완전 랜덤
            Shuffle(positions);
        }
        else if (difficulty == GameDifficulty.Normal)
        {
            Shuffle(positions);
        }
        else // Hard
        {
            // 어려움: 대칭적이고 전략적으로 비움
            positions = GetStrategicPositions(positions);
        }

        for (int i = 0; i < count && i < positions.Count; i++)
        {
            Vector2Int pos = positions[i];
            board[pos.x, pos.y] = ShapeType.None;
        }
    }


    // 전략적 위치 반환 (어려움 모드)
    List<Vector2Int> GetStrategicPositions(List<Vector2Int> positions)
    {
        List<Vector2Int> strategic = new List<Vector2Int>();
        List<Vector2Int> remaining = new List<Vector2Int>(positions);
        
        // 1. 각 블록에서 균등하게 제거
        for (int block = 0; block < 9; block++)
        {
            int startRow = (block / 3) * 3;
            int startCol = (block % 3) * 3;
            
            List<Vector2Int> blockCells = new List<Vector2Int>();
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    blockCells.Add(new Vector2Int(startRow + r, startCol + c));
                }
            }
            
            Shuffle(blockCells);
            // 각 블록에서 2-3개씩 선택
            int takeCount = Random.Range(2, 4);
            for (int i = 0; i < takeCount && i < blockCells.Count; i++)
            {
                strategic.Add(blockCells[i]);
                remaining.Remove(blockCells[i]);
            }
        }
        
        // 2. 나머지는 랜덤
        Shuffle(remaining);
        strategic.AddRange(remaining);
        
        return strategic;
    }

    int GetEmptyCellCount(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => 21,
            GameDifficulty.Normal => 41,
            GameDifficulty.Hard => 61,
            _ => 41
        };
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}