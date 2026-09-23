using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>완성 시 떨어지는 색종이. 프리팹 한 장을 필요한 수만큼 인스턴스화한다.</summary>
public class ConfettiView : MonoBehaviour
{
    public RectTransform root;
    public Image piecePrefab;
    [Min(1)] public int count = 28;
    public float duration = 1.25f;
    public float fallDistance = 420f;

    public void Play(SamgakwonTheme theme)
    {
        if (root == null || piecePrefab == null) return;
        StartCoroutine(Run(theme));
    }

    IEnumerator Run(SamgakwonTheme theme)
    {
        var pieces = new List<RectTransform>(count);
        float width = root.rect.width;
        for (int i = 0; i < count; i++)
        {
            var piece = Instantiate(piecePrefab, root);
            piece.gameObject.SetActive(true);
            if (theme != null)
            {
                var set = theme.confetti;
                piece.sprite = set != null && set.Length == 5 && set[i % 5] != null
                    ? set[i % 5] : theme.Token((ShapeType)(i % 5 + 1));
            }
            var rect = piece.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(Random.Range(-width * 0.42f, width * 0.42f), Random.Range(-40f, -220f));
            pieces.Add(rect);
        }

        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float step = Time.unscaledDeltaTime;
            for (int i = 0; i < pieces.Count; i++)
            {
                var rect = pieces[i];
                if (rect == null) continue;
                rect.anchoredPosition += new Vector2(Mathf.Sin(time * 5f + i) * 40f, -fallDistance) * step;
                rect.Rotate(0f, 0f, 200f * step);
                var image = rect.GetComponent<Image>();
                if (image != null)
                {
                    var color = image.color;
                    color.a = Mathf.Clamp01(1f - time / duration);
                    image.color = color;
                }
            }
            yield return null;
        }
        foreach (var rect in pieces) if (rect != null) Destroy(rect.gameObject);
    }
}
