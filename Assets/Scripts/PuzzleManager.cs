using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public Transform boardParent;
    public Sprite[] shapeSprites;

    private Cell[,] cells = new Cell[9, 9];
    private ShapeType[,] puzzleData;

    

    void Start()
    {
        CreateBoard();
        puzzleData = PuzzleGenerator.GeneratePuzzle();
        ApplyPuzzleToBoard();
    }

    void CreateBoard()
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                GameObject obj = Instantiate(cellPrefab, boardParent);
                Cell cell = obj.GetComponent<Cell>();
                cell.shapeSprites = shapeSprites;
                cell.Init(x, y, this);
                cells[x, y] = cell;
            }
        }
    }

    void ApplyPuzzleToBoard()
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                cells[x, y].SetShape(puzzleData[x, y]);
            }
        }
    }

    public ShapeType selectedShape = ShapeType.None;

    public void SelectShape(int shapeIndex)
    {
        selectedShape = (ShapeType)shapeIndex;
    }

    public void OnCellClicked(Cell cell)
    {
        if (selectedShape == ShapeType.None) return;

        cell.SetShape(selectedShape);
        // 여기에 규칙 검사나 정답 확인도 추가할 수 있음
    }

}

