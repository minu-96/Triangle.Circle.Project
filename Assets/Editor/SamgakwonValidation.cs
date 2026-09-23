using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SamgakwonValidation
{
    static int checks;
    static readonly List<string> results = new List<string>();
    static void Check(bool condition, string message)
    {
        checks++;
        if (!condition) throw new Exception("Validation failed: " + message);
    }

    [MenuItem("Samgakwon/Open Playable Scene")]
    public static void OpenScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene("Assets/WebScene/Samgakwon.unity");
    }

    [MenuItem("Samgakwon/Validate Puzzle and Save")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode before running editor validation.");
        checks = 0; results.Clear();
        var random = UnityEngine.Random.state;
        var singleton = typeof(GameManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        var previous = GameManager.Instance;
        var root = new GameObject("Validation (temporary)") { hideFlags = HideFlags.HideAndDontSave };
        var preview = EditorSceneManager.NewPreviewScene();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, preview);
        string oldSave = PlayerPrefs.HasKey("SamgakwonResume_v1") ? PlayerPrefs.GetString("SamgakwonResume_v1") : null;
        try
        {
            var gm = root.AddComponent<GameManager>(); singleton.SetValue(null, gm);
            var generator = root.AddComponent<PuzzleGenerator>();
            UnityEngine.Random.InitState(1435);
            for (int i = 0; i < 300; i++) Check(PuzzleSolver.IsValid(generator.GenerateCompletePuzzle(), true), "generated board obeys every unit");
            var solution = generator.GenerateCompletePuzzle();
            gm.currentMode = GameMode.Classic;
            int[] blanks = { 15, 35, 55 };
            for (int d = 0; d < 3; d++)
            {
                var puzzle = generator.CreatePuzzleFromSolution(solution, (GameDifficulty)d);
                Check(EmptyCount(puzzle) == blanks[d], "classic blank count");
                Check(PuzzleSolver.IsValid(puzzle), "classic clues valid");
            }
            results.Add("300 valid generated boards; classic 15/35/55 blanks");
            gm.currentMode = GameMode.Stage;
            for (int stage = 1; stage <= 81; stage++)
            {
                gm.currentStage = stage;
                var puzzle = generator.CreatePuzzleFromSolution(solution, GameDifficulty.Normal);
                Check(EmptyCount(puzzle) == stage, "stage blank count");
                if (stage >= 21 && stage <= 40)
                    for (int b = 0; b < 9; b++) Check(puzzle[b / 3 * 3 + 1, b % 3 * 3 + 1] == ShapeType.None, "centers removed first");
                if (stage >= 41 && stage <= 60)
                {
                    int min = 9, max = 0;
                    for (int b = 0; b < 9; b++)
                    {
                        int count = 0;
                        for (int i = 0; i < 9; i++) if (puzzle[b / 3 * 3 + i / 3, b % 3 * 3 + i % 3] == ShapeType.None) count++;
                        min = Mathf.Min(min, count); max = Mathf.Max(max, count);
                    }
                    Check(max - min <= 1, "block-even distribution differs by at most one");
                }
            }
            Check(Chapters.GetChapter(81) == 4, "stage 81 belongs to chapter 4");
            gm.currentMode = GameMode.Endless;
            for (int i = 0; i < 30; i++)
            {
                int count = EmptyCount(generator.CreatePuzzleFromSolution(solution, GameDifficulty.Hard));
                Check(count >= 61 && count <= 75, "endless blank range");
            }
            results.Add("81 stage counts, center-first, even blocks, empty stage 81, endless range");
            gm.currentMode = GameMode.Classic; gm.currentStage = 1; gm.currentDifficulty = GameDifficulty.Easy;
            root.AddComponent<RuleChecker>();
            var board = root.AddComponent<BoardManager>();
            var holder = new GameObject("Cells", typeof(RectTransform)); holder.transform.SetParent(root.transform);
            var template = new GameObject("Template", typeof(RectTransform), typeof(Cell)); template.transform.SetParent(root.transform);
            template.SetActive(false);
            board.cellPrefab = template; board.boardParent = holder.transform;
            board.InitializeBoard();
            var fixedCell = Array.Find(holder.GetComponentsInChildren<Cell>(), c => c.isInitial);
            ShapeType clue = fixedCell.currentShape;
            board.OnCellClicked(fixedCell); board.SelectShape((int)clue % 5 + 1); board.ClearCell();
            Check(fixedCell.currentShape == clue && fixedCell.isInitial, "fixed clue cannot be overwritten or erased");
            var initial = new ShapeType[9, 9];
            var alternate = (ShapeType[,])solution.Clone();
            for (int r = 0; r < 9; r++) for (int c = 0; c < 9; c++) alternate[r, c] = (ShapeType)((int)alternate[r, c] % 5 + 1);
            var expected = (ShapeType[,])alternate.Clone(); alternate[0, 0] = ShapeType.None;
            int completions = 0; board.Completed += () => completions++;
            board.Restore(initial, alternate, solution);
            board.UseHint();
            var actual = board.GetBoard();
            Check(PuzzleSolver.IsValid(actual, true), "hint completes alternate valid solution");
            for (int i = 1; i < 81; i++) Check(actual[i / 9, i % 9] == expected[i / 9, i % 9], "hint preserves existing player choices");
            board.UseHint(); board.SelectShape(3); board.ClearCell();
            Check(completions == 1, "completion emits once and locks input");
            board.Restore(initial, alternate, solution);
            board.InputEnabled = false; board.OnCellClicked(holder.GetComponentsInChildren<Cell>()[0]); board.UseHint();
            Check(board.GetBoard()[0, 0] == ShapeType.None, "pause blocks hints and placement");
            board.InputEnabled = true;
            PuzzleSave.Store(board, 125.5f);
            var saved = PuzzleSave.Load();
            Check(saved != null && Mathf.Abs(saved.elapsed - 125.5f) < 0.001f, "save round trip");
            saved.board[0] = 9; Check(!saved.IsValid(), "reject malformed save");
            Check(!PuzzleSolver.TrySolve(new ShapeType[9, 9], out _, out bool timedOut, 0) && timedOut, "solver budget enforced");
            board.RestartPuzzle(); Check(EmptyCount(board.GetBoard()) == 81, "restart restores same initial puzzle");
            results.Add("fixed-clue protection, alternate-solution hint, completion idempotence, pause guard, save validation, solver time budget");
            Check(Resources.Load<Font>("Fonts/NanumGothic-Regular") != null, "packaged Korean font");
            foreach (string asset in new[] { "tokens/triangle_normal", "tokens/circle_normal", "tokens/square_normal", "tokens/diamond_normal", "tokens/pentagon_normal", "icons/info_dark", "icons/settings_dark", "icons/back_dark", "icons/pause_dark", "icons/lock_dark", "icons/check_teal", "buttons/primary_normal", "buttons/secondary_normal", "panels/popup" })
                Check(Resources.LoadAll<Sprite>("samgakwon-assets-v1/png/" + asset).Length > 0, "UI asset " + asset);
            results.Add("Korean font and all required presentation assets load");
            string report = $"PASS — {checks} assertions\n" + string.Join("\n", results);
            Debug.Log(report);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "samgakwon-validation.txt"), report);
        }
        finally
        {
            singleton.SetValue(null, previous);
            EditorSceneManager.ClosePreviewScene(preview);
            UnityEngine.Random.state = random;
            if (oldSave == null) PlayerPrefs.DeleteKey("SamgakwonResume_v1"); else PlayerPrefs.SetString("SamgakwonResume_v1", oldSave);
            PlayerPrefs.Save();
        }
    }

    public static void RunAndBuild()
    {
        Run();
        var output = Path.Combine(Path.GetTempPath(), "SamgakwonPreview.app");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/WebScene/Samgakwon.unity" }, locationPathName = output,
            target = BuildTarget.StandaloneOSX, options = BuildOptions.Development });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Preview build failed: " + report.summary.result);
        Debug.Log("PREVIEW_BUILD_OK " + output);
    }
    static int EmptyCount(ShapeType[,] board) { int count = 0; foreach (var value in board) if (value == ShapeType.None) count++; return count; }
}
