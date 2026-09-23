using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼을 누를 때 공통 클릭음을 낸다.
///
/// 빌더가 만드는 버튼에는 미리 붙여 두지만, 그것만으로는 부족하다.
/// 스테이지 격자나 다이얼로그 버튼처럼 **런타임에 만들어지는 버튼**도 있고,
/// 빌더를 다시 돌리지 않은 기존 씬도 있기 때문이다.
/// 그래서 라우터가 화면·모달을 띄울 때마다 <see cref="BindAll"/> 로 훑어 붙인다.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    void Awake()
    {
        var button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(SFXManager.PlayUIClick);
    }

    /// <summary>
    /// root 아래의 모든 버튼에 클릭음을 붙인다(꺼져 있는 것 포함).
    /// 이미 붙어 있으면 건너뛰므로 여러 번 불러도 소리가 겹치지 않는다.
    /// </summary>
    public static void BindAll(GameObject root)
    {
        if (root == null) return;
        foreach (var button in root.GetComponentsInChildren<Button>(true))
            if (button.GetComponent<ButtonClickSound>() == null)
                button.gameObject.AddComponent<ButtonClickSound>();
    }
}
