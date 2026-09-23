using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>스테이지 화면의 챕터 한 구획(제목 · 범위 · 한 줄 설명 · 5열 격자).</summary>
public class ChapterSectionView : MonoBehaviour
{
    [Min(1)] public int chapter = 1;
    public Image icon;
    public TMP_Text titleText;
    public TMP_Text rangeText;
    public TMP_Text taglineText;
    public RectTransform gridRoot;
    public StageButtonView stageButtonPrefab;

    readonly List<StageButtonView> buttons = new List<StageButtonView>();

    public void Refresh(SamgakwonTheme theme, int unlocked, Action<int> onSelect)
    {
        var info = Chapters.GetInfo(chapter);
        if (icon != null)
        {
            icon.sprite = theme != null ? theme.ChapterIcon(chapter) : null;
            icon.gameObject.SetActive(icon.sprite != null);
        }
        if (titleText != null) { titleText.text = info.title; if (theme != null) titleText.color = theme.ink; }
        if (rangeText != null) { rangeText.text = $"{info.startStage}–{info.endStage}"; if (theme != null) rangeText.color = theme.muted; }
        if (taglineText != null) { taglineText.text = info.tagline; if (theme != null) taglineText.color = theme.muted; }
        if (gridRoot == null || stageButtonPrefab == null) return;

        int count = info.endStage - info.startStage + 1;
        while (buttons.Count < count)
        {
            var instance = Instantiate(stageButtonPrefab, gridRoot);
            instance.gameObject.SetActive(true);
            buttons.Add(instance);
        }
        for (int i = 0; i < buttons.Count; i++)
        {
            bool used = i < count;
            buttons[i].gameObject.SetActive(used);
            if (!used) continue;
            int stage = info.startStage + i;
            buttons[i].Set(stage, stage <= unlocked, StageSelectManager.IsStageClear(stage),
                stage == unlocked && !StageSelectManager.IsStageClear(stage), theme, () => onSelect?.Invoke(stage));
        }
    }
}
