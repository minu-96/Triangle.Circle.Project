using UnityEngine;

public class GameManagerInitializer : MonoBehaviour
{
    void Awake()
    {
        // GameManager가 아직 없으면 생성
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
        
        // RecordManager가 아직 없으면 생성
        if (RecordManager.Instance == null)
        {
            GameObject rm = new GameObject("RecordManager");
            rm.AddComponent<RecordManager>();
        }
    }
}