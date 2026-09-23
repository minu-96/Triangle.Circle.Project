using UnityEngine;
using System.Collections.Generic;

public class PuzzleGenerator : MonoBehaviour
{
    // 빈칸 수 밸런스는 StageBalance 로 옮겼다. 이 필드는 기존 씬 호환용으로만 남아 있다.
    public DifficultySettings settings;

    // 완전한 보드 생성 (정답용)
    public ShapeType[,] GenerateCompletePuzzle()
    {
        // Rule-preserving permutations avoid unbounded random backtracking at startup.
        int[] template = {
            1,1,2,3,4,0,4,2,3, 0,3,4,1,0,1,3,4,2, 4,0,2,4,3,2,0,1,1,
            0,0,1,4,2,4,3,3,2, 3,2,4,3,2,1,4,1,0, 4,2,1,1,3,0,0,2,4,
            2,1,3,2,4,4,1,0,0, 1,4,3,0,1,2,2,3,4, 2,4,0,0,1,3,2,4,3
        };
        var shapes = new List<int> { 1, 2, 3, 4, 5 };
        Shuffle(shapes);
        var rows = ShuffledAxis();
        var columns = ShuffledAxis();
        bool transpose = Random.value < 0.5f;
        var board = new ShapeType[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                board[r, c] = (ShapeType)shapes[template[transpose ? columns[c] * 9 + rows[r] : rows[r] * 9 + columns[c]]];
        return board;
    }

    List<int> ShuffledAxis()
    {
        var groups = new List<int> { 0, 1, 2 };
        Shuffle(groups);
        var result = new List<int>();
        foreach (int group in groups)
        {
            var offsets = new List<int> { 0, 1, 2 };
            Shuffle(offsets);
            foreach (int offset in offsets) result.Add(group * 3 + offset);
        }
        return result;
    }
    
    // 정답에서 일부를 제거하여 퍼즐 생성
    public ShapeType[,] CreatePuzzleFromSolution(ShapeType[,] solution, GameDifficulty difficulty)
    {
        ShapeType[,] puzzle = (ShapeType[,])solution.Clone();

        int emptyCells;
        RemovalStrategy strategy;

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager가 없습니다!");
            emptyCells = 35;
            strategy = RemovalStrategy.Random;
        }
        else if (GameManager.Instance.currentMode == GameMode.Stage)
        {
            // 스테이지 모드: 빈칸 수는 스테이지 번호, 제거 전략은 챕터별
            int stage = GameManager.Instance.currentStage;
            emptyCells = StageBalance.StageBlanks(stage);
            strategy = Chapters.GetStrategyForStage(stage);
            Debug.Log($"[PuzzleGenerator] Stage {stage} (챕터 {Chapters.GetChapter(stage)}), 빈칸 {emptyCells}, 전략 {strategy}");
        }
        else if (GameManager.Instance.currentMode == GameMode.Endless)
        {
            // 엔드리스: 4장(복합) 난이도, 매 판 무작위 빈칸 + 복합 전략
            emptyCells = StageBalance.EndlessBlanks();
            strategy = RemovalStrategy.Composite;
            Debug.Log($"[PuzzleGenerator] Endless, 빈칸 {emptyCells}, 전략 {strategy}");
        }
        else
        {
            // 클래식: 난이도별 빈칸 수만 다르고 제거는 전 난이도 완전 랜덤
            emptyCells = StageBalance.ClassicBlanks(difficulty);
            strategy = RemovalStrategy.Random;
            Debug.Log($"[PuzzleGenerator] Classic, 난이도 {difficulty}, 빈칸 {emptyCells}, 전략 Random");
        }

        RemoveCells(puzzle, emptyCells, strategy);

        return puzzle;
    }

    /// <summary>
    /// 빈 칸 일부에 후보 2개짜리 메모를 만들어 비트마스크(도형 1~5 = 비트 1~5)로 돌려준다.
    /// 후보는 "정답 + 그 칸에 놓을 수 있는 다른 도형 1개"이므로 정답이 반드시 포함된다.
    /// 처음 한 번만 계산하며 이후 갱신하지 않는다 — 실시간으로 갱신하면
    /// "후보가 가장 적은 칸부터 채운다"는 최적 전략을 그대로 쥐여주게 된다.
    /// </summary>
    public int[] CreateMemos(ShapeType[,] puzzle, ShapeType[,] solution, int count)
    {
        var masks = new int[81];
        if (count <= 0 || puzzle == null || solution == null) return masks;

        var pool = new List<int>();
        for (int i = 0; i < 81; i++)
            if (puzzle[i / 9, i % 9] == ShapeType.None) pool.Add(i);
        Shuffle(pool);

        var legal = new List<ShapeType>();
        int placed = 0;
        foreach (int index in pool)
        {
            if (placed >= count) break;
            int r = index / 9, c = index % 9;
            legal.Clear();
            for (int v = 1; v <= 5; v++)
                if (PuzzleSolver.CanPlace(puzzle, r, c, (ShapeType)v)) legal.Add((ShapeType)v);

            var answer = solution[r, c];
            if (legal.Count < 2 || !legal.Contains(answer)) continue;

            ShapeType other;
            do { other = legal[Random.Range(0, legal.Count)]; } while (other == answer);
            masks[index] = (1 << (int)answer) | (1 << (int)other);
            placed++;
        }
        return masks;
    }

