using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 챕터가 바뀌는 스테이지(21 · 41 · 61)에 들어설 때 잠깐 지나가는 전환 연출.
/// 기획서의 "간단한 전환 연출" 항목으로, 1회만 뜨는 안내 팝업과 달리
/// 챕터 경계에 진입할 때마다 재생한다.
///
/// 오브젝트는 항상 켜 두고 CanvasGroup 의 알파로만 보였다 사라진다.
/// (꺼 두면 Play 에서 코루틴을 시작할 수 없다.)
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ChapterTransitionView : MonoBehaviour
{
    public CanvasGroup group;
    public Image background;
    public Image icon;
    public TMP_Text titleText;
    public TMP_Text taglineText;

    [Header("타이밍(초)")]
    public float fadeIn = 0.28f;
    public float hold = 0.95f;
    public float fadeOut = 0.38f;

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        Hide();
    }

    void Hide()
    {
        if (group == null) return;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    public void Play(ChapterInfo info, SamgakwonTheme theme, Action onDone)
    {
        // 모션을 끈 플레이어는 연출을 건너뛴다.
        if (group == null || PlayerPrefs.GetInt("SamgakwonMotion", 1) == 0 || !isActiveAndEnabled)
        {
            onDone?.Invoke();
            return;
        }
        if (icon != null)
        {
            icon.sprite = theme != null ? theme.ChapterIcon(info.chapter) : null;
            icon.gameObject.SetActive(icon.sprite != null);
        }
        if (titleText != null) { titleText.text = info.title; if (theme != null) titleText.color = theme.ink; }
        if (taglineText != null) { taglineText.text = info.tagline; if (theme != null) taglineText.color = theme.muted; }
        if (background != null && theme != null) background.color = theme.mint;
        StartCoroutine(Run(onDone));
    }

    IEnumerator Run(Action onDone)
    {
        IsPlaying = true;
        group.blocksRaycasts = true;   // 연출 중에는 보드 입력을 막는다

        yield return Fade(0f, 1f, fadeIn);
        yield return new WaitForSecondsRealtime(hold);
        yield return Fade(1f, 0f, fadeOut);

        Hide();
        IsPlaying = false;
        onDone?.Invoke();
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f) { group.alpha = to; yield break; }
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }
        group.alpha = to;
    }
}
