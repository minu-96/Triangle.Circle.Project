using UnityEngine;
using UnityEngine.UI;

public class EditableIndicator : MonoBehaviour
{
    [Header("Settings")]
    public bool showIndicator = true;
    public Image indicatorImage;
    public Color editableColor = new Color(0.3f, 0.8f, 0.3f, 0.3f);
    
    private Cell cell;

    void Start()
    {
        cell = GetComponent<Cell>();
        
        if (indicatorImage == null)
        {
            // 자동으로 인디케이터 생성
            GameObject indicatorObj = new GameObject("EditableIndicator");
            indicatorObj.transform.SetParent(transform, false);
            
            indicatorImage = indicatorObj.AddComponent<Image>();
            indicatorImage.color = editableColor;
            
            RectTransform rect = indicatorObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // 맨 뒤로 보내기
            indicatorObj.transform.SetAsFirstSibling();
        }
        
        UpdateIndicator();
    }

    void Update()
    {
        UpdateIndicator();
    }

    void UpdateIndicator()
    {
        if (indicatorImage != null && cell != null)
        {
            // 초기 셀이 아니고, 표시 옵션이 켜져있으면 표시
            indicatorImage.enabled = showIndicator && !cell.isInitial;
        }
    }
    
    public void ToggleIndicator()
    {
        showIndicator = !showIndicator;
        UpdateIndicator();
    }
}