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

    [Header("Feedback / Effects")]
    [Tooltip("클리어 시 재생할 파티클(1종). 비어 있어도 안전.")]
    public ParticleSystem clearParticle;
    [Tooltip("엔드리스 누적 클리어 수 표시(선택).")]
    public TextMeshProUGUI endlessCountText;
    [Tooltip("챕터 전환 안내를 담당(선택). 없으면 안내 생략.")]
    public ChapterIntroController chapterIntro;

    private float elapsedTime = 0f;
    private bool isGameActive = true;

    [Header("UIPanel")]
    public GameObject uiPanel;
    

    void Start()
    {
        // 난이도/스테이지 표시
        if (difficultyText != null && GameManager.Instance != null)
        {
            switch (GameManager.Instance.currentMode)
            {
                case GameMode.Stage:
                    difficultyText.text = $"Stage {GameManager.Instance.currentStage}";
                    break;
                case GameMode.Endless:
                    difficultyText.text = "Endless";
                    break;
                default:
                    difficultyText.text = $"{GameManager.Instance.currentDifficulty}";
                    break;
            }
        }

        // 엔드리스 누적 수 표시
        UpdateEndlessCountDisplay();

        // 챕터 전환 안내(스테이지 첫 진입 시 1회) — 스테이지 모드에서만
        if (GameManager.Instance != null && GameManager.Instance.currentMode == GameMode.Stage
            && chapterIntro != null)
        {
            chapterIntro.MaybeShowForStage(GameManager.Instance.currentStage);
        }

        // 최고 기록 표시
        UpdateBestTimeDisplay();

        // 클리어 패널 숨기기
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
        if (uiPanel != null) uiPanel.SetActive(false);
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
        if (!isGameActive) return;
        // 엔드리스 모드는 즉시 다음 판으로 이어서 진행 (별도 처리)
        if (GameManager.Instance != null && GameManager.Instance.currentMode == GameMode.Endless)
        {
            OnEndlessBoardComplete();
            return;
        }

        isGameActive = false;

        // 클리어 파티클 연출
        PlayClearParticle();

        // 기록 저장
        bool isNewRecord = false;
        if (RecordManager.Instance != null)
        {
            isNewRecord = RecordManager.Instance.IsNewRecord(elapsedTime);
            RecordManager.Instance.SaveRecord(elapsedTime);
        }

        // 클리어 사운드 (신기록이면 상위음)
        if (isNewRecord) SFXManager.PlayNewRecord();
        else SFXManager.PlayClear();

        // 스테이지 모드면 다음 스테이지 해금 + 81 클리어 시 엔드리스 해금
        if (GameManager.Instance != null && GameManager.Instance.currentMode == GameMode.Stage)
        {
            StageSelectManager.UnlockStage(GameManager.Instance.currentStage);
            if (GameManager.Instance.currentStage >= Chapters.TotalStages)
            {
                GameManager.UnlockEndless();
            }
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

    // 엔드리스: 한 판 클리어 → 카운터 증가 → 즉시 새 판 생성해 이어서 플레이
    void OnEndlessBoardComplete()
    {
        int total = GameManager.IncrementEndlessClearCount();
        SFXManager.PlayClear();
        PlayClearParticle();
        UpdateEndlessCountDisplay();

        Debug.Log($"엔드리스 클리어! 누적 {total}판");

        // 새 판 즉시 생성, 타이머 리셋하고 계속 진행
        elapsedTime = 0f;
        isGameActive = true;
        if (boardManager != null) boardManager.ResetBoard();
    }

    void PlayClearParticle()
    {
        if (clearParticle != null)
        {
            clearParticle.Clear();
            clearParticle.Play();
        }
    }

    void UpdateEndlessCountDisplay()
    {
        if (endlessCountText == null) return;
        bool endless = GameManager.Instance != null && GameManager.Instance.currentMode == GameMode.Endless;
        endlessCountText.gameObject.SetActive(endless);
        if (endless)
            endlessCountText.text = $"클리어 {GameManager.GetEndlessClearCount()}판";
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
                if (difficultyText != null) difficultyText.text = $"Stage {nextStage}";
                if (chapterIntro != null) chapterIntro.MaybeShowForStage(nextStage);
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
        if (uiPanel != null) uiPanel.SetActive(true);
    }
    public void OffUI()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
    }
    
}