    /// <summary>
    /// 빈칸 수와 제거 전략을 직접 지정해 퍼즐을 만든다.
    /// 밸런스 측정처럼 GameManager 상태와 무관하게 특정 조건을 재현해야 할 때 쓴다.
    /// </summary>
    public ShapeType[,] CreatePuzzle(ShapeType[,] solution, int blanks, RemovalStrategy strategy)
    {
        var puzzle = (ShapeType[,])solution.Clone();
        RemoveCells(puzzle, blanks, strategy);
        return puzzle;
    }

    public ShapeType[,] GeneratePuzzle(GameDifficulty difficulty)
    {
        return CreatePuzzleFromSolution(GenerateCompletePuzzle(), difficulty);
    }

    // 제거 전략에 따라 count개의 칸을 비운다.
    void RemoveCells(ShapeType[,] board, int count, RemovalStrategy strategy)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
                positions.Add(new Vector2Int(row, col));

        List<Vector2Int> ordered;
        switch (strategy)
        {
            case RemovalStrategy.CenterFocused:
                ordered = GetCenterFocusedPositions(positions);
                break;
            case RemovalStrategy.BlockEven:
                ordered = GetStrategicPositions(positions);
                break;
            case RemovalStrategy.Composite:
                ordered = GetCompositePositions(positions);
                break;
            default: // Random
                Shuffle(positions);
                ordered = positions;
                break;
        }

        for (int i = 0; i < count && i < ordered.Count; i++)
        {
            Vector2Int pos = ordered[i];
            board[pos.x, pos.y] = ShapeType.None;
        }
    }

    // 통합 제거: 세 배치를 번갈아 뽑아 한 판에 섞는다 (4장).
    // 셋 중 하나를 고르는 것이 아니라, 중심부·균등·랜덤 순서를 돌아가며 한 칸씩 가져온다.
    List<Vector2Int> GetCompositePositions(List<Vector2Int> positions)
    {
        var sources = new List<Vector2Int>[]
        {
            GetCenterFocusedPositions(positions),
            GetStrategicPositions(positions),
            ShuffledCopy(positions),
        };
        var cursor = new int[sources.Length];
        var taken = new HashSet<Vector2Int>();
        var ordered = new List<Vector2Int>(positions.Count);

        for (int turn = 0; ordered.Count < positions.Count; turn++)
        {
            int s = turn % sources.Length;
            var list = sources[s];
            while (cursor[s] < list.Count && taken.Contains(list[cursor[s]])) cursor[s]++;
            if (cursor[s] >= list.Count)
            {
                // 이 배치가 소진됐다면 다른 배치가 남아 있는지 확인한다.
                bool any = false;
                for (int i = 0; i < sources.Length; i++) if (cursor[i] < sources[i].Count) any = true;
                if (!any) break;
                continue;
            }
            var pick = list[cursor[s]++];
            taken.Add(pick);
            ordered.Add(pick);
        }
        return ordered;
    }

    List<Vector2Int> ShuffledCopy(List<Vector2Int> source)
    {
        var copy = new List<Vector2Int>(source);
        Shuffle(copy);
        return copy;
    }

    // 중심부 위주 제거: 각 블록의 중앙 → 중앙의 상하좌우 → 나머지(모서리) 순으로 정렬 (3장)
    List<Vector2Int> GetCenterFocusedPositions(List<Vector2Int> positions)
    {
        List<Vector2Int> center = new List<Vector2Int>();   // 우선순위 0: 블록 중앙
        List<Vector2Int> adjacent = new List<Vector2Int>(); // 우선순위 1: 중앙의 상하좌우
        List<Vector2Int> rest = new List<Vector2Int>();     // 우선순위 2: 나머지

        foreach (var pos in positions)
        {
            int rIn = pos.x % 3; // 블록 내 행 (0~2)
            int cIn = pos.y % 3; // 블록 내 열 (0~2)
            bool isCenter = (rIn == 1 && cIn == 1);
            bool isAdjacent = (rIn == 1 || cIn == 1) && !isCenter; // 십자(모서리 제외)

            if (isCenter) center.Add(pos);
            else if (isAdjacent) adjacent.Add(pos);
            else rest.Add(pos);
        }

        // 같은 우선순위 안에서는 무작위로 섞어 매 판 다르게
        Shuffle(center); Shuffle(adjacent); Shuffle(rest);

        List<Vector2Int> ordered = new List<Vector2Int>();
        ordered.AddRange(center);
        ordered.AddRange(adjacent);
        ordered.AddRange(rest);
        return ordered;
    }

    // 블록별 균등 배분 제거: 각 블록에서 고르게 뽑은 뒤 나머지 (3장)
    List<Vector2Int> GetStrategicPositions(List<Vector2Int> positions)
    {
        var blocks = new List<Vector2Int>[9];
        for (int b = 0; b < 9; b++) blocks[b] = new List<Vector2Int>();
        foreach (var p in positions) blocks[p.x / 3 * 3 + p.y / 3].Add(p);
        foreach (var block in blocks) Shuffle(block);
        var result = new List<Vector2Int>();
        var order = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        for (int i = 0; i < 9; i++)
        {
            Shuffle(order);
            foreach (int b in order) result.Add(blocks[b][i]);
        }
        return result;
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