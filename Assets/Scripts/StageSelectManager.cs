using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class StageSelectManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform stageButtonContainer;
    public GameObject stageButtonPrefab;
    public Button backButton;
    
    [Header("Stage Settings")]
    public int totalStages = 81;
    public int stagesPerPage = 20; // 한 페이지에 표시할 스테이지 수
    
    [Header("Progress")]
    public int unlockedStages = 1; // 해금된 스테이지 수

    private List<GameObject> stageButtons = new List<GameObject>();
    private int currentPage = 0;

    void Start()
    {
        LoadProgress();
        CreateStageButtons();
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMenu);
        }
    }

    void CreateStageButtons()
    {
        // 기존 버튼 제거
        foreach (var btn in stageButtons)
        {
            if (btn != null) Destroy(btn);
        }
        stageButtons.Clear();

        int startStage = currentPage * stagesPerPage + 1;
        int endStage = Mathf.Min(startStage + stagesPerPage - 1, totalStages);

        for (int i = startStage; i <= endStage; i++)
        {
            int stageNumber = i;
            GameObject btnObj;
            
            if (stageButtonPrefab != null)
            {
                btnObj = Instantiate(stageButtonPrefab, stageButtonContainer);
            }
            else
            {
                // 프리팹이 없으면 기본 버튼 생성
                btnObj = new GameObject($"Stage_{stageNumber}");
                btnObj.transform.SetParent(stageButtonContainer, false);
                btnObj.AddComponent<RectTransform>();
                btnObj.AddComponent<Image>();
                Button btn = btnObj.AddComponent<Button>();
                
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform, false);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 24;
                
                RectTransform textRect = text.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }

            // 버튼 텍스트 설정
            TextMeshProUGUI buttonText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                // 기본 텍스트
                string displayText = stageNumber.ToString();
                
                // 클리어한 스테이지는 체크 마크 표시
                if (IsStageClear(stageNumber))
                {
                    
                    
                    // 기록 표시 (선택사항)
                    if (RecordManager.Instance != null)
                    {
                        float record = RecordManager.Instance.GetStageRecord(stageNumber);
                        if (record > 0)
                        {
                            int minutes = (int)(record / 60);
                            int seconds = (int)(record % 60);
                            displayText += $"\n{minutes}:{seconds:D2}";
                            buttonText.fontSize = 15; // 작게
                        }
                    }
                }
                
                buttonText.text = displayText;
            }

            // 버튼 클릭 이벤트
            Button button = btnObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnStageButtonClick(stageNumber));
                
                // 잠금 상태 표시
                button.interactable = (stageNumber <= unlockedStages);
                
                // 잠긴 스테이지는 어둡게
                if (stageNumber > unlockedStages)
                {
                    Image img = button.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }
                }
            }

            stageButtons.Add(btnObj);
        }
    }

    void OnStageButtonClick(int stageNumber)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetStage(stageNumber);
            
            // 확실하게 스테이지 모드로 설정
            GameManager.Instance.currentMode = GameMode.Stage;
            
            Debug.Log($"스테이지 선택: 모드={GameManager.Instance.currentMode}, 스테이지={GameManager.Instance.currentStage}");
            
            SceneManager.LoadScene("InGame"); // 인게임 씬 이름에 맞게 수정
        }
    }

    public void NextPage()
    {
        int maxPage = (totalStages - 1) / stagesPerPage;
        if (currentPage < maxPage)
        {
            currentPage++;
            CreateStageButtons();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            CreateStageButtons();
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Mode"); // 메뉴 씬 이름에 맞게 수정
    }

    // 스테이지 클리어 시 호출 (특정 스테이지 해금)
    public static void UnlockStage(int clearedStage)
    {
        // 해당 스테이지 클리어 기록
        PlayerPrefs.SetInt($"Stage_{clearedStage}_Cleared", 1);
        
        int currentUnlocked = PlayerPrefs.GetInt("UnlockedStages", 1);
        int nextStage = clearedStage + 1;
        
        // 클리어한 스테이지의 다음 스테이지만 해금 (최대값 갱신)
        if (nextStage > currentUnlocked && nextStage <= 81)
        {
            PlayerPrefs.SetInt("UnlockedStages", nextStage);
            PlayerPrefs.Save();
            Debug.Log($"Stage {nextStage} 해금!");
        }
        else
        {
            PlayerPrefs.Save();
        }
    }
    
    // 스테이지 클리어 여부 확인
    public static bool IsStageClear(int stageNumber)
    {
        return PlayerPrefs.GetInt($"Stage_{stageNumber}_Cleared", 0) == 1;
    }

    void LoadProgress()
    {
        unlockedStages = PlayerPrefs.GetInt("UnlockedStages", 1);
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("UnlockedStages", 1);
        PlayerPrefs.Save();
        unlockedStages = 1;
        CreateStageButtons();
    }
}