using UnityEditor;
using UnityEngine;

/// <summary>
/// 저장 상태를 되돌려 '처음 하는 사람' 경험을 다시 확인할 수 있게 한다.
/// 에디터의 PlayerPrefs 는 플레이를 멈춰도 남으므로, 한 번 튜토리얼을 보고 나면
/// 초기화하지 않는 한 다시 뜨지 않는다.
/// </summary>
public static class SamgakwonDevTools
{
    [MenuItem("Samgakwon/Reset/처음 하는 사람으로 되돌리기 (튜토리얼·챕터 안내)")]
    public static void ResetFirstPlay()
    {
        PlayerPrefs.DeleteKey("TutorialDone_v1");
        for (int chapter = 2; chapter <= 4; chapter++) PlayerPrefs.DeleteKey("ChapterIntro_" + chapter);
        PlayerPrefs.Save();
        Debug.Log("[삼각원] 튜토리얼과 챕터 안내를 초기화했습니다. 다음 판 시작 때 다시 뜹니다.");
    }

    [MenuItem("Samgakwon/Reset/진행과 기록까지 전부 초기화")]
    public static void ResetEverything()
    {
        if (!EditorUtility.DisplayDialog("삼각원",
            "해금 상태 · 클리어 기록 · 최고 기록 · 이어하기를 모두 지웁니다.\n되돌릴 수 없습니다.",
            "전부 초기화", "취소")) return;

        ResetFirstPlay();
        PlayerPrefs.DeleteKey("UnlockedStages");
        PlayerPrefs.DeleteKey("EndlessUnlocked");
        PlayerPrefs.DeleteKey("EndlessClearCount");
        PlayerPrefs.DeleteKey("SamgakwonResume_v1");
        for (int stage = 1; stage <= Chapters.TotalStages; stage++)
        {
            PlayerPrefs.DeleteKey($"Stage_{stage}_Cleared");
            PlayerPrefs.DeleteKey($"Record_Stage_{stage}");
        }
        foreach (GameDifficulty difficulty in System.Enum.GetValues(typeof(GameDifficulty)))
            PlayerPrefs.DeleteKey($"Record_Classic_{difficulty}");
        PlayerPrefs.DeleteKey("Record_Endless");
        PlayerPrefs.Save();
        Debug.Log("[삼각원] 모든 진행과 기록을 초기화했습니다.");
    }

    [MenuItem("Samgakwon/Reset/현재 저장 상태 보기")]
    public static void DumpState()
    {
        int cleared = 0;
        for (int stage = 1; stage <= Chapters.TotalStages; stage++)
            if (PlayerPrefs.GetInt($"Stage_{stage}_Cleared", 0) == 1) cleared++;

        Debug.Log($"[삼각원] 튜토리얼 완료: {PlayerPrefs.GetInt("TutorialDone_v1", 0) == 1}" +
                  $" · 해금 스테이지: {PlayerPrefs.GetInt("UnlockedStages", 1)}" +
                  $" · 클리어: {cleared}개" +
                  $" · 엔드리스 해금: {PlayerPrefs.GetInt("EndlessUnlocked", 0) == 1}" +
                  $" · 이어하기: {PlayerPrefs.HasKey("SamgakwonResume_v1")}");
    }
}
