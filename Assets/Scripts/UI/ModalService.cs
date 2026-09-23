using UnityEngine;

/// <summary>모달 레이어. 스크림 + 다이얼로그 한 장을 재사용한다.</summary>
public class ModalService : MonoBehaviour
{
    public GameObject root;
    public DialogView dialog;
    [SerializeField] SamgakwonTheme theme;

    ModalRequest current;

    public bool IsOpen => root != null && root.activeSelf;

    void Awake()
    {
        // root 가 이 컴포넌트가 붙은 오브젝트 자신이면 여기서 끄면 안 된다.
        // Show() 가 root 를 켜는 순간 Awake 가 호출되고, 그 Awake 가 다시 꺼 버려
        // 팝업이 영영 열리지 않는다.
        if (root != null && root != gameObject) root.SetActive(false);
    }

    public void SetTheme(SamgakwonTheme value) => theme = value;

    public void Show(ModalRequest request)
    {
        current = request;
        if (dialog != null) dialog.Render(theme, request);
        if (root != null)
        {
            root.SetActive(true);
            // 액션 버튼은 요청마다 새로 만들어지므로 여기서 클릭음을 붙인다.
            ButtonClickSound.BindAll(root);
        }
    }

    public void Close()
    {
        current = null;
        if (root != null) root.SetActive(false);
    }

    /// <summary>
    /// Esc. 요청이 취소 동작을 지정했을 때만 닫는다.
    /// 완료 모달처럼 취소 동작이 없는 모달은 무시해야 한다
    /// (그냥 닫으면 다 푼 보드에 입력이 막힌 채로 갇힌다).
    /// </summary>
    public void Dismiss()
    {
        if (!IsOpen) return;
        current?.onDismiss?.Invoke();
    }
}
