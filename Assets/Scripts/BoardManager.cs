using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public GameObject cellPrefab;
    public Transform boardParent;
    public GridLayoutGroup gridLayout;
    
    [Header("Shape Sprites")]
    public Sprite triangleSprite;
    public Sprite circleSprite;
    public Sprite squareSprite;
    public Sprite pentagonSprite;
    public Sprite starSprite;

    private Cell[,] cells = new Cell[9, 9];
    private Cell selectedCell;
    private ShapeType selectedShape = ShapeType.None;
    
    private RuleChecker ruleChecker;
    private PuzzleGenerator puzzleGenerator;
    
    [Header("Memo Mode")]
    public bool isMemoMode = false; // 메모 모드 활성화 여부
    
    [Header("Hint System")]
    private ShapeType[,] solutionBoard; // 정답 보드 (힌트용)

    void Start()
    {
        ruleChecker = GetComponent<RuleChecker>();
        puzzleGenerator = GetComponent<PuzzleGenerator>();
        
        CreateBoard();
        GeneratePuzzle();
    }

    void CreateBoard()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GameObject cellObj = Instantiate(cellPrefab, boardParent);
                Cell cell = cellObj.GetComponent<Cell>();
                cell.Initialize(row, col, this);
                cells[row, col] = cell;
            }
        }
    }

    void GeneratePuzzle()
    {
        if (puzzleGenerator != null)
        {
            // GameManager 인스턴스 확인
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager가 없습니다!");
                return;
            }
            
            Debug.Log($"현재 모드: {GameManager.Instance.currentMode}, 난이도: {GameManager.Instance.currentDifficulty}, 스테이지: {GameManager.Instance.currentStage}");
            
            // 완전한 보드 생성 (정답)
            solutionBoard = puzzleGenerator.GenerateCompletePuzzle();
            
            // 난이도에 따라 일부 셀 제거
            ShapeType[,] puzzle = puzzleGenerator.CreatePuzzleFromSolution(
                solutionBoard,
                GameManager.Instance.currentDifficulty
            );
            
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    ShapeType shape = puzzle[row, col];
                    bool isInitial = (shape != ShapeType.None);
                    cells[row, col].SetShape(shape, isInitial);
                }
            }
        }
    }

    public void OnCellClicked(Cell cell)
    {
        // 이전 선택 해제
        if (selectedCell != null)
        {
            selectedCell.Highlight(false);
        }
        
        // 새로운 셀 선택
        selectedCell = cell;
        selectedCell.Highlight(true);
        
        Debug.Log($"셀 선택됨: ({cell.row}, {cell.col})");
    }

    public void SelectShape(int shapeIndex)
    {
        // 셀이 선택되어 있지 않으면 무시
        if (selectedCell == null)
        {
            Debug.LogWarning("먼저 퍼즐판의 셀을 클릭하세요!");
            return;
        }
        
        selectedShape = (ShapeType)shapeIndex;
        Debug.Log($"도형 선택: {selectedShape} (메모 모드: {isMemoMode})");
        
        // 메모 모드면 메모 추가/제거, 아니면 배치
        if (isMemoMode)
        {
            AddOrRemoveMemo();
        }
        else
        {
            PlaceShape();
        }
    }
    
    void AddOrRemoveMemo()
    {
        if (selectedCell == null || selectedShape == ShapeType.None) return;
        
        // 이미 메모에 있으면 제거, 없으면 추가
        if (selectedCell.memos.Contains(selectedShape))
        {
            selectedCell.RemoveMemo(selectedShape);
        }
        else
        {
            selectedCell.AddMemo(selectedShape);
        }
        
        selectedShape = ShapeType.None;
    }
    
    public void ToggleMemoMode()
    {
        isMemoMode = !isMemoMode;
        Debug.Log($"메모 모드: {(isMemoMode ? "ON" : "OFF")}");
    }
    
    public void UseHint()
    {
        if (selectedCell == null)
        {
            Debug.LogWarning("먼저 힌트를 받을 셀을 선택하세요!");
            return;
        }
        
        if (selectedCell.isInitial)
        {
            Debug.LogWarning("이미 채워진 셀입니다!");
            return;
        }
        
        if (solutionBoard != null)
        {
            ShapeType correctShape = solutionBoard[selectedCell.row, selectedCell.col];
            selectedCell.SetShape(correctShape, false);
            
            Debug.Log($"힌트: ({selectedCell.row}, {selectedCell.col})에 {correctShape} 배치");
            
            selectedCell.Highlight(false);
            selectedCell = null;
            
            // 클리어 체크
            CheckCompletion();
        }
    }
    
    void CheckCompletion()
    {
        if (ruleChecker != null && ruleChecker.IsComplete(cells))
        {
            Debug.Log("퍼즐 완성!");
            
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController != null)
            {
                gameController.OnPuzzleComplete();
            }
        }
    }

    void PlaceShape()
    {
        if (selectedCell == null || selectedShape == ShapeType.None) return;
        
        // 도형 배치
        selectedCell.SetShape(selectedShape, false);
        
        // 규칙 체크
        CheckCompletion();
        
        // 선택 해제
        selectedCell.Highlight(false);
        selectedCell = null;
        selectedShape = ShapeType.None;
    }

    public void ClearCell()
    {
        if (selectedCell != null && !selectedCell.isInitial)
        {
            selectedCell.SetShape(ShapeType.None, false);
            selectedCell.Highlight(false);
            selectedCell = null;
        }
    }

    public Sprite GetShapeSprite(ShapeType shape)
    {
        return shape switch
        {
            ShapeType.Triangle => triangleSprite,
            ShapeType.Circle => circleSprite,
            ShapeType.Square => squareSprite,
            ShapeType.Pentagon => pentagonSprite,
            ShapeType.Star => starSprite,
            _ => null
        };
    }

    public void ResetBoard()
    {
        GeneratePuzzle();
    }
}