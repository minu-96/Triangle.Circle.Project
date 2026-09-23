using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>화면 아래쪽에 잠깐 뜨는 안내 문구.</summary>
public class ToastView : MonoBehaviour
{
    public CanvasGroup group;
    public TMP_Text label;
    public Image background;
    public float duration = 2.4f;
    public float fade = 0.18f;

    Coroutine routine;

    void Awake()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null) { group.alpha = 0; group.blocksRaycasts = false; group.interactable = false; }
    }

    public void Show(string message)
    {
        if (label != null) label.text = message;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return Fade(1f);
        yield return new WaitForSecondsRealtime(duration);
        yield return Fade(0f);
        routine = null;
    }

    IEnumerator Fade(float target)
    {
        if (group == null) yield break;
        float start = group.alpha, time = 0f;
        while (time < fade)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, time / fade);
            yield return null;
        }
        group.alpha = target;
    }
}
