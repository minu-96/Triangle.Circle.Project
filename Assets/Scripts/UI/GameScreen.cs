using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>퍼즐을 푸는 화면. 보드 엔진과 화면 위젯을 이어 준다.</summary>
public class GameScreen : UIScreen
{
    [Header("Engine")]
    public BoardManager board;

    [Header("Header")]
    public TMP_Text metaText;
    public TMP_Text titleText;
    public TMP_Text timerText;
    public Button pauseButton;

    [Header("Controls")]
    public ShapePieceView[] rack = new ShapePieceView[5];
    public LabeledButton eraseButton;
    public LabeledButton hintButton;
    public LabeledButton memoButton;

    [Header("Rules")]
    [Tooltip("한 판에 쓸 수 있는 힌트 횟수. 힌트는 정답을 바로 채우므로 제한한다.")]
    [Min(0)] public int maxHints = 3;

    [Header("Readouts")]
    public TMP_Text ruleText;
    public TMP_Text filledText;
    public TMP_Text footerText;
    public Image progressFill;

    float elapsed, saveClock;
    int hintsUsed;
    bool won, sessionActive, wired;

    public bool SessionActive => sessionActive && !won;

    public override void Bind(SamgakwonRouter router)
    {
        base.Bind(router);
        if (wired) return;
        wired = true;

        if (pauseButton != null) pauseButton.onClick.AddListener(Pause);
        if (eraseButton != null) eraseButton.Set("지우기", Erase, false, Router.Theme, Router.Theme != null ? Router.Theme.iconErase : null);
        if (hintButton != null) hintButton.Set("힌트", Hint, false, Router.Theme, Router.Theme != null ? Router.Theme.iconHint : null);
        if (memoButton != null) memoButton.Set("메모", ToggleMemo, false, Router.Theme, Router.Theme != null ? Router.Theme.iconMemo : null);
        RefreshHintButton();

        for (int i = 0; i < rack.Length; i++)
        {
            int value = i + 1;
            if (rack[i] == null) continue;
            rack[i].Set(value, Router.Theme != null ? Router.Theme.Token((ShapeType)value) : null, Router.Theme,
                () => { if (board != null) board.SelectShape(value); });
        }

        if (ruleText != null)
        {
            ruleText.text = "행 · 열 · 블록마다 같은 도형은 최대 2개";
            if (Router.Theme != null) ruleText.color = Router.Theme.ink;
        }
        if (footerText != null)
        {
            footerText.text = "칸과 도형을 고르면 놓여요(순서 무관) · 다시 누르면 해제 · Del 지우기";
            if (Router.Theme != null) footerText.color = Router.Theme.muted;
        }

        if (board != null)
        {
            board.Changed += BoardChanged;
            board.Completed += Completed;
            board.Feedback += OnFeedback;
        }
    }

    void OnDestroy()
    {
        if (board == null) return;
        board.Changed -= BoardChanged;
        board.Completed -= Completed;
        board.Feedback -= OnFeedback;
    }

    // ── 세션 ────────────────────────────────────────────────
    public void StartSession(GameMode mode, int stage, GameDifficulty difficulty, PuzzleSave saved = null)
    {
        var gm = GameManager.Instance;
        if (gm == null || board == null) return;
        gm.currentMode = mode;
        gm.currentDifficulty = difficulty;
        gm.currentStage = Mathf.Clamp(stage, 1, Chapters.TotalStages);

        won = false;
        elapsed = saved != null ? saved.elapsed : 0f;
        hintsUsed = saved != null ? Mathf.Clamp(saved.hintsUsed, 0, maxHints) : 0;
        saveClock = 0f;

        if (!board.IsReady) board.InitializeBoard();
        else board.ResetBoard();
        if (saved != null)
            board.Restore(PuzzleSave.Expand(saved.initial), PuzzleSave.Expand(saved.board),
                          PuzzleSave.Expand(saved.solution), saved.memos);

        board.InputEnabled = true;
        sessionActive = true;
        RefreshHeader();
        RefreshHintButton();
        BoardChanged();
    }

    /// <summary>화면을 떠날 때. 진행 중이면 저장하고 타이머를 멈춘다.</summary>
    public void EndSession()
    {
        Save();
        sessionActive = false;
    }

