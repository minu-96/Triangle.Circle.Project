using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    [Header("Cell Info")]
    public int row;
    public int col;
    public int blockIndex;
    
    [Header("Shape Data")]
    public ShapeType currentShape = ShapeType.None;
    public bool isInitial = false; // 초기 배치된 셀인지 (수정 불가)
    
    [Header("Memo System")]
    public List<ShapeType> memos = new List<ShapeType>(); // 메모된 도형들
    
    [Header("UI References")]
    public Image shapeImage;
    public Image backgroundImage;
    public Transform memoContainer; // 메모 표시할 컨테이너
    public GameObject memoPrefab; // 메모 아이템 프리팹
    [Tooltip("고정 단서 표시(작은 점). 처음부터 놓인 칸에만 보인다.")]
    public GameObject givenMarker;
    [Tooltip("선택된 칸을 감싸는 테두리. 어떤 칸이 켜져 있는지 한눈에 보이게 한다.")]
    public GameObject selectionRing;
    
    [Header("Cell Sprites (있으면 색상 대신 사용)")]
    public Sprite normalSprite;
    public Sprite selectedSprite;
    public Sprite relatedSprite;
    public Sprite errorSprite;

    [Header("Colors")]
    // 삼각원 라이트 테마 (목업 기준: 크림 배경 · 틸 강조)
    public Color normalColor = new Color(0.988f, 0.973f, 0.933f);     // 크림 셀 배경
    public Color selectedColor = new Color(0.816f, 0.945f, 0.925f);   // 선택 셀 (연한 틸)
    public Color initialColor = new Color(0.976f, 0.953f, 0.898f);    // 초기(고정) 셀 - 살짝 진한 크림
    public Color editableHighlight = new Color(0.902f, 0.965f, 0.957f); // 관련 행·열·블록 하이라이트

    private BoardManager boardManager;
    private List<GameObject> memoObjects = new List<GameObject>();

    public void Initialize(int r, int c, BoardManager manager)
    {
        row = r;
        col = c;
        blockIndex = (row / 3) * 3 + (col / 3);
        boardManager = manager;
        
        UpdateVisual();
    }

    public void SetShape(ShapeType shape, bool initial = false)
    {
        currentShape = shape;
        isInitial = initial;
        
        // 도형을 배치하면 메모 지우기
        if (shape != ShapeType.None)
        {
            ClearMemos();
        }
        
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        // 배경 (선택 상태를 유지한 채 갱신한다)
        ApplyBackground();

        // 도형 이미지 설정
        if (shapeImage != null)
        {
            if (currentShape == ShapeType.None)
            {
                shapeImage.enabled = false;
            }
            else
            {
                shapeImage.enabled = true;
                shapeImage.sprite = boardManager.GetShapeSprite(currentShape);
                shapeImage.color = isInitial ? Color.white : new Color(1f, 1f, 1f, 0.9f);
                
                // 도형 크기 조절 (셀의 70% 크기)
                RectTransform rect = shapeImage.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.15f, 0.15f);
                    rect.anchorMax = new Vector2(0.85f, 0.85f);
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                }
            }
        }
        
        // 고정 단서 점
        if (givenMarker != null)
            givenMarker.SetActive(isInitial && currentShape != ShapeType.None);

        // 메모 표시
        UpdateMemoVisual();
    }
    
    public void AddMemo(ShapeType shape)
    {
        if (!memos.Contains(shape))
        {
            memos.Add(shape);
            UpdateMemoVisual();
        }
    }
    
    public void RemoveMemo(ShapeType shape)
    {
        if (memos.Contains(shape))
        {
            memos.Remove(shape);
            UpdateMemoVisual();
        }
    }
    
    public void ClearMemos()
    {
        memos.Clear();
        UpdateMemoVisual();
    }
    
    void UpdateMemoVisual()
    {
        // 기존 메모 오브젝트 제거
        foreach (var obj in memoObjects)
        {
            if (obj != null) Destroy(obj);
        }
        memoObjects.Clear();
        
        // 메인 도형이 있으면 메모 숨김
        if (currentShape != ShapeType.None || memoContainer == null)
        {
            return;
        }
        
        // 메모 표시 (최대 5개, 2x3 그리드로 배치)
        int memoCount = Mathf.Min(memos.Count, 5);
        for (int i = 0; i < memoCount; i++)
        {
            GameObject memoObj = new GameObject($"Memo_{memos[i]}");
            memoObj.transform.SetParent(memoContainer, false);
            
            Image memoImage = memoObj.AddComponent<Image>();
            memoImage.sprite = boardManager.GetShapeSprite(memos[i]);
            memoImage.color = new Color(1f, 1f, 1f, 0.4f); // 반투명
            
            RectTransform rect = memoObj.GetComponent<RectTransform>();
            
            // 위치 계산 (2열 그리드)
            int col = i % 2;
            int row = i / 2;
            
            float cellWidth = 0.4f;
            float cellHeight = 0.3f;
            float startX = 0.1f;
            float startY = 0.7f;
            
            rect.anchorMin = new Vector2(startX + col * cellWidth, startY - row * cellHeight);
            rect.anchorMax = new Vector2(startX + col * cellWidth + 0.35f, startY - row * cellHeight + 0.25f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            memoObjects.Add(memoObj);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        boardManager.OnCellClicked(this);
    }

    bool selected, related;
    Coroutine animationRoutine, conflictRoutine;

    public void SetSelection(bool isSelected, bool isRelated)
    {
        selected = isSelected;
        related = isRelated;
        if (selectionRing != null) selectionRing.SetActive(selected);
        ApplyBackground();
    }

    void ApplyBackground()
    {
        if (backgroundImage == null) return;
        Sprite sprite = selected ? selectedSprite : related ? relatedSprite : normalSprite;
        if (sprite != null)
        {
            backgroundImage.sprite = sprite;
            backgroundImage.color = Color.white;
        }
        else
        {
            backgroundImage.color = selected ? selectedColor : related ? editableHighlight
                                  : isInitial ? initialColor : normalColor;
        }
    }

    public void Highlight(bool enable) => SetSelection(enable, false);

    public void ShowConflict()
    {
        if (conflictRoutine != null) StopCoroutine(conflictRoutine);
        conflictRoutine = StartCoroutine(Conflict());
    }
    IEnumerator Conflict()
    {
        if (backgroundImage != null)
        {
            if (errorSprite != null) { backgroundImage.sprite = errorSprite; backgroundImage.color = Color.white; }
            else backgroundImage.color = new Color(1f, 0.83f, 0.79f);
        }
        yield return new WaitForSecondsRealtime(0.35f);
        SetSelection(selected, related);
    }
    public void PlayPlacement()
    {
        if (shapeImage == null || PlayerPrefs.GetInt("SamgakwonMotion", 1) == 0) return;
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(Pop());
    }
    IEnumerator Pop()
    {
        float elapsed = 0;
        while (elapsed < 0.16f)
        {
            elapsed += Time.unscaledDeltaTime;
            shapeImage.transform.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(elapsed / 0.16f * Mathf.PI));
            yield return null;
        }
        shapeImage.transform.localScale = Vector3.one;
    }
}
