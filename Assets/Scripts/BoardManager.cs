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
            ShapeType[,] puzzle = puzzleGenerator.GeneratePuzzle(
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
        Debug.Log($"도형 선택: {selectedShape}");
        
        // 즉시 배치
        PlaceShape();
    }

    void PlaceShape()
    {
        if (selectedCell == null || selectedShape == ShapeType.None) return;
        
        // 도형 배치
        selectedCell.SetShape(selectedShape, false);
        
        // 규칙 체크
        if (ruleChecker != null)
        {
            ruleChecker.CheckRules(cells);
            
            if (ruleChecker.IsComplete(cells))
            {
                Debug.Log("퍼즐 완성!");
                
                // GameController에 클리어 알림
                GameController gameController = FindObjectOfType<GameController>();
                if (gameController != null)
                {
                    gameController.OnPuzzleComplete();
                }
            }
        }
        
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