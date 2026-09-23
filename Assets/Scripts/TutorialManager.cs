// TutorialManager.cs
// 최초 게임 진입 시 1회, 기본 규칙(행/열/블록당 같은 도형 최대 2개)을 설명한다.
// 스킵 가능하며, 완료 여부를 PlayerPrefs 에 저장해 재노출을 막는다.
// 패널/텍스트/버튼은 인스펙터 연결(선택). 비어 있으면 아무 것도 하지 않는다.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("UI (선택)")]
    public GameObject panel;            // 튜토리얼 팝업 루트
    public TextMeshProUGUI bodyText;    // 규칙 설명 텍스트
    public Button confirmButton;        // 확인 버튼
    public Button skipButton;           // 스킵 버튼(없으면 confirm 만 사용)

    [Header("옵션")]
    [Tooltip("체크 시 매번 표시(개발용). 실제 빌드에선 해제.")]
    public bool alwaysShow = false;

    [TextArea]
    public string ruleText =
        "삼각원 규칙\n\n" +
        "· 각 행, 각 열, 각 3×3 블록 안에\n  같은 도형은 최대 2개까지!\n\n" +
        "빈칸을 눌러 아래 도형으로 채우세요.\n지우개로 지우고, 힌트로 도움을 받을 수 있어요.";

    private const string Key = "TutorialDone_v1";

    void Awake()
    {
        if (confirmButton != null) confirmButton.onClick.AddListener(Complete);
        if (skipButton != null) skipButton.onClick.AddListener(Complete);
        if (panel != null) panel.SetActive(false);
    }

    void Start()
    {
        if (alwaysShow || PlayerPrefs.GetInt(Key, 0) == 0)
            Show();
    }

    public void Show()
    {
        if (bodyText != null && !string.IsNullOrEmpty(ruleText))
            bodyText.text = ruleText;
        if (panel != null) panel.SetActive(true);
    }

    public void Complete()
    {
        if (panel != null) panel.SetActive(false);
        PlayerPrefs.SetInt(Key, 1);
        PlayerPrefs.Save();
    }

    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
