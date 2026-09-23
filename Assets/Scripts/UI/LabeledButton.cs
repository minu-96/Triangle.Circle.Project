using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>텍스트(+선택적 아이콘)를 가진 버튼. 다이얼로그 액션과 도구 버튼이 함께 쓴다.</summary>
public class LabeledButton : MonoBehaviour
{
    public Button button;
    public Image background;
    public Image icon;
    public TMP_Text label;

    Action callback;

    void Awake()
    {
        if (button != null) button.onClick.AddListener(Invoke);
    }

    void Invoke() => callback?.Invoke();

    public void Set(string text, Action action, bool primary, SamgakwonTheme theme, Sprite iconSprite = null)
    {
        callback = action;
        if (label != null)
        {
            label.text = text;
            if (theme != null) label.color = primary ? Color.white : theme.ink;
        }
        if (background != null && theme != null)
        {
            // 상태별 스프라이트(hover/pressed/disabled)를 그대로 쓴다.
            var set = primary ? theme.primary : theme.secondary;
            set.Apply(button, background);
        }
        if (icon != null)
        {
            icon.sprite = iconSprite;
            icon.gameObject.SetActive(iconSprite != null);
        }
        if (button != null) button.interactable = action != null;
    }

    public void SetInteractable(bool value) { if (button != null) button.interactable = value; }

    /// <summary>메모 모드처럼 켜짐/꺼짐이 있는 버튼의 강조 표시.</summary>
    public void SetHighlighted(bool on, SamgakwonTheme theme)
    {
        if (theme == null) return;
        if (background != null) background.color = on ? theme.cellSelected : Color.white;
        if (label != null) label.color = on ? theme.teal : theme.ink;
    }
}