    void RefreshHeader()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        var theme = Router != null ? Router.Theme : null;

        string title, meta;
        switch (gm.currentMode)
        {
            case GameMode.Stage:
                var info = Chapters.GetInfoForStage(gm.currentStage);
                title = $"스테이지 {gm.currentStage}";
                meta = info.title;
                break;
            case GameMode.Endless:
                title = "엔드리스";
                meta = $"누적 {GameManager.GetEndlessClearCount()}판 완성";
                break;
            default:
                title = $"클래식 · {Difficulty(gm.currentDifficulty)}";
                meta = "조금씩 채워지는 즐거움";
                break;
        }
        if (titleText != null) { titleText.text = title; if (theme != null) titleText.color = theme.ink; }
        if (metaText != null) { metaText.text = meta; if (theme != null) metaText.color = theme.muted; }
        if (timerText != null) { timerText.text = SamgakwonFormat.Time(elapsed); if (theme != null) timerText.color = theme.ink; }
    }

    public static string Difficulty(GameDifficulty value) => value switch
    {
        GameDifficulty.Easy => "쉬움",
        GameDifficulty.Normal => "보통",
        _ => "어려움"
    };

    void Update()
    {
        if (!sessionActive || won) return;
        if (Router != null && Router.IsOverlayOpen) return;

        elapsed += Time.unscaledDeltaTime;
        saveClock += Time.unscaledDeltaTime;
        if (timerText != null) timerText.text = SamgakwonFormat.Time(elapsed);
        if (saveClock > 5f) { Save(); saveClock = 0f; }

#if ENABLE_LEGACY_INPUT_MANAGER
        for (int i = 0; i < 5; i++)
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)) && board != null) board.SelectShape(i + 1);
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) Erase();
#endif
    }

    void Save()
    {
        if (board != null && board.IsReady && !won) PuzzleSave.Store(board, elapsed, hintsUsed);
    }

    // ── 보드 이벤트 ─────────────────────────────────────────
    void BoardChanged()
    {
        if (board == null) return;
        var theme = Router != null ? Router.Theme : null;
        for (int i = 0; i < rack.Length; i++)
            if (rack[i] != null) rack[i].SetSelected(i + 1 == (int)board.SelectedShape, theme);

        int count = 0;
        foreach (var value in board.GetBoard()) if (value != ShapeType.None) count++;
        if (filledText != null) { filledText.text = $"{count} / 81"; if (theme != null) filledText.color = theme.muted; }
        if (progressFill != null) progressFill.fillAmount = count / 81f;
        RefreshMemoButton();
        Save();
    }

    void OnFeedback(string message) => Router?.ShowToast(message);

    public void Erase() { if (board != null) board.ClearCell(); }

    /// <summary>빈 공간을 눌렀을 때. 켜져 있던 칸을 끈다.</summary>
    public void ClearCellSelection() { if (board != null) board.ClearSelection(); }
    public void Hint()
    {
        if (board == null) return;
        if (hintsUsed >= maxHints)
        {
            Router?.ShowToast($"힌트는 한 판에 {maxHints}번까지예요.");
            return;
        }
        // 실제로 채웠을 때만 차감한다(막힌 배치·시간 초과는 그대로 둔다).
        if (!board.UseHint()) return;
        hintsUsed++;
        RefreshHintButton();
        Save();
    }

    void RefreshHintButton()
    {
        if (hintButton == null) return;
        int left = Mathf.Max(0, maxHints - hintsUsed);
        var theme = Router != null ? Router.Theme : null;
        hintButton.Set($"힌트 {left}", Hint, false, theme, theme != null ? theme.iconHint : null);
        hintButton.SetInteractable(left > 0);
    }

    /// <summary>메모 모드 토글. 켜면 칸을 눌렀을 때 도형 대신 메모가 찍힌다.</summary>
    public void ToggleMemo()
    {
        if (board == null) return;
        board.ToggleMemoMode();
        RefreshMemoButton();
        Router?.ShowToast(board.isMemoMode
            ? "메모 모드예요. 칸을 누르면 작게 표시돼요."
            : "메모 모드를 껐어요.");
    }

    void RefreshMemoButton()
    {
        if (memoButton == null) return;
        memoButton.SetHighlighted(board != null && board.isMemoMode, Router != null ? Router.Theme : null);
    }

    // ── 완료 ────────────────────────────────────────────────
    void Completed()
    {
        if (won) return;
        won = true;
        if (board != null) board.InputEnabled = false;

        var gm = GameManager.Instance;
        var theme = Router != null ? Router.Theme : null;
        bool endless = gm.currentMode == GameMode.Endless;
        bool record = !endless && RecordManager.Instance != null && RecordManager.Instance.IsNewRecord(elapsed);

        if (record) SFXManager.PlayNewRecord(); else SFXManager.PlayClear();
        Router?.Celebrate();

        if (endless)
        {
            int total = GameManager.IncrementEndlessClearCount();
            Router?.ShowToast($"완성! 누적 {total}판 · 다음 퍼즐로 이어집니다");
            StartCoroutine(NextEndless());
            return;
        }

        if (RecordManager.Instance != null) RecordManager.Instance.SaveRecord(elapsed);
        PuzzleSave.Clear();

        int stage = gm.currentStage;
        var mode = gm.currentMode;
        var difficulty = gm.currentDifficulty;
        bool finale = mode == GameMode.Stage && stage >= Chapters.TotalStages;

        if (mode == GameMode.Stage)
        {
            StageSelectManager.UnlockStage(stage);
            if (finale) GameManager.UnlockEndless();
        }

        var request = new ModalRequest(record ? "신기록!" : "퍼즐 완성!")
            .Metric(SamgakwonFormat.Time(elapsed))
            .Icon(theme != null ? (record ? theme.iconTrophy : theme.iconCheck) : null)
            .Highlight(record);
        request.body = finale
            ? "81개의 퍼즐을 모두 완성했어요.\n엔드리스 모드가 열렸어요!"
            : $"힌트 {hintsUsed}회 · 작은 도형으로 완성한 한 판.";

        request.Button(finale ? "엔드리스 시작" : mode == GameMode.Stage ? "다음 스테이지" : "한 판 더",
            () => Router?.StartGame(finale ? GameMode.Endless : mode,
                                    mode == GameMode.Stage ? stage + 1 : 1, difficulty));
        request.Button(mode == GameMode.Stage ? "스테이지 선택" : "메인으로",
            () => { if (mode == GameMode.Stage) Router?.ShowStages(); else Router?.ShowHome(); });
        Router?.Modals.Show(request);
    }

    IEnumerator NextEndless()
    {
        yield return new WaitForSecondsRealtime(1.2f);
        // 기다리는 사이 메인/스테이지로 나갔으면 다시 끌고 오지 않는다.
        if (won && isActiveAndEnabled && sessionActive
            && GameManager.Instance != null && GameManager.Instance.currentMode == GameMode.Endless)
            Router?.StartGame(GameMode.Endless, 1, GameDifficulty.Hard);
    }

    // ── 일시정지 ────────────────────────────────────────────
    public void Pause()
    {
        if (!sessionActive || won || Router == null) return;
        Save();
        var request = new ModalRequest("잠깐 쉬어가요.", "시간도 잠시 멈춰둘게요.")
            .Dismiss(Router.Modals.Close);
        request.Button("계속하기", Router.Modals.Close);
        request.Button("다시 시작", ConfirmRestart);
        request.Button("메인으로", Router.ShowHome);
        Router.Modals.Show(request);
    }

    void ConfirmRestart()
    {
        var request = new ModalRequest("다시 시작할까요?", "이 판에 놓은 도형과 시간이 초기화돼요.")
            .Dismiss(Pause);
        request.Button("다시 시작", () =>
        {
            elapsed = 0f;
            hintsUsed = 0;
            won = false;
            if (board != null) { board.RestartPuzzle(); board.InputEnabled = true; }
            Router.Modals.Close();
            RefreshHeader();
            RefreshHintButton();
            BoardChanged();
        });
        request.Button("취소", Pause);
        Router.Modals.Show(request);
    }
}

public static class SamgakwonFormat
{
    public static string Time(float seconds)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{total / 60:00}:{total % 60:00}";
    }
}
