using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>스테이지 격자의 칸 하나. 잠김 / 진행 중 / 완료 세 상태를 표현한다.</summary>
public class StageButtonView : MonoBehaviour
{
    public Button button;
    public Image background;
    public Image badge;
    public TMP_Text numberText;
    public TMP_Text recordText;

    Action callback;

    void Awake() { if (button != null) button.onClick.AddListener(() => callback?.Invoke()); }

    public void Set(int stage, bool unlocked, bool cleared, bool current, SamgakwonTheme theme, Action action)
    {
        callback = action;
        if (numberText != null)
        {
            numberText.text = stage.ToString();
            if (theme != null) numberText.color = current ? theme.stageCurrentInk : theme.ink;
        }
        if (background != null && theme != null)
        {
            theme.stage.Apply(button, background);
            background.color = cleared ? theme.stageDone : current ? theme.stageCurrent : theme.stageLocked;
        }
        if (button != null) button.interactable = unlocked;
        if (recordText != null)
        {
            float best = cleared && RecordManager.Instance != null ? RecordManager.Instance.GetStageRecord(stage) : 0f;
            bool show = best > 0f;
            recordText.gameObject.SetActive(show);
            if (show)
            {
                recordText.text = SamgakwonFormat.Time(best);
                if (theme != null) recordText.color = current ? theme.stageCurrentInk : theme.muted;
            }
        }
        if (badge != null && theme != null)
        {
            // 전용 배지 에셋을 우선 쓰고, 없으면 아이콘으로 대체한다.
            Sprite sprite = cleared ? (theme.badgeComplete != null ? theme.badgeComplete : theme.iconCheck)
                          : !unlocked ? (theme.badgeLocked != null ? theme.badgeLocked : theme.iconLock)
                          : current ? theme.badgeCurrent : null;
            badge.sprite = sprite;
            badge.gameObject.SetActive(sprite != null);
        }
    }
}
