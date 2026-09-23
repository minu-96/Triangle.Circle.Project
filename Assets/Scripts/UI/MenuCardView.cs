using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>메인 화면의 큰 메뉴 카드(클래식 / 스테이지 / 엔드리스).</summary>
public class MenuCardView : MonoBehaviour
{
    public Button button;
    public Image background;
    public Image icon;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public GameObject arrow;

    Action callback;

    void Awake() { if (button != null) button.onClick.AddListener(() => callback?.Invoke()); }

    public void Set(string title, string description, Sprite iconSprite, bool enabled, Action action)
    {
        callback = action;
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        if (icon != null) icon.sprite = iconSprite;
        if (button != null) button.interactable = enabled;
        if (arrow != null) arrow.SetActive(enabled);
        var group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = enabled ? 1f : 0.55f;
    }
}
