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

    void SetDifficultyAndStart(GameDifficulty difficulty)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDifficulty(difficulty);
        }
        
        SceneManager.LoadScene("InGame"); // 인게임 씬 이름에 맞게 수정
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("Start"); // 타이틀 씬 이름에 맞게 수정
    }
}