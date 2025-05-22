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

    public void Init(int x, int y, PuzzleManager manager)
    {
        this.x = x;
        this.y = y;
        button.onClick.AddListener(() => manager.OnCellClicked(this));
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
    }

    public bool IsLocked() => isLocked;
}
