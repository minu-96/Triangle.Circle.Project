using UnityEngine;

public class ShapeSelect : MonoBehaviour
{
    public ShapeType selectedShape = ShapeType.None;

    public void SelectShape(int shapeIndex)
    {
        selectedShape = (ShapeType)shapeIndex;
    }

    public void OnCellClicked(Cell cell)
    {
        if (selectedShape == ShapeType.None) return;

        cell.SetShape(selectedShape);
        // 여기에 규칙 검사나 정답 확인도 추가할 수 있음
    }

}
