using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 빈칸 수와 난이도의 관계를 숫자로 재는 도구.
///
/// 이 게임의 승리 조건은 "규칙을 만족하며 모두 채움"이지 생성된 정답과 일치하는 것이 아니다.
/// 그래서 빈칸이 많을수록 어렵다는 통념이 성립하지 않는다 — 단서가 거의 없으면
/// 아무 유효한 배치나 만들어도 이기기 때문이다. StageBalance 의 곡선은 이 가정 위에 있으므로,
/// 가정이 맞는지 세 가지 대리지표로 확인한다.
///
///  · 단독 확정 진행률 : 선택지가 1개뿐인 칸만 반복해 채웠을 때 메워지는 비율.
///                       높을수록 추론만으로 풀려 쉽다.
///  · 막힘률           : 무작위로 합법적인 도형을 채워 나갈 때 막다른 길에 빠지는 비율.
///                       높을수록 잘못 고르기 쉬워 어렵다.
///  · 탐색 노드 수     : 해답 하나를 찾기까지 탐색한 칸 수(MRV 백트래킹).
///                       높을수록 경우의 수가 얽혀 있다.
/// </summary>
public static class SamgakwonBalance
{
    const int SamplesPerPoint = 40;
    const int GreedyTrials = 12;
    const int NodeCap = 300000;

