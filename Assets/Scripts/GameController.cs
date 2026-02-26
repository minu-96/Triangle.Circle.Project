using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UIElements;

public class GameController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI bestTimeText; // 최고 기록 표시
    public GameObject clearPanel;
    public TextMeshProUGUI clearTimeText;
    public TextMeshProUGUI newRecordText; // "신기록!" 텍스트 (선택사항)
    
    [Header("Memo Mode UI")]
    public UnityEngine.UI.Button memoButton;
    public Color memoActiveColor = new Color(0.3f, 0.7f, 1f);
    public Color memoInactiveColor = Color.white;
    
    [Header("Managers")]
    public BoardManager boardManager;

    private float elapsedTime = 0f;
    private bool isGameActive = true;

    [Header("UIPanel")]
    public GameObject uiPanel;
    

    void Start()
    {
        // 난이도/스테이지 표시
        if (difficultyText != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.currentMode == GameMode.Stage)
            {
                difficultyText.text = $"Stage {GameManager.Instance.currentStage}";
            }
            else
            {
                difficultyText.text = $"{GameManager.Instance.currentDifficulty}";
            }
        }

        // 최고 기록 표시
        UpdateBestTimeDisplay();

        // 클리어 패널 숨기기
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
        uiPanel.SetActive(false);
    }

    void UpdateBestTimeDisplay()
    {
        if (bestTimeText != null && RecordManager.Instance != null)
        {
            string bestTime = RecordManager.Instance.GetBestTimeFormatted();
            bestTimeText.text = $"Record: {bestTime}";
        }
    }

    void Update()
    {
        if (isGameActive)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
        
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
            int milliseconds = (int)((elapsedTime - Math.Floor(elapsedTime)) * 100);
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", 
                time.Minutes, time.Seconds, milliseconds);
        }
    }

    public void OnPuzzleComplete()
    {
        isGameActive = false;
        
        // 기록 저장
        bool isNewRecord = false;
        if (RecordManager.Instance != null)
        {
            isNewRecord = RecordManager.Instance.IsNewRecord(elapsedTime);
            RecordManager.Instance.SaveRecord(elapsedTime);
        }
        
        // 스테이지 모드면 해당 스테이지의 다음 스테이지만 해금
        if (GameManager.Instance != null && GameManager.Instance.currentMode == GameMode.Stage)
        {
            StageSelectManager.UnlockStage(GameManager.Instance.currentStage);
        }
        
        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
            
            if (clearTimeText != null)
            {
                TimeSpan time = TimeSpan.FromSeconds(elapsedTime);
                int milliseconds = (int)((elapsedTime - Math.Floor(elapsedTime)) * 100);
                clearTimeText.text = string.Format("Time: {0:D2}:{1:D2}:{2:D2}", 
                    time.Minutes, time.Seconds, milliseconds);
            }
            
            // 신기록 표시
            if (newRecordText != null)
            {
                newRecordText.gameObject.SetActive(isNewRecord);
                if (isNewRecord)
                {
                    newRecordText.text = "New Record!!!";
                    newRecordText.color = new Color(1f, 0.8f, 0f); // 금색
                }
            }
        }

        Debug.Log($"게임 클리어! 시간: {elapsedTime:F2}초 {(isNewRecord ? "(신기록!)" : "")}");
    }

    public void ResetGame()
    {
        elapsedTime = 0f;
        isGameActive = true;
        
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
        
        if (boardManager != null)
        {
            boardManager.ResetBoard();
        }
        
        // 최고 기록 다시 표시
        UpdateBestTimeDisplay();
    }

    public void BackToMenu()
    {
        // 스테이지 모드면 스테이지 선택으로, 아니면 메뉴로
        if (GameManager.Instance != null && GameManager.Instance.currentMode == GameMode.Stage)
        {
            SceneManager.LoadScene("Stage");
        }
        else
        {
            SceneManager.LoadScene("Mode");
        }
    }
    
    public void NextStage()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentMode == GameMode.Stage)
        {
            int nextStage = GameManager.Instance.currentStage + 1;
            if (nextStage <= 81)
            {
                GameManager.Instance.SetStage(nextStage);
                ResetGame();
            }
            else
            {
                Debug.Log("모든 스테이지 클리어!");
                BackToMenu();
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // 도형 선택 버튼에 연결할 메서드
    public void OnShapeButtonClick(int shapeIndex)
    {
        if (boardManager != null)
        {
            boardManager.SelectShape(shapeIndex);
            UpdateMemoButtonVisual(); // 메모 상태 업데이트
        }
    }
    
    // 지우개 버튼에 연결할 메서드
    public void OnEraseButtonClick()
    {
        if (boardManager != null)
        {
            boardManager.ClearCell();
        }
    }

    // 메모 모드 토글
    public void OnMemoButtonClick()
    {
        if (boardManager != null)
        {
            boardManager.ToggleMemoMode();
            UpdateMemoButtonVisual();
        }
    }
    
    void UpdateMemoButtonVisual()
    {
        if (memoButton != null && boardManager != null)
        {
            var colors = memoButton.colors;
            colors.normalColor = boardManager.isMemoMode ? memoActiveColor : memoInactiveColor;
            colors.selectedColor = boardManager.isMemoMode ? memoActiveColor : memoInactiveColor;
            memoButton.colors = colors;
        }
    }
    
    // 힌트 사용
    public void OnHintButtonClick()
    {
        if (boardManager != null)
        {
            boardManager.UseHint();
        }
    }

    public void OnUI()
    {
        uiPanel.SetActive(true);
    }
    public void OffUI()
    {
        uiPanel.SetActive(false);
    }
    
}