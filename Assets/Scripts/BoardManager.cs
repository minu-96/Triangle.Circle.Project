using System;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public GameObject cellPrefab;
    public Transform boardParent;
    [Tooltip("3×3 블록 9개(좌상단부터 행 우선). 지정하면 칸을 블록별로 나눠 담아 블록이 간격으로 구분된다.")]
    public Transform[] blockParents;
    public GridLayoutGroup gridLayout;
    [Header("Shape Sprites")]
    public Sprite triangleSprite, circleSprite, squareSprite, pentagonSprite, starSprite;
    [Header("Memo Mode")]
    public bool isMemoMode;
    [Tooltip("HTML 방식: 도형을 먼저 고르고 칸을 클릭해 배치")]
    public bool placeOnCellClick;
    public bool InputEnabled { get; set; } = true;
    public event Action Changed;
    public event Action Completed;
    public event Action<string> Feedback;
    public ShapeType SelectedShape => selectedShape;
    public Cell SelectedCell => selectedCell;
    public bool IsReady => ready;

    readonly Cell[,] cells = new Cell[9, 9];
    Cell selectedCell;
    ShapeType selectedShape = ShapeType.None;
    ShapeType[,] solutionBoard, initialBoard;
    PuzzleGenerator puzzleGenerator;
    bool ready, complete;

    void Start() { if (!ready) InitializeBoard(); }

    public void InitializeBoard()
    {
        if (ready) return;
        puzzleGenerator = GetComponent<PuzzleGenerator>();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                // 블록 컨테이너가 있으면 해당 블록에 담는다. 전역 행 우선 순회이므로
                // 블록 안에서도 자연히 행 우선 순서가 된다.
                var parent = boardParent;
                if (blockParents != null && blockParents.Length == 9)
                    parent = blockParents[(r / 3) * 3 + (c / 3)];
                var obj = Instantiate(cellPrefab, parent);
                obj.name = $"Cell_{r + 1}_{c + 1}";
                obj.SetActive(true);
                var cell = obj.GetComponent<Cell>();
                cell.Initialize(r, c, this);
                cells[r, c] = cell;
            }
        ready = true;
        ResetBoard();
    }

    public ShapeType[,] GetBoard()
    {
        var board = new ShapeType[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++) board[r, c] = cells[r, c].currentShape;
        return board;
    }
    public ShapeType[,] GetInitialBoard() => (ShapeType[,])initialBoard.Clone();
    public ShapeType[,] GetSolutionBoard() => (ShapeType[,])solutionBoard.Clone();

    public void Restore(ShapeType[,] initial, ShapeType[,] board, ShapeType[,] solution)
    {
        initialBoard = (ShapeType[,])initial.Clone();
        solutionBoard = (ShapeType[,])solution.Clone();
        ApplyBoard(board);
    }

    void ApplyBoard(ShapeType[,] board)
    {
        selectedCell = null;
        selectedShape = ShapeType.None;
        complete = false;
        isMemoMode = false;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                cells[r, c].ClearMemos();
                cells[r, c].SetShape(board[r, c], initialBoard[r, c] != ShapeType.None);
            }
        RefreshSelection();
        Changed?.Invoke();
    }

    public void ResetBoard()
    {
        if (!ready || puzzleGenerator == null || GameManager.Instance == null) return;
        solutionBoard = puzzleGenerator.GenerateCompletePuzzle();
        initialBoard = puzzleGenerator.CreatePuzzleFromSolution(solutionBoard, GameManager.Instance.currentDifficulty);
        ApplyBoard(initialBoard);
    }

    public void RestartPuzzle() { if (ready) ApplyBoard(initialBoard); }

    /// <summary>칸 선택만 해제한다(고른 도형은 그대로 둔다).</summary>
    public void ClearSelection()
    {
        if (selectedCell == null) return;
        selectedCell = null;
        RefreshSelection();
        Changed?.Invoke();
    }

    public void OnCellClicked(Cell cell)
    {
        if (!InputEnabled || complete) return;
        // 같은 칸을 다시 누르면 선택 해제 (도형 선택과 동일한 토글)
        if (selectedCell == cell) { ClearSelection(); return; }
        selectedCell = cell;
        RefreshSelection();
        // 도형을 고르지 않은 클릭은 '둘러보기'이므로 경고하지 않는다.
        if (cell.isInitial)
        {
            if (selectedShape != ShapeType.None)
                Feedback?.Invoke("처음부터 놓인 도형은 바꿀 수 없어요.");
            return;
        }
        if (placeOnCellClick) PlaceShape();
    }

    public void SelectShape(int shapeIndex)
    {
        if (!InputEnabled || complete || shapeIndex < 1 || shapeIndex > 5) return;
        var shape = (ShapeType)shapeIndex;
        // 같은 도형을 다시 누르면 선택을 해제한다.
        // 항상 무언가 선택돼 있으면 칸을 둘러보려던 클릭에도 도형이 놓여 버린다.
        selectedShape = selectedShape == shape ? ShapeType.None : shape;
        // 칸이 먼저 선택돼 있으면 도형을 누르는 순간 놓는다(순서와 무관).
        if (selectedCell != null) PlaceShape();
        Changed?.Invoke();
    }

    void PlaceShape()
    {
        if (selectedCell == null || !InputEnabled || complete) return;
        // 선택한 도형이 없으면 칸 선택(강조)만 하고 아무것도 놓지 않는다.
        if (selectedShape == ShapeType.None) return;
        // 고정 칸을 켜 둔 채 도형을 눌렀을 때. 조용히 무시하면 고장난 줄 안다.
        if (selectedCell.isInitial)
        {
            Feedback?.Invoke("처음부터 놓인 도형은 바꿀 수 없어요.");
            return;
        }
        if (isMemoMode)
        {
            if (selectedCell.memos.Contains(selectedShape)) selectedCell.RemoveMemo(selectedShape);
            else selectedCell.AddMemo(selectedShape);
            return;
        }
        if (!PuzzleSolver.CanPlace(GetBoard(), selectedCell.row, selectedCell.col, selectedShape))
        {
            selectedCell.ShowConflict();
            Feedback?.Invoke("같은 행·열·블록에 세 번째 도형은 놓을 수 없어요.");
            return;
        }
        if (selectedCell.currentShape == selectedShape) return;
        selectedCell.SetShape(selectedShape);
        selectedCell.PlayPlacement();
        SFXManager.PlayPlace();
        // 놓은 뒤에는 칸 선택을 푼다. 칸이 계속 켜져 있으면 다음에 다른 도형을
        // 고르는 순간 방금 놓은 칸이 바뀌어 버린다.
        selectedCell = null;
        AfterChange();
    }

    public void ToggleMemoMode() { if (InputEnabled && !complete) isMemoMode = !isMemoMode; }

    /// <summary>힌트를 한 칸 채운다. 실제로 채웠을 때만 true(횟수 차감 기준).</summary>
    public bool UseHint()
    {
        if (!InputEnabled || complete || !ready) return false;
        Cell target = selectedCell;
        if (target == null || target.isInitial || target.currentShape != ShapeType.None)
        {
            target = null;
            for (int r = 0; r < 9 && target == null; r++)
                for (int c = 0; c < 9; c++)
                    if (cells[r, c].currentShape == ShapeType.None) { target = cells[r, c]; break; }
        }
        if (target == null) return false;
        var current = GetBoard();
        bool compatible = true;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (current[r, c] != ShapeType.None && current[r, c] != solutionBoard[r, c]) compatible = false;
        ShapeType[,] answer = solutionBoard;
        if (!compatible && !PuzzleSolver.TrySolve(current, out answer, out bool timeout))
        {
            Feedback?.Invoke(timeout ? "힌트 계산이 길어졌어요. 최근 도형을 지우고 다시 시도해 주세요." : "현재 배치로는 완성할 수 없어요. 놓은 도형 일부를 바꿔 주세요.");
            return false;
        }
        selectedCell = target;
        target.SetShape(answer[target.row, target.col]);
        target.PlayPlacement();
        SFXManager.PlayHint();
        Feedback?.Invoke("지금 배치와 연결되는 도형을 하나 채웠어요.");
        AfterChange();
        return true;
    }

    public void ClearCell()
    {
        if (!InputEnabled || complete || selectedCell == null || selectedCell.isInitial) return;
        selectedCell.ClearMemos();
        selectedCell.SetShape(ShapeType.None);
        SFXManager.PlayErase();
        AfterChange();
    }

    void AfterChange()
    {
        RefreshSelection();
        Changed?.Invoke();
        if (!PuzzleSolver.IsValid(GetBoard(), true)) return;
        complete = true;
        if (Completed != null) Completed.Invoke();
        else FindFirstObjectByType<GameController>()?.OnPuzzleComplete();
    }

    void RefreshSelection()
    {
        foreach (var cell in cells)
            cell.SetSelection(cell == selectedCell, selectedCell != null &&
                (cell.row == selectedCell.row || cell.col == selectedCell.col || cell.blockIndex == selectedCell.blockIndex));
    }

    public Sprite GetShapeSprite(ShapeType shape) => shape switch
    {
        ShapeType.Triangle => triangleSprite, ShapeType.Circle => circleSprite,
        ShapeType.Square => squareSprite, ShapeType.Pentagon => pentagonSprite,
        ShapeType.Star => starSprite, _ => null
    };
}
