using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("퍼즐 보드")]
    public GameObject cellPrefab;
    public GameObject cellPrefabs;
    public Transform boardParent;
    public GameObject boardPanel;
    public Sprite[] shapeSprites;
    public GameObject shapePanel;

    [Header("정답 패널")]
    public GameObject answerPanel;
    public Transform answerPanelParent; // AnswerPanel 안에 셀을 넣을 부모 Transform

    private Cell[,] cells = new Cell[9, 9];
    private Cell[,] answerCells = new Cell[9, 9]; // 동적으로 생성되는 정답 셀

    private ShapeType[,] puzzleData;
    private ShapeType[,] answerData;
    private Cell selectedCell;

    public int Fill_Puzzle = 40;
    private int esc = 0;
    public int remaing_esc = 4;

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

        puzzleData = PuzzleGenerator.MakePuzzleFromAnswer(answerData, Fill_Puzzle);
        ApplyPuzzleToBoard();

        CreateAnswerPanel(); // 정답 셀 생성
        answerPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && esc < remaing_esc)
        {
            answerPanel.SetActive(!answerPanel.activeSelf);
            boardPanel.SetActive(!boardPanel.activeSelf);
            esc++;
        }

        CheckAnswer();
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

    void CreateAnswerPanel()
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                GameObject ob = Instantiate(cellPrefabs, answerPanelParent.transform);
                Cell answercell = ob.GetComponent<Cell>();
                answercell.shapeSprites = shapeSprites;
                answercell.Init(x, y, this);
                answercell.SetShape(answerData[x, y]);
                answercell.Lock(); // 클릭 불가능하게
                answerCells[x, y] = answercell;
            }
        }
    }

    public void OnCellClicked(Cell cell)
    {
        Debug.Log("셀 클릭됨");
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
                    return;
                }
            }
        }

        Debug.Log("정답입니다!");
        Application.Quit();
        // UnityEditor.EditorApplication.isPlaying = false;
    }
}
