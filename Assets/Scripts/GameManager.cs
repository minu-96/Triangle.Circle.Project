using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public GameDifficulty currentDifficulty = GameDifficulty.Easy;
    public GameMode currentMode = GameMode.Classic;
    public int currentStage = 1; // 스테이지 모드용 (1~81)

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.None; // 에러 방지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 자동 생성 (RuntimeInitializeOnLoadMethod 사용)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
    }

    public void SetDifficulty(GameDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        Debug.Log($"난이도 설정: {difficulty}");
    }

    public void SetStage(int stageNumber)
{
    currentStage = Mathf.Clamp(stageNumber, 1, 81);
    currentMode = GameMode.Stage;
    Debug.Log($"스테이지 설정: {currentStage}");
}

    // 엔드리스 보너스 모드 진입 (스테이지 81 클리어 시 해금)
    public void SetEndless()
    {
        currentMode = GameMode.Endless;
        Debug.Log("엔드리스 모드 진입");
    }

    // ── 엔드리스 해금/기록 (PlayerPrefs) ─────────────────────────
    private const string KEY_ENDLESS_UNLOCKED = "EndlessUnlocked";
    private const string KEY_ENDLESS_COUNT = "EndlessClearCount";

    public static bool IsEndlessUnlocked()
    {
        return PlayerPrefs.GetInt(KEY_ENDLESS_UNLOCKED, 0) == 1;
    }

    public static void UnlockEndless()
    {
        if (PlayerPrefs.GetInt(KEY_ENDLESS_UNLOCKED, 0) != 1)
        {
            PlayerPrefs.SetInt(KEY_ENDLESS_UNLOCKED, 1);
            PlayerPrefs.Save();
            Debug.Log("엔드리스 모드 해금!");
        }
    }

    public static int GetEndlessClearCount()
    {
        return PlayerPrefs.GetInt(KEY_ENDLESS_COUNT, 0);
    }

    // 엔드리스에서 한 판 클리어할 때마다 누적 카운터 증가 후 새 카운트 반환
    public static int IncrementEndlessClearCount()
    {
        int next = GetEndlessClearCount() + 1;
        PlayerPrefs.SetInt(KEY_ENDLESS_COUNT, next);
        PlayerPrefs.Save();
        return next;
    }

    // Inspector에서 값 조절 가능하도록 변경
    [Header("Difficulty Balance")]
    [Tooltip("Easy 모드 빈칸 개수")]
    public int easyEmptyCells = 15;
    
    [Tooltip("Normal 모드 빈칸 개수")]
    public int normalEmptyCells = 35;
    
    [Tooltip("Hard 모드 빈칸 개수")]
    public int hardEmptyCells = 55;

    public int GetEmptyCellCount()
    {
        return currentDifficulty switch
        {
            GameDifficulty.Easy => easyEmptyCells,
            GameDifficulty.Normal => normalEmptyCells,
            GameDifficulty.Hard => hardEmptyCells,
            _ => normalEmptyCells
        };
    }
}

public enum GameDifficulty
{
    Easy,    // 빈칸 15
    Normal,  // 빈칸 35
    Hard     // 빈칸 55
}

public enum GameMode
{
    Classic,
    Stage,
    Endless
}

public enum ShapeType
{
    None = 0,
    Triangle = 1,  // △
    Circle = 2,    // ○
    Square = 3,    // □
    Pentagon = 4,  // ⬟
    Star = 5       // ★
}