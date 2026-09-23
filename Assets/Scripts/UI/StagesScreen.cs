using TMPro;
using UnityEngine;

/// <summary>81개 스테이지를 챕터 네 구획으로 보여 준다.</summary>
public class StagesScreen : UIScreen
{
    public TMP_Text eyebrowText;
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public ChapterSectionView[] sections = new ChapterSectionView[4];

    public override void Bind(SamgakwonRouter router)
    {
        base.Bind(router);
        var theme = Router.Theme;
        if (eyebrowText != null) { eyebrowText.text = "A JOURNEY OF 81 PUZZLES"; if (theme != null) eyebrowText.color = theme.muted; }
        if (titleText != null) { titleText.text = "한 칸씩, 차근차근."; if (theme != null) titleText.color = theme.ink; }
    }

    public override void OnShow()
    {
        var theme = Router != null ? Router.Theme : null;
        int unlocked = Mathf.Clamp(PlayerPrefs.GetInt("UnlockedStages", 1), 1, Chapters.TotalStages);

        int cleared = 0;
        for (int stage = 1; stage <= Chapters.TotalStages; stage++)
            if (StageSelectManager.IsStageClear(stage)) cleared++;
        if (subtitleText != null)
        {
            subtitleText.text = $"{cleared}개 완성 · 다음 퍼즐이 기다리고 있어요.";
            if (theme != null) subtitleText.color = theme.muted;
        }

        if (sections == null) return;
        foreach (var section in sections)
            if (section != null)
                section.Refresh(theme, unlocked, stage => Router.StartGame(GameMode.Stage, stage, GameDifficulty.Normal));
    }
}
