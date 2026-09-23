using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>모달 한 장. 버튼은 프리팹으로 필요한 수만큼 생성한다.</summary>
public class DialogView : MonoBehaviour
{
    public Image panel;
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public TMP_Text metricText;
    public RectTransform actionsRoot;
    public LabeledButton actionPrefab;
    [Header("튜토리얼 설명 그림 (선택)")]
    public Transform sampleRow;          // 행 · 열 · 블록 예시 줄
    public Image[] sampleImages;         // 3장: 가로줄 / 세로줄 / 블록
    public Image invalidImage;           // 세 번째는 놓을 수 없다는 예시

    readonly List<LabeledButton> spawned = new List<LabeledButton>();

    public void Render(SamgakwonTheme theme, ModalRequest request)
    {
        if (panel != null && theme != null)
        {
            panel.sprite = theme.panelPopup;
            panel.color = request.highlight ? new Color32(0xFF, 0xF5, 0xCF, 0xFF) : theme.paper;
        }
        if (iconImage != null)
        {
            iconImage.sprite = request.icon;
            iconImage.gameObject.SetActive(request.icon != null);
        }
        if (titleText != null) { titleText.text = request.title; if (theme != null) titleText.color = theme.ink; }
        if (metricText != null)
        {
            bool has = !string.IsNullOrEmpty(request.metric);
            metricText.gameObject.SetActive(has);
            if (has) { metricText.text = request.metric; if (theme != null) metricText.color = theme.ink; }
        }
        if (bodyText != null)
        {
            bool has = !string.IsNullOrEmpty(request.body);
            bodyText.gameObject.SetActive(has);
            if (has) { bodyText.text = request.body; if (theme != null) bodyText.color = theme.muted; }
        }
        if (sampleRow != null) sampleRow.gameObject.SetActive(request.showSamples);
        if (invalidImage != null)
            invalidImage.gameObject.SetActive(request.showInvalid && theme != null && theme.tutorialInvalid != null);
        if ((request.showSamples || request.showInvalid) && theme != null)
        {
            // 팩에 들어 있는 설명 그림을 그대로 쓴다 (행 / 열 / 블록 + 세 번째는 불가).
            var set = new[] { theme.tutorialRow, theme.tutorialColumn, theme.tutorialBlock };
            if (sampleImages != null)
                for (int i = 0; i < sampleImages.Length && i < set.Length; i++)
                    if (sampleImages[i] != null)
                    {
                        sampleImages[i].sprite = set[i] != null ? set[i] : theme.Token(ShapeType.Circle);
                        sampleImages[i].color = Color.white;
                        sampleImages[i].preserveAspect = true;
                    }
            if (invalidImage != null && request.showInvalid)
            {
                invalidImage.sprite = theme.tutorialInvalid;
                invalidImage.preserveAspect = true;
            }
        }

        foreach (var item in spawned) if (item != null) Destroy(item.gameObject);
        spawned.Clear();
        if (actionPrefab == null || actionsRoot == null || request.buttons == null) return;
        for (int i = 0; i < request.buttons.Count; i++)
        {
            var spec = request.buttons[i];
            var instance = Instantiate(actionPrefab, actionsRoot);
            instance.gameObject.SetActive(true);
            instance.Set(spec.label, spec.action, i == 0, theme);
            spawned.Add(instance);
        }
    }
}

public struct ModalButtonSpec
{
    public string label;
    public Action action;
    public ModalButtonSpec(string label, Action action) { this.label = label; this.action = action; }
}

public class ModalRequest
{
    public string title;
    public string body;
    public string metric;
    public Sprite icon;
    public bool highlight;
    public bool showSamples;   // 행/열/블록 예시 3장
    public bool showInvalid;   // '세 번째는 불가' 그림
    public Action onDismiss;
    public List<ModalButtonSpec> buttons = new List<ModalButtonSpec>();

    public ModalRequest(string title, string body = null)
    {
        this.title = title;
        this.body = body;
    }
    public ModalRequest Button(string label, Action action) { buttons.Add(new ModalButtonSpec(label, action)); return this; }
    public ModalRequest Metric(string value) { metric = value; return this; }
    public ModalRequest Icon(Sprite value) { icon = value; return this; }
    public ModalRequest Highlight(bool value = true) { highlight = value; return this; }
    public ModalRequest Samples(bool value = true) { showSamples = value; return this; }
    public ModalRequest Invalid(bool value = true) { showInvalid = value; return this; }
    public ModalRequest Dismiss(Action value) { onDismiss = value; return this; }
}
