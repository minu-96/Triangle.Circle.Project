using UnityEngine;

/// <summary>
/// 화면 하나. 라우터가 활성/비활성으로 전환한다(HTML처럼 매번 파괴·재생성하지 않는다).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public abstract class UIScreen : MonoBehaviour
{
    protected SamgakwonRouter Router { get; private set; }

    public virtual void Bind(SamgakwonRouter router) => Router = router;

    /// <summary>화면이 보이기 직전마다 호출된다. 표시 내용을 여기서 갱신한다.</summary>
    public virtual void OnShow() { }

    /// <summary>화면이 가려지기 직전에 호출된다.</summary>
    public virtual void OnHide() { }

    public void SetVisible(bool visible)
    {
        if (visible == gameObject.activeSelf)
        {
            if (visible) OnShow();
            return;
        }
        if (!visible) OnHide();
        gameObject.SetActive(visible);
        if (visible) OnShow();
    }
}
