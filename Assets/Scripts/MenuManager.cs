using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void StartEasyMode()
    {
        SetDifficultyAndStart(GameDifficulty.Easy);
    }

    public void StartNormalMode()
    {
        SetDifficultyAndStart(GameDifficulty.Normal);
    }

    public void StartHardMode()
    {
        SetDifficultyAndStart(GameDifficulty.Hard);
    }
    
    public void GoToStageSelect()
    {
        SceneManager.LoadScene("Stage"); // 스테이지 선택 씬 이름
    }

    // 엔드리스 보너스 모드 시작 (스테이지 81 클리어로 해금된 경우에만)
    public void StartEndless()
    {
        if (!GameManager.IsEndlessUnlocked())
        {
            Debug.Log("엔드리스 모드는 스테이지 81 클리어 후 해금됩니다.");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetEndless();
        }
        SceneManager.LoadScene("InGame");
    }

    void SetDifficultyAndStart(GameDifficulty difficulty)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDifficulty(difficulty);
            
            // 확실하게 클래식 모드로 설정
            GameManager.Instance.currentMode = GameMode.Classic;
            
            Debug.Log($"메뉴에서 설정: 모드={GameManager.Instance.currentMode}, 난이도={GameManager.Instance.currentDifficulty}");
        }
        
        SceneManager.LoadScene("InGame"); // 인게임 씬 이름에 맞게 수정
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("Start"); // 타이틀 씬 이름에 맞게 수정
    }
}