using System;
using UnityEngine;

[Serializable]
public class PuzzleSave
{
    public int version = 1;
    public GameMode mode;
    public GameDifficulty difficulty;
    public int stage;
    public float elapsed;
    public int hintsUsed;               // 한 판에 쓴 힌트 수 (이어하기로 초기화되지 않도록 저장)
    public int[] initial, board, solution;
    const string Key = "SamgakwonResume_v1";

    public static int[] Flatten(ShapeType[,] source)
    {
        var result = new int[81];
        for (int i = 0; i < 81; i++) result[i] = (int)source[i / 9, i % 9];
        return result;
    }
    public static ShapeType[,] Expand(int[] values)
    {
        var result = new ShapeType[9, 9];
        for (int i = 0; i < 81; i++) result[i / 9, i % 9] = (ShapeType)values[i];
        return result;
    }
    public bool IsValid()
    {
        if (version != 1 || (mode != GameMode.Classic && mode != GameMode.Stage) ||
            (int)difficulty < 0 || (int)difficulty > 2 || stage < 1 || stage > 81 ||
            float.IsNaN(elapsed) || float.IsInfinity(elapsed) || elapsed < 0 ||
            hintsUsed < 0 || hintsUsed > 99 ||
            initial == null || board == null || solution == null ||
            initial.Length != 81 || board.Length != 81 || solution.Length != 81) return false;
        for (int i = 0; i < 81; i++)
            if (initial[i] < 0 || initial[i] > 5 || board[i] < 0 || board[i] > 5 ||
                solution[i] < 1 || solution[i] > 5 ||
                (initial[i] != 0 && (initial[i] != board[i] || initial[i] != solution[i]))) return false;
        return PuzzleSolver.IsValid(Expand(board)) && !PuzzleSolver.IsValid(Expand(board), true) &&
            PuzzleSolver.IsValid(Expand(solution), true);
    }
    public static PuzzleSave Load()
    {
        if (!PlayerPrefs.HasKey(Key)) return null;
        try
        {
            var save = JsonUtility.FromJson<PuzzleSave>(PlayerPrefs.GetString(Key));
            return save != null && save.IsValid() ? save : null;
        }
        catch (ArgumentException) { return null; }
    }
    public static void Store(BoardManager source, float elapsed, int hintsUsed = 0)
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.currentMode == GameMode.Endless) return;
        var save = new PuzzleSave { mode = gm.currentMode, difficulty = gm.currentDifficulty,
            stage = gm.currentStage, elapsed = elapsed, hintsUsed = hintsUsed,
            initial = Flatten(source.GetInitialBoard()),
            board = Flatten(source.GetBoard()), solution = Flatten(source.GetSolutionBoard()) };
        if (!save.IsValid()) return;
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(save));
        PlayerPrefs.Save();
    }
    public static void Clear() { PlayerPrefs.DeleteKey(Key); PlayerPrefs.Save(); }
}
