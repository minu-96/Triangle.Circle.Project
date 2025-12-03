using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    
    [Header("Colors")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f);
    public Color selectedColor = new Color(0.4f, 0.4f, 0.5f);
    public Color initialColor = new Color(0.15f, 0.15f, 0.15f);
    public Color editableHighlight = new Color(0.3f, 0.3f, 0.35f); // 수정 가능 표시

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
        // 배경색 설정
        if (backgroundImage != null)
        {
            if (isInitial)
            {
                backgroundImage.color = initialColor;
            }
            else
            {
                // 수정 가능한 셀은 약간 다른 색으로 표시
                backgroundImage.color = normalColor;
            }
        }

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
        // 초기 배치된 셀은 클릭 불가
        if (isInitial) return;
        
        boardManager.OnCellClicked(this);
    }

    public void Highlight(bool enable)
    {
        if (backgroundImage != null && !isInitial)
        {
            backgroundImage.color = enable ? selectedColor : normalColor;
        }
    }
}