    [MenuItem("Samgakwon/Validate/난이도 곡선 측정 (빈칸 수 스윕)")]
    public static void SweepBlanks()
    {
        var generator = CreateGenerator();
        try
        {
            var report = new StringBuilder();
            report.AppendLine("blanks,strategy,singles_progress,dead_end_rate,search_nodes");
            Debug.Log("[삼각원] 빈칸 수 스윕 시작 — 각 지점 " + SamplesPerPoint + "판");

            var strategies = new[] { RemovalStrategy.Random, RemovalStrategy.CenterFocused, RemovalStrategy.BlockEven };
            var lines = new List<string>();
            lines.Add(string.Format("{0,7}{1,16}{2,14}{3,14}", "빈칸", "단독확정 진행률", "막힘률", "탐색 노드"));

            for (int blanks = 10; blanks <= 81; blanks += 3)
            {
                double singles = 0, dead = 0, nodes = 0;
                int total = 0;
                foreach (var strategy in strategies)
                {
                    var m = Measure(generator, blanks, strategy);
                    report.AppendLine($"{blanks},{strategy},{m.singles:F4},{m.deadEnd:F4},{m.nodes:F1}");
                    singles += m.singles; dead += m.deadEnd; nodes += m.nodes; total++;
                }
                lines.Add(string.Format("{0,7}{1,15:P0}{2,14:P0}{3,14:N0}",
                    blanks, singles / total, dead / total, nodes / total));
                if (EditorUtility.DisplayCancelableProgressBar("난이도 곡선 측정",
                    $"빈칸 {blanks} / 81", (blanks - 10) / 71f)) break;
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("[삼각원] 빈칸 수 스윕 (세 전략 평균)\n" + string.Join("\n", lines));
            WriteCsv("SamgakwonBalance-blanks.csv", report.ToString());
        }
        finally { EditorUtility.ClearProgressBar(); Cleanup(generator); }
    }

    [MenuItem("Samgakwon/Validate/실제 스테이지 1~81 측정")]
    public static void SweepStages()
    {
        var generator = CreateGenerator();
        try
        {
            var report = new StringBuilder();
            report.AppendLine("stage,chapter,blanks,strategy,singles_progress,dead_end_rate,search_nodes");
            var lines = new List<string>();
            lines.Add(string.Format("{0,7}{1,7}{2,7}{3,16}{4,14}{5,14}", "스테이지", "장", "빈칸", "단독확정 진행률", "막힘률", "탐색 노드"));

            for (int stage = 1; stage <= 81; stage += 4)
            {
                int blanks = StageBalance.StageBlanks(stage);
                var strategy = Chapters.GetStrategyForStage(stage);
                var m = Measure(generator, blanks, strategy);
                report.AppendLine($"{stage},{Chapters.GetChapter(stage)},{blanks},{strategy},{m.singles:F4},{m.deadEnd:F4},{m.nodes:F1}");
                lines.Add(string.Format("{0,7}{1,7}{2,7}{3,15:P0}{4,14:P0}{5,14:N0}",
                    stage, Chapters.GetChapter(stage), blanks, m.singles, m.deadEnd, m.nodes));
                if (EditorUtility.DisplayCancelableProgressBar("스테이지 측정", $"스테이지 {stage} / 81", stage / 81f)) break;
            }
            EditorUtility.ClearProgressBar();
            Debug.Log("[삼각원] 실제 스테이지 난이도\n" + string.Join("\n", lines));
            WriteCsv("SamgakwonBalance-stages.csv", report.ToString());
        }
        finally { EditorUtility.ClearProgressBar(); Cleanup(generator); }
    }

    // ── 측정 ────────────────────────────────────────────────
    struct Metrics { public double singles, deadEnd, nodes; }

    static Metrics Measure(PuzzleGenerator generator, int blanks, RemovalStrategy strategy)
    {
        double singles = 0, dead = 0, nodes = 0;
        for (int i = 0; i < SamplesPerPoint; i++)
        {
            var solution = generator.GenerateCompletePuzzle();
            var puzzle = generator.CreatePuzzle(solution, blanks, strategy);
            singles += SinglesProgress(puzzle);
            dead += DeadEndRate(puzzle);
            nodes += SearchNodes(puzzle);
        }
        return new Metrics
        {
            singles = singles / SamplesPerPoint,
            deadEnd = dead / SamplesPerPoint,
            nodes = nodes / SamplesPerPoint
        };
    }

    /// <summary>선택지가 1개뿐인 칸만 반복해 채웠을 때 메워지는 빈칸 비율.</summary>
    static double SinglesProgress(ShapeType[,] source)
    {
        var board = (ShapeType[,])source.Clone();
        int blanks = CountBlanks(board);
        if (blanks == 0) return 1;
        int filled = 0;
        bool progressed = true;
        while (progressed)
        {
            progressed = false;
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] != ShapeType.None) continue;
                    ShapeType only = ShapeType.None;
                    int options = 0;
                    for (int v = 1; v <= 5 && options < 2; v++)
                        if (PuzzleSolver.CanPlace(board, r, c, (ShapeType)v)) { options++; only = (ShapeType)v; }
                    if (options == 1) { board[r, c] = only; filled++; progressed = true; }
                }
        }
        return (double)filled / blanks;
    }

    /// <summary>무작위로 합법 수를 채워 나갈 때 막다른 길에 빠지는 비율.</summary>
    static double DeadEndRate(ShapeType[,] source)
    {
        int failures = 0;
        for (int trial = 0; trial < GreedyTrials; trial++)
        {
            var board = (ShapeType[,])source.Clone();
            var cells = new List<int>();
            for (int i = 0; i < 81; i++) if (board[i / 9, i % 9] == ShapeType.None) cells.Add(i);
            for (int i = cells.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (cells[i], cells[j]) = (cells[j], cells[i]);
            }
            foreach (int index in cells)
            {
                int r = index / 9, c = index % 9;
                var options = new List<ShapeType>();
                for (int v = 1; v <= 5; v++)
                    if (PuzzleSolver.CanPlace(board, r, c, (ShapeType)v)) options.Add((ShapeType)v);
                if (options.Count == 0) { failures++; break; }
                board[r, c] = options[UnityEngine.Random.Range(0, options.Count)];
            }
        }
        return (double)failures / GreedyTrials;
    }

    /// <summary>해답 하나를 찾기까지 방문한 칸 수(MRV 백트래킹). 상한에 걸리면 상한을 돌려준다.</summary>
    static double SearchNodes(ShapeType[,] source)
    {
        var board = (ShapeType[,])source.Clone();
        int visited = 0;
        bool Search()
        {
            if (visited >= NodeCap) return true;
            int row = -1, col = -1, minimum = 6;
            var best = new List<ShapeType>();
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] != ShapeType.None) continue;
                    var options = new List<ShapeType>();
                    for (int v = 1; v <= 5; v++)
                        if (PuzzleSolver.CanPlace(board, r, c, (ShapeType)v)) options.Add((ShapeType)v);
                    if (options.Count == 0) return false;
                    if (options.Count < minimum) { row = r; col = c; minimum = options.Count; best = options; }
                }
            if (row < 0) return true;
            foreach (var value in best)
            {
                visited++;
                board[row, col] = value;
                if (Search()) return true;
                board[row, col] = ShapeType.None;
                if (visited >= NodeCap) return true;
            }
            return false;
        }
        Search();
        return visited;
    }

    static int CountBlanks(ShapeType[,] board)
    {
        int n = 0;
        foreach (var value in board) if (value == ShapeType.None) n++;
        return n;
    }

    // ── 보조 ────────────────────────────────────────────────
    static PuzzleGenerator CreateGenerator()
    {
        var host = new GameObject("Balance (temporary)") { hideFlags = HideFlags.HideAndDontSave };
        return host.AddComponent<PuzzleGenerator>();
    }

    static void Cleanup(PuzzleGenerator generator)
    {
        if (generator != null) UnityEngine.Object.DestroyImmediate(generator.gameObject);
    }

    static void WriteCsv(string name, string content)
    {
        string path = Path.Combine(Application.dataPath, "..", name);
        File.WriteAllText(Path.GetFullPath(path), content);
        Debug.Log("[삼각원] CSV 저장: " + Path.GetFullPath(path));
    }
}
