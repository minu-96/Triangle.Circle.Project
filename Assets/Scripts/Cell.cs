using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Cell : MonoBehaviour
{
    public int x, y;
    public Image shapeImage;
    public Button button;
    public Sprite[] shapeSprites;

    private ShapeType shape = ShapeType.None;

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
}
