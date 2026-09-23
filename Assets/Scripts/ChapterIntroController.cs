// ChapterIntroController.cs
// 스테이지 모드에서 챕터 전환(21/41/61 진입) 시 새 변주 규칙을 1회 안내한다.
// 챕터 1(스테이지 1)의 기본 규칙 안내는 TutorialManager 가 담당하므로 여기서는 2~4장만 다룬다.
// 패널/텍스트/닫기 버튼은 인스펙터에서 연결하며, 비어 있어도 안전하게 무시된다.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChapterIntroController : MonoBehaviour
{
    [Header("UI (선택)")]
    public GameObject panel;              // 안내 팝업 루트
    public TextMeshProUGUI titleText;     // "2장 · 집중"
    public TextMeshProUGUI bodyText;      // 변주 설명
    public Button closeButton;            // 확인/스킵 버튼

    private const string KeyPrefix = "ChapterIntro_"; // + chapter

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        if (panel != null)
            panel.SetActive(false);
    }

    // 해당 스테이지가 챕터(2~4)의 시작이면, 아직 안 봤을 때 1회 표시
    public void MaybeShowForStage(int stage)
    {
        if (!Chapters.IsChapterStart(stage)) return;

        int chapter = Chapters.GetChapter(stage);
        if (chapter <= 1) return; // 1장 기본 규칙은 튜토리얼이 담당

        string key = KeyPrefix + chapter;
        if (PlayerPrefs.GetInt(key, 0) == 1) return;

        Show(Chapters.GetInfo(chapter));
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public void Show(ChapterInfo info)
    {
        if (titleText != null) titleText.text = info.title;
        if (bodyText != null) bodyText.text = info.description;
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }

    // 저장된 챕터 안내 노출 기록 초기화(테스트/옵션용)
    public static void ResetSeen()
    {
        for (int c = 2; c <= 4; c++)
            PlayerPrefs.DeleteKey(KeyPrefix + c);
        PlayerPrefs.Save();
    }
}
