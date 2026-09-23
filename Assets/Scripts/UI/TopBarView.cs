using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>모든 화면이 공유하는 상단바. 화면마다 다시 만들지 않는다.</summary>
public class TopBarView : MonoBehaviour
{
    public Button backButton;
    public Button infoButton;
    public Button settingsButton;
    public GameObject brandMarks;   // 뒤로가기가 숨을 때 보이는 3개 도형
    public TMP_Text brandText;
    public Image[] markImages;

    public void Bind(SamgakwonRouter router)
    {
        if (backButton != null) backButton.onClick.AddListener(router.Back);
        if (infoButton != null) infoButton.onClick.AddListener(router.ShowHelp);
        if (settingsButton != null) settingsButton.onClick.AddListener(router.ShowSettings);
        var theme = router.Theme;
        if (theme != null)
        {
            if (brandText != null) brandText.color = theme.ink;
            if (markImages != null)
                for (int i = 0; i < markImages.Length; i++)
                    if (markImages[i] != null) markImages[i].sprite = theme.Token((ShapeType)(i + 1));
        }
    }

    public void ShowBack(bool visible)
    {
        if (backButton != null) backButton.gameObject.SetActive(visible);
        if (brandMarks != null) brandMarks.SetActive(!visible);
    }
}
