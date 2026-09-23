using TMPro;
using UnityEngine;

/// <summary>
/// 색·스프라이트·폰트를 한곳에 모은 디자인 토큰 에셋.
/// HTML 프로토타입의 :root 변수와 1:1로 대응하며, 화면 컴포넌트는 문자열 경로 대신
/// 이 에셋을 인스펙터로 참조한다(Resources.Load 제거).
/// </summary>
/// <summary>버튼 한 종류의 상태별 스프라이트 묶음(SpriteSwap 용).</summary>
[System.Serializable]
public struct ButtonSprites
{
    public Sprite normal, hover, pressed, disabled, selected;

    /// <summary>버튼을 SpriteSwap 으로 바꾸고 상태 스프라이트를 연결한다.</summary>
    public void Apply(UnityEngine.UI.Button button, UnityEngine.UI.Image image)
    {
        if (button == null || normal == null) return;
        if (image != null) { image.sprite = normal; image.color = Color.white; }
        button.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
        button.spriteState = new UnityEngine.UI.SpriteState
        {
            highlightedSprite = hover,
            pressedSprite = pressed,
            selectedSprite = selected,
            disabledSprite = disabled
        };
    }
}

[CreateAssetMenu(fileName = "SamgakwonTheme", menuName = "Samgakwon/Theme")]
public class SamgakwonTheme : ScriptableObject
{
    [Header("Palette (CSS :root)")]
    public Color paper = new Color32(0xFF, 0xFB, 0xF3, 0xFF);   // --paper
    public Color ink = new Color32(0x48, 0x4B, 0x57, 0xFF);     // --ink
    public Color teal = new Color32(0x08, 0xAA, 0xA5, 0xFF);    // --teal
    public Color line = new Color32(0xD2, 0xC8, 0xB9, 0xFF);    // --line
    public Color mint = new Color32(0xE7, 0xF7, 0xF2, 0xFF);    // --mint
    public Color coral = new Color32(0xFF, 0x88, 0x75, 0xFF);   // --coral

    [Header("Neutrals")]
    public Color backdrop = new Color32(0xEE, 0xE9, 0xE1, 0xFF); // body background
    public Color surface = new Color32(0xFF, 0xFD, 0xF8, 0xFF);  // card background
    public Color muted = new Color32(0x93, 0x90, 0x8A, 0xFF);    // secondary text
    public Color hairline = new Color32(0xE8, 0xDF, 0xD0, 0xFF); // card border
    public Color blob = new Color32(0xFF, 0xF0, 0xCC, 0xFF);     // decorative circles

    [Header("Cells")]
    public Color cellNormal = new Color32(0xFF, 0xFB, 0xF3, 0xFF);
    public Color cellGiven = new Color32(0xF9, 0xF4, 0xE9, 0xFF);
    public Color cellSelected = new Color32(0xE6, 0xFA, 0xF3, 0xFF);
    public Color cellRelated = new Color32(0xE7, 0xF7, 0xF2, 0xFF);
    public Color cellConflict = new Color32(0xFF, 0xE2, 0xDC, 0xFF);
    public Color cellHairline = new Color32(0xE4, 0xDC, 0xCF, 0xFF);
    [Tooltip("선택된 칸을 감싸는 테두리 스프라이트")]
    public Sprite cellSelectionRing;

    [Header("Stage buttons")]
    public Color stageLocked = new Color32(0xEE, 0xEA, 0xE3, 0xFF);
    public Color stageDone = new Color32(0xBC, 0xE7, 0xDC, 0xFF);
    public Color stageCurrent = new Color32(0xFF, 0xAD, 0x98, 0xFF);
    public Color stageCurrentInk = new Color32(0x68, 0x3D, 0x35, 0xFF);

    [Header("Shape tokens — index 0..4 = 삼각형/원/사각형/마름모/오각형")]
    public Sprite[] tokens = new Sprite[5];
    public Sprite[] tokensGiven = new Sprite[5];

    [Header("Panels")]
    public Sprite panelCard;
    public Sprite panelPopup;
    public Sprite panelToast;
    public Sprite panelTooltip;
    public Sprite modalScrim;

    [Header("Buttons — 상태별 스프라이트")]
    public ButtonSprites primary;     // 주요 버튼
    public ButtonSprites secondary;   // 보조 버튼 · 도구
    public ButtonSprites stage;       // 스테이지 격자
    public ButtonSprites selector;    // 도형 칩

    [Header("Board")]
    public Sprite cellNormalSprite;
    public Sprite cellSelectedSprite;
    public Sprite cellRelatedSprite;
    public Sprite cellErrorSprite;

    [Header("Controls")]
    public Sprite progressTrack;
    public Sprite progressFill;

    [Header("Badges")]
    public Sprite badgeComplete, badgeCurrent, badgeLocked;

    [Header("Tutorial")]
    public Sprite tutorialRow, tutorialColumn, tutorialBlock, tutorialInvalid;

    [Header("Chapters / Particles")]
    public Sprite[] chapterIcons = new Sprite[4];
    public Sprite[] confetti = new Sprite[5];

    [Header("Icons")]
    public Sprite iconBack, iconInfo, iconSettings, iconPause, iconErase, iconHint;
    public Sprite iconPlay, iconSparkle, iconInfinity, iconLock, iconCheck, iconTrophy, iconMemo;

    [Header("Type")]
    public TMP_FontAsset font;

    /// <summary>챕터(1~4) 아이콘.</summary>
    public Sprite ChapterIcon(int chapter)
    {
        int i = chapter - 1;
        return chapterIcons != null && i >= 0 && i < chapterIcons.Length ? chapterIcons[i] : null;
    }

    /// <summary>ShapeType(1..5) → 토큰 스프라이트. 범위를 벗어나면 null.</summary>
    public Sprite Token(ShapeType shape, bool given = false)
    {
        int index = (int)shape - 1;
        var set = given && tokensGiven != null && tokensGiven.Length == 5 ? tokensGiven : tokens;
        if (set == null || index < 0 || index >= set.Length) return null;
        return set[index] != null ? set[index] : (tokens != null && index < tokens.Length ? tokens[index] : null);
    }
}
