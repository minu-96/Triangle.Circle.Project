using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    public GameObject cellPrefab;
    public Transform boardParent;
    public Sprite[] shapeSprites;
    public GameObject shapePanel;

    private Cell[,] cells = new Cell[9, 9];
    private ShapeType[,] puzzleData;
    private ShapeType[,] answerData;
    private Cell selectedCell;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CreateBoard();

        answerData = new ShapeType[9, 9];
        PuzzleGenerator.GenerateFullAnswer(answerData);
        Debug.Log("정답 생성 완료");

        puzzleData = PuzzleGenerator.MakePuzzleFromAnswer(answerData, 40);
        Debug.Log("퍼즐 생성 완료");

        int filled = 0;
        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 9; y++)
                if (puzzleData[x, y] != ShapeType.None)
                    filled++;

        Debug.Log($"퍼즐 채워진 칸 수: {filled}");
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
                var cell = cells[x, y];

                if (puzzleData[x, y] != ShapeType.None)
                {
                    cell.SetShape(puzzleData[x, y]);
                    cell.Lock();
                }
                else
                {
                    cell.Clear();
                    cell.Unlock();
                }
            }
        }
    }

    public void OnCellClicked(Cell cell)
    {
        selectedCell = cell;
        shapePanel.SetActive(true);
    }

    public void OnShapeSelected(int shapeIndex)
    {
        if (selectedCell == null) return;

        ShapeType type = (ShapeType)shapeIndex;
        selectedCell.SetShape(type);
        shapePanel.SetActive(false);
        selectedCell = null;
    }

    public void CheckAnswer()
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (cells[x, y].GetShape() != answerData[x, y])
                {
                    Debug.Log("틀렸습니다!");
                    return;
                }
            }
        }
        Debug.Log("정답입니다!");
    }
}