using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "Samgakwon/Difficulty Settings")]
public class DifficultySettings : ScriptableObject
{
    [Header("Easy Mode")]
    public int easyEmptyCells = 15;
    [Range(0f, 1f)]
    public float easySymmetryChance = 0.3f; // 대칭 배치 확률
    
    [Header("Normal Mode")]
    public int normalEmptyCells = 35;
    [Range(0f, 1f)]
    public float normalSymmetryChance = 0.5f;
    
    [Header("Hard Mode")]
    public int hardEmptyCells = 55;
    [Range(0f, 1f)]
    public float hardSymmetryChance = 0.8f;

    [Header("Advanced Settings")]
    [Tooltip("블록당 최소 힌트 개수")]
    public int minHintsPerBlock = 1;
    
    [Tooltip("각 행/열에 최소 힌트 개수")]
    public int minHintsPerLine = 2;

    public int GetEmptyCellCount(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => easyEmptyCells,
            GameDifficulty.Normal => normalEmptyCells,
            GameDifficulty.Hard => hardEmptyCells,
            _ => normalEmptyCells
        };
    }

    public float GetSymmetryChance(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => easySymmetryChance,
            GameDifficulty.Normal => normalSymmetryChance,
            GameDifficulty.Hard => hardSymmetryChance,
            _ => normalSymmetryChance
        };
    }
}