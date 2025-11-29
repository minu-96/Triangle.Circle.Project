using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI difficultyText;
    public GameObject clearPanel;
    public TextMeshProUGUI clearTimeText;
    
    [Header("Managers")]
    public BoardManager boardManager;

    private float elapsedTime = 0f;
    private bool isGameActive = true;

    void Start()
    {
        // 난이도 표시
        if (difficultyText != null && GameManager.Instance != null)
        {
            difficultyText.text = $"{GameManager.Instance.currentDifficulty}";
        }

        // 클리어 패널 숨기기
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
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
        }

        Debug.Log($"게임 클리어! 시간: {elapsedTime:F2}초");
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
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu"); // 메뉴 씬 이름에 맞게 수정
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
}