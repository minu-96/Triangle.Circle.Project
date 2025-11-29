using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public GameDifficulty currentDifficulty = GameDifficulty.Easy;
    public GameMode currentMode = GameMode.Classic;

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
    Easy,    // 60칸 채워짐 (21칸 빈칸)
    Normal,  // 40칸 채워짐 (41칸 빈칸)
    Hard     // 20칸 채워짐 (61칸 빈칸)
}

public enum GameMode
{
    Classic,
    Stage
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