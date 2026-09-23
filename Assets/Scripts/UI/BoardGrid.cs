using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 9×9 칸을 항상 정사각형으로 유지한다.
/// CSS의 `grid-template-columns:repeat(9,1fr)` + `aspect-ratio:1`에 해당하는 역할로,
/// 절대 픽셀 대신 부모 폭에서 칸 크기를 계산해 어떤 해상도에서도 맞춘다.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public class BoardGrid : MonoBehaviour
{
    [Min(1)] public int columns = 9;

    GridLayoutGroup grid;
    RectTransform rect;

    void OnEnable() { Cache(); Apply(); }
    // OnEnable 은 레이아웃 전에 돌아 폭이 아직 확정되지 않을 수 있다.
    // Start 에서 한 번 더 계산해야 첫 프레임부터 올바른 칸 크기가 나온다.
    void Start() { Apply(); }
    void OnRectTransformDimensionsChange() => Apply();
#if UNITY_EDITOR
    void OnValidate() { Cache(); Apply(); }
#endif

    void Cache()
    {
        if (grid == null) grid = GetComponent<GridLayoutGroup>();
        if (rect == null) rect = (RectTransform)transform;
    }

    public void Apply()
    {
        Cache();
        if (grid == null || rect == null || columns < 1) return;
        var padding = grid.padding;
        float available = rect.rect.width - padding.left - padding.right - grid.spacing.x * (columns - 1);
        if (available <= 0) return;
        float size = available / columns;
        if (Mathf.Abs(grid.cellSize.x - size) > 0.01f || Mathf.Abs(grid.cellSize.y - size) > 0.01f)
            grid.cellSize = new Vector2(size, size);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
    }
}
