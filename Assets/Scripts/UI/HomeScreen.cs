using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>메인 화면: 타이틀 + 이어하기 + 세 가지 모드 카드.</summary>
public class HomeScreen : UIScreen
{
    [Header("Hero")]
    public Image[] heroMarks = new Image[3];
    public TMP_Text eyebrowText;
    public TMP_Text titleText;
    public TMP_Text subtitleText;

    [Header("Menu")]
    public LabeledButton resumeButton;
    public MenuCardView classicCard;
    public MenuCardView stagesCard;
    public MenuCardView endlessCard;
    public TMP_Text footnoteText;

    public override void Bind(SamgakwonRouter router)
    {
        base.Bind(router);
        var theme = Router.Theme;
        if (theme != null && heroMarks != null)
            for (int i = 0; i < heroMarks.Length; i++)
                if (heroMarks[i] != null) heroMarks[i].sprite = theme.Token((ShapeType)(i + 1));

        if (eyebrowText != null) { eyebrowText.text = "A LITTLE SHAPE OF JOY"; if (theme != null) eyebrowText.color = theme.muted; }
        if (titleText != null) { titleText.text = "삼각원"; if (theme != null) titleText.color = theme.ink; }
        if (subtitleText != null)
        {
            subtitleText.text = "같은 도형은 두 개까지.\n작은 퍼즐로 가볍게 쉬어가요.";
            if (theme != null) subtitleText.color = theme.muted;
        }
        if (footnoteText != null)
        {
            footnoteText.text = "오프라인 플레이 · 기록은 이 기기에 저장돼요";
            if (theme != null) footnoteText.color = theme.muted;
        }
    }

    public override void OnShow()
    {
        var theme = Router != null ? Router.Theme : null;
        var saved = PuzzleSave.Load();

        if (resumeButton != null)
        {
            bool has = saved != null;
            resumeButton.gameObject.SetActive(has);
            if (has)
            {
                string where = saved.mode == GameMode.Stage
                    ? $"스테이지 {saved.stage}"
                    : $"클래식 · {GameScreen.Difficulty(saved.difficulty)}";
                resumeButton.Set($"이어하기 · {where}", () => Router.Resume(), true, theme);
            }
        }

        int cleared = 0;
        for (int stage = 1; stage <= Chapters.TotalStages; stage++)
            if (StageSelectManager.IsStageClear(stage)) cleared++;

        classicCard?.Set("클래식", "내 속도에 맞는 한 판",
            theme != null ? theme.iconPlay : null, true, () => Router.ShowClassicPicker());

        stagesCard?.Set("스테이지", $"{cleared} / {Chapters.TotalStages}개의 퍼즐 완성",
            theme != null ? theme.iconSparkle : null, true, () => Router.ShowStages());

        bool unlocked = GameManager.IsEndlessUnlocked() || StageSelectManager.IsStageClear(Chapters.TotalStages);
        endlessCard?.Set("엔드리스",
            unlocked ? $"누적 {GameManager.GetEndlessClearCount()}판 완성" : "81단계 클리어 후 해금",
            theme != null ? (unlocked ? theme.iconInfinity : theme.iconLock) : null,
            unlocked, () => Router.StartGame(GameMode.Endless, 1, GameDifficulty.Hard));
    }
}
