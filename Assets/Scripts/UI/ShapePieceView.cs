using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>하단 도형 선택 칩 하나.</summary>
public class ShapePieceView : MonoBehaviour
{
    public Button button;
    public Image background;
    public Image tokenImage;
    public TMP_Text numberText;
    public Outline selectionOutline;

    Action callback;

    void Awake() { if (button != null) button.onClick.AddListener(() => callback?.Invoke()); }

    public void Set(int number, Sprite token, SamgakwonTheme theme, Action action)
    {
        callback = action;
        if (numberText != null) { numberText.text = number.ToString(); if (theme != null) numberText.color = theme.muted; }
        if (tokenImage != null) tokenImage.sprite = token;
        if (background != null && theme != null) theme.selector.Apply(button, background);
    }

    public void SetSelected(bool selected, SamgakwonTheme theme)
    {
        Sprite target = null;
        if (theme != null) target = selected ? theme.selector.selected : theme.selector.normal;
        if (background != null)
        {
            if (target != null) { background.sprite = target; background.color = Color.white; }
            else if (theme != null) background.color = selected ? theme.cellSelected : Color.white;
        }
        if (selectionOutline != null)
        {
            // 전용 selected 스프라이트가 있으면 테두리가 이미 들어 있으므로 겹쳐 그리지 않는다.
            selectionOutline.enabled = selected && target == null;
            if (theme != null) selectionOutline.effectColor = theme.teal;
        }
    }
}
