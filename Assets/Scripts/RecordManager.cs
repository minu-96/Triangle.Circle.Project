using UnityEngine;

public class RecordManager : MonoBehaviour
{
    public static RecordManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 기록 저장
    public void SaveRecord(float time)
    {
        if (GameManager.Instance == null) return;

        string key = GetRecordKey();
        float currentBest = GetBestTime();

        // 기록이 없거나, 새 기록이 더 빠르면 저장
        if (currentBest == 0f || time < currentBest)
        {
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();
            Debug.Log($"New Record! {key}: {time:F2}");
        }
    }

    // 최고 기록 가져오기
    public float GetBestTime()
    {
        if (GameManager.Instance == null) return 0f;

        string key = GetRecordKey();
        return PlayerPrefs.GetFloat(key, 0f);
    }

    // 최고 기록을 포맷된 문자열로 반환
    public string GetBestTimeFormatted()
    {
        float bestTime = GetBestTime();
        
        if (bestTime == 0f)
        {
            return "No Record";
        }

        int minutes = (int)(bestTime / 60);
        int seconds = (int)(bestTime % 60);
        int milliseconds = (int)((bestTime - Mathf.Floor(bestTime)) * 100);

        return string.Format("{0:D2}:{1:D2}:{2:D2}", minutes, seconds, milliseconds);
    }

    // 현재 모드/난이도/스테이지에 맞는 키 생성
    string GetRecordKey()
    {
        if (GameManager.Instance.currentMode == GameMode.Stage)
        {
            return $"Record_Stage_{GameManager.Instance.currentStage}";
        }
        else
        {
            return $"Record_Classic_{GameManager.Instance.currentDifficulty}";
        }
    }

    // 특정 난이도의 기록 가져오기
    public float GetClassicRecord(GameDifficulty difficulty)
    {
        string key = $"Record_Classic_{difficulty}";
        return PlayerPrefs.GetFloat(key, 0f);
    }

    // 특정 스테이지의 기록 가져오기
    public float GetStageRecord(int stageNumber)
    {
        string key = $"Record_Stage_{stageNumber}";
        return PlayerPrefs.GetFloat(key, 0f);
    }

    // 모든 기록 삭제 (테스트용)
    public void ClearAllRecords()
    {
        // 클래식 모드 기록 삭제
        foreach (GameDifficulty difficulty in System.Enum.GetValues(typeof(GameDifficulty)))
        {
            string key = $"Record_Classic_{difficulty}";
            PlayerPrefs.DeleteKey(key);
        }

        // 스테이지 기록 삭제
        for (int i = 1; i <= 81; i++)
        {
            string key = $"Record_Stage_{i}";
            PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.Save();
        Debug.Log("모든 기록이 삭제되었습니다.");
    }

    // 신기록인지 확인
    public bool IsNewRecord(float time)
    {
        float currentBest = GetBestTime();
        return currentBest == 0f || time < currentBest;
    }
}