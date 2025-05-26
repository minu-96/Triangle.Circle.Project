using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public int x, y;
    public Image shapeImage;
    public Button button;
    public Sprite[] shapeSprites;

    private ShapeType shape = ShapeType.None;
    private bool isLocked = false;

    private bool isInitialized = false;

    public void Init(int x, int y, PuzzleManager manager)
    {
        if (isInitialized) return; // 이미 초기화했으면 다시 하지 않음

        this.x = x;
        this.y = y;
        button.onClick.RemoveAllListeners(); // 혹시 이전 이벤트가 있다면 제거
        button.onClick.AddListener(() => manager.OnCellClicked(this));

        isInitialized = true;
    }

    public void SetShape(ShapeType type)
    {
        shape = type;
        shapeImage.sprite = shapeSprites[(int)type];
    }

    public ShapeType GetShape() => shape;

    public void Clear()
    {
        shape = ShapeType.None;
        shapeImage.sprite = null;
    }

    public void Lock()
    {
        isLocked = true;
        button.interactable = false;
    }

    public void Unlock()
    {
        isLocked = false;
        button.interactable = true;
        Debug.Log($"[UNLOCKED] {x},{y} 셀 버튼 활성화");
    }

    public bool IsLocked() => isLocked;


}
