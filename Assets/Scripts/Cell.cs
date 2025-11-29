using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    [Header("Cell Info")]
    public int row;
    public int col;
    public int blockIndex;
    
    [Header("Shape Data")]
    public ShapeType currentShape = ShapeType.None;
    public bool isInitial = false; // 초기 배치된 셀인지 (수정 불가)
    
    [Header("UI References")]
    public Image shapeImage;
    public Image backgroundImage;
    
    [Header("Colors")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f);
    public Color selectedColor = new Color(0.4f, 0.4f, 0.5f);
    public Color initialColor = new Color(0.15f, 0.15f, 0.15f);

    private BoardManager boardManager;

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
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        // 배경색 설정
        if (backgroundImage != null)
        {
            backgroundImage.color = isInitial ? initialColor : normalColor;
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
                shapeImage.color = Color.white; // 원본 색상 유지
                
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