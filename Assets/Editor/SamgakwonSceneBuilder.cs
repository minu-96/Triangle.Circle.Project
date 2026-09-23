using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>
/// 삼각원 UI를 "실제 씬 + 프리팹 에셋"으로 한 번에 생성한다.
/// 절대 좌표 대신 앵커 · LayoutGroup · ContentSizeFitter 로 구성하므로
/// 생성 후에는 인스펙터에서 그대로 편집할 수 있다.
/// </summary>
public static class SamgakwonSceneBuilder
{
    const string ScenePath = "Assets/WebScene/Samgakwon.unity";
    const string Folder = "Assets/Samgakwon";
    const string PrefabFolder = Folder + "/Prefabs";
    const string ThemePath = Folder + "/SamgakwonTheme.asset";
    const string FontPath = Folder + "/NanumGothic SDF.asset";
    const string TtfPath = "Assets/Resources/Fonts/NanumGothic-Regular.ttf";
    const string Png = "Assets/Resources/samgakwon-assets-v1/png/";

    static SamgakwonTheme theme;

    [MenuItem("Samgakwon/Build UI (Scene + Prefabs)")]
    public static void Build()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("삼각원", "Play 모드를 끄고 실행하세요.", "확인");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsureFolders();
        theme = BuildTheme();

        var cellPrefab = BuildCellPrefab();
        var actionPrefab = BuildActionButtonPrefab();
        var stagePrefab = BuildStageButtonPrefab();
        var piecePrefab = BuildShapePiecePrefab();
        var cardPrefab = BuildMenuCardPrefab();
        var chapterPrefab = BuildChapterSectionPrefab(stagePrefab);
        var confettiPrefab = BuildConfettiPiecePrefab();

        BuildScene(cellPrefab, actionPrefab, piecePrefab, cardPrefab, chapterPrefab, confettiPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("삼각원 UI 생성 완료 — " + ScenePath);
    }

    // ── 에셋 준비 ───────────────────────────────────────────
    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "Samgakwon");
        if (!AssetDatabase.IsValidFolder(PrefabFolder)) AssetDatabase.CreateFolder(Folder, "Prefabs");
    }

    static Sprite Load(string relativePath)
    {
        string path = Png + relativePath + ".png";
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (sprites.Length == 0) { Debug.LogWarning("스프라이트를 찾지 못했습니다: " + path); return null; }
        // 자동 분할로 조각이 여러 개면 첫 조각만 잡혀 아이콘 일부만 그려진다.
        // (예: 일시정지 아이콘의 막대 하나) Sprite Mode 를 Single 로 바꿔야 한다.
        if (sprites.Length > 1)
            Debug.LogWarning($"{path} 가 {sprites.Length}조각으로 분할돼 있습니다. " +
                             "Inspector 에서 Sprite Mode 를 Single 로 바꾸세요. 지금은 첫 조각만 사용합니다.");
        return sprites[0];
    }

    /// <summary>9-slice 로 그린다. 모서리 반경을 유지한 채 가운데만 늘어난다.</summary>
    static Image Sliced(Image image)
    {
        if (image != null && image.sprite != null) image.type = Image.Type.Sliced;
        return image;
    }

    static ButtonSprites LoadButtonSet(string prefix) => new ButtonSprites
    {
        normal   = Load(prefix + "_normal"),
        hover    = Load(prefix + "_hover"),
        pressed  = Load(prefix + "_pressed"),
        disabled = Load(prefix + "_disabled"),
        selected = Load(prefix + "_selected"),
    };

    static TMP_FontAsset BuildFont()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (existing != null) return existing;

        var ttf = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
        if (ttf == null) { Debug.LogWarning("한글 폰트를 찾지 못했습니다: " + TtfPath); return null; }

        // 한글은 글리프가 많으므로 동적 아틀라스로 만든다.
        var asset = TMP_FontAsset.CreateFontAsset(ttf, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
            AtlasPopulationMode.Dynamic, true);
        asset.name = "NanumGothic SDF";
        AssetDatabase.CreateAsset(asset, FontPath);
        if (asset.atlasTextures != null && asset.atlasTextures.Length > 0)
        {
            asset.atlasTextures[0].name = "NanumGothic Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
        }
        if (asset.material != null)
        {
            asset.material.name = "NanumGothic Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
        }
        AssetDatabase.SaveAssets();
        return asset;
    }

    static SamgakwonTheme BuildTheme()
    {
        var asset = AssetDatabase.LoadAssetAtPath<SamgakwonTheme>(ThemePath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SamgakwonTheme>();
            AssetDatabase.CreateAsset(asset, ThemePath);
        }

        string[] kinds = { "triangle", "circle", "square", "diamond", "pentagon" };
        asset.tokens = kinds.Select(k => Load("tokens/" + k + "_normal")).ToArray();
        asset.tokensGiven = kinds.Select(k => Load("tokens/" + k + "_given")).ToArray();

        asset.cellSelectionRing = Load("board/selection_outline");
        asset.cellNormalSprite = Load("board/cell_normal");
        asset.cellSelectedSprite = Load("board/cell_selected");
        asset.cellRelatedSprite = Load("board/cell_related");
        asset.cellErrorSprite = Load("board/cell_error");

        asset.panelCard = Load("panels/surface");
        asset.panelPopup = Load("panels/popup");
        asset.panelToast = Load("panels/toast");
        asset.panelTooltip = Load("panels/tooltip");
        asset.modalScrim = Load("background/modal_scrim");

        asset.primary = LoadButtonSet("buttons/primary");
        asset.secondary = LoadButtonSet("buttons/secondary");
        asset.stage = LoadButtonSet("buttons/stage");
        asset.selector = LoadButtonSet("buttons/selector");

        asset.progressTrack = Load("controls/progress_track");
        asset.progressFill = Load("controls/progress_fill");

        asset.badgeComplete = Load("badges/complete");
        asset.badgeCurrent = Load("badges/current");
        asset.badgeLocked = Load("badges/locked");

        asset.tutorialRow = Load("tutorial/row_two_allowed");
        asset.tutorialColumn = Load("tutorial/column_two_allowed");
        asset.tutorialBlock = Load("tutorial/block_two_allowed");
        asset.tutorialInvalid = Load("tutorial/third_is_invalid");

        asset.chapterIcons = new[] { Load("chapters/chapter_1"), Load("chapters/chapter_2"),
                                     Load("chapters/chapter_3"), Load("chapters/chapter_4") };
        asset.confetti = new[] { Load("particles/confetti_triangle"), Load("particles/confetti_circle"),
                                 Load("particles/confetti_square"), Load("particles/confetti_diamond"),
                                 Load("particles/confetti_pentagon") };

        asset.iconBack = Load("icons/back_dark");
        asset.iconInfo = Load("icons/info_dark");
        asset.iconSettings = Load("icons/settings_dark");
        asset.iconPause = Load("icons/pause_dark");
        asset.iconErase = Load("icons/erase_teal");
        asset.iconHint = Load("icons/hint_teal");
        asset.iconPlay = Load("icons/play_dark");
        asset.iconSparkle = Load("icons/sparkle_teal");
        asset.iconInfinity = Load("icons/infinity_dark");
        asset.iconLock = Load("icons/lock_dark");
        asset.iconCheck = Load("icons/check_teal");
        asset.iconTrophy = Load("icons/trophy_teal");
        asset.iconMemo = Load("icons/memo_teal");

        asset.font = BuildFont();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    // ── uGUI 헬퍼 ──────────────────────────────────────────
    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        if (parent != null) rect.SetParent(parent, false);
        return rect;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static Image AddImage(RectTransform rect, Sprite sprite, Color color, bool raycast = true)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycast;
        image.preserveAspect = sprite != null && rect.gameObject.name.StartsWith("Icon");
        return image;
    }

    static TextMeshProUGUI AddText(RectTransform rect, string content, float size, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.Left, FontStyles style = FontStyles.Normal)
    {
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (theme != null && theme.font != null) text.font = theme.font;
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = style;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    static TextMeshProUGUI TextChild(Transform parent, string name, string content, float size, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.Left, FontStyles style = FontStyles.Normal)
    {
        var rect = NewRect(name, parent);
        return AddText(rect, content, size, color, alignment, style);
    }

    static VerticalLayoutGroup Vertical(RectTransform rect, int spacing, RectOffset padding,
        TextAnchor align = TextAnchor.UpperCenter, bool expandHeight = false)
    {
        var group = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        group.spacing = spacing;
        group.padding = padding ?? new RectOffset();
        group.childAlignment = align;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = expandHeight;
        return group;
    }

    static HorizontalLayoutGroup Horizontal(RectTransform rect, int spacing, RectOffset padding,
        TextAnchor align = TextAnchor.MiddleLeft, bool expandWidth = false)
    {
        var group = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
        group.spacing = spacing;
        group.padding = padding ?? new RectOffset();
        group.childAlignment = align;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = expandWidth;
        group.childForceExpandHeight = false;
        return group;
    }

    static ContentSizeFitter FitVertical(RectTransform rect)
    {
        var fitter = rect.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return fitter;
    }

    static LayoutElement Size(RectTransform rect, float width = -1, float height = -1, bool flexibleWidth = false)
    {
        var element = rect.GetComponent<LayoutElement>();
        if (element == null) element = rect.gameObject.AddComponent<LayoutElement>();
        if (width >= 0) { element.preferredWidth = width; element.minWidth = width; }
        if (height >= 0) { element.preferredHeight = height; element.minHeight = height; }
        if (flexibleWidth) element.flexibleWidth = 1;
        return element;
    }

    static Button MakeButton(RectTransform rect, Image target)
    {
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = target;
        var colors = button.colors;
        colors.highlightedColor = new Color(0.95f, 0.99f, 0.96f);
        colors.pressedColor = new Color(0.86f, 0.94f, 0.90f);
        colors.disabledColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        // 모든 버튼이 이 함수를 거치므로 클릭음을 여기서 한 번에 붙인다.
        rect.gameObject.AddComponent<ButtonClickSound>();
        return button;
    }

    static GameObject SavePrefab(GameObject source, string name)
    {
        string path = PrefabFolder + "/" + name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
        Object.DestroyImmediate(source);
        return prefab;
    }

    // ── 프리팹 ──────────────────────────────────────────────
    static GameObject BuildCellPrefab()
    {
        var root = NewRect("Cell", null);
        root.sizeDelta = new Vector2(64, 64);
        // 원본 CSS 의 칸은 각지고(border-radius:0) 헤어라인만 있다.
        // 칸마다 둥근 스프라이트를 깔면 칸이 따로 떠 보여 격자로 읽히지 않는다.
        var background = AddImage(root, null, theme.cellNormal);

        var shape = NewRect("Shape", root);
        shape.anchorMin = new Vector2(0.14f, 0.14f);
        shape.anchorMax = new Vector2(0.86f, 0.86f);
        shape.offsetMin = shape.offsetMax = Vector2.zero;
        var shapeImage = AddImage(shape, null, Color.white, false);
        shapeImage.preserveAspect = true;

        var memos = NewRect("Memos", root);
        Stretch(memos);

        var dot = NewRect("Given Dot", root);
        dot.anchorMin = dot.anchorMax = new Vector2(1f, 0f);
        dot.pivot = new Vector2(1f, 0f);
        dot.anchoredPosition = new Vector2(-5f, 5f);
        dot.sizeDelta = new Vector2(4f, 4f);
        AddImage(dot, null, new Color32(0xBC, 0xB5, 0xA9, 0xFF), false);

        var ring = NewRect("Selection", root);
        Stretch(ring);
        // Sliced 는 모서리 반경이 원본 픽셀(약 12px)로 고정돼 너무 둥글다.
        // Simple 이면 100px 원본이 칸 크기(약 66px)로 줄며 반경도 ~8px 가 된다(CSS 의 7px 와 비슷).
        var ringImage = AddImage(ring, theme.cellSelectionRing, Color.white, false);
        ringImage.preserveAspect = false;
        ring.gameObject.SetActive(false);

        var cell = root.gameObject.AddComponent<Cell>();
        cell.selectionRing = ring.gameObject;
        cell.backgroundImage = background;
        cell.shapeImage = shapeImage;
        cell.memoContainer = memos;
        cell.givenMarker = dot.gameObject;
        cell.normalColor = theme.cellNormal;
        cell.initialColor = theme.cellGiven;
        cell.selectedColor = theme.cellSelected;
        cell.editableHighlight = theme.cellRelated;

        return SavePrefab(root.gameObject, "Cell");
    }

    static GameObject BuildActionButtonPrefab()
    {
        var root = NewRect("Action Button", null);
        root.sizeDelta = new Vector2(360, 54);
        var background = Sliced(AddImage(root, theme.secondary.normal, Color.white));
        Size(root, height: 54);
        var group = Horizontal(root, 9, new RectOffset(16, 16, 0, 0), TextAnchor.MiddleCenter);
        group.childForceExpandWidth = false;

        var icon = NewRect("Icon", root);
        Size(icon, 24, 24);
        var iconImage = AddImage(icon, null, Color.white, false);
        iconImage.preserveAspect = true;
        icon.gameObject.SetActive(false);

        var label = TextChild(root, "Label", "버튼", 20, theme.ink, TextAlignmentOptions.Center, FontStyles.Bold);

        var button = MakeButton(root, background);
        var view = root.gameObject.AddComponent<LabeledButton>();
        view.button = button;
        view.background = background;
        view.icon = iconImage;
        view.label = label;
        return SavePrefab(root.gameObject, "ActionButton");
    }

    static GameObject BuildStageButtonPrefab()
    {
        var root = NewRect("Stage Button", null);
        root.sizeDelta = new Vector2(100, 90);
        var background = Sliced(AddImage(root, theme.stage.normal, theme.stageLocked));

        var number = TextChild(root, "Number", "1", 19, theme.ink, TextAlignmentOptions.Center, FontStyles.Bold);
        Stretch(number.rectTransform);
        number.rectTransform.offsetMin = new Vector2(0f, 14f); // 아래 14px 는 기록 자리

        var record = TextChild(root, "Record", "", 10, theme.muted, TextAlignmentOptions.Center);
        var recordRect = record.rectTransform;
        recordRect.anchorMin = new Vector2(0f, 0f);
        recordRect.anchorMax = new Vector2(1f, 0f);
        recordRect.pivot = new Vector2(0.5f, 0f);
        recordRect.anchoredPosition = new Vector2(0f, 6f);
        recordRect.sizeDelta = new Vector2(0f, 14f);
        record.gameObject.SetActive(false);

        var badge = NewRect("Badge", root);
        badge.anchorMin = badge.anchorMax = new Vector2(1f, 1f);
        badge.pivot = new Vector2(1f, 1f);
        badge.anchoredPosition = new Vector2(-6f, -6f);
        badge.sizeDelta = new Vector2(22f, 22f);
        var badgeImage = AddImage(badge, null, Color.white, false);
        badgeImage.preserveAspect = true;

        var button = MakeButton(root, background);
        var view = root.gameObject.AddComponent<StageButtonView>();
        view.button = button;
        view.background = background;
        view.badge = badgeImage;
        view.numberText = number;
        view.recordText = record;
        return SavePrefab(root.gameObject, "StageButton");
    }

    static GameObject BuildShapePiecePrefab()
    {
        var root = NewRect("Shape Piece", null);
        root.sizeDelta = new Vector2(74, 84);
        Size(root, 74, 84);
        var background = Sliced(AddImage(root, theme.selector.normal, Color.white));
        var outline = root.gameObject.AddComponent<Outline>();
        outline.effectColor = theme.teal;
        outline.effectDistance = new Vector2(2.5f, 2.5f);
        outline.enabled = false;

        var token = NewRect("Token", root);
        token.anchorMin = new Vector2(0.5f, 1f);
        token.anchorMax = new Vector2(0.5f, 1f);
        token.pivot = new Vector2(0.5f, 1f);
        token.anchoredPosition = new Vector2(0f, -6f);
        token.sizeDelta = new Vector2(48f, 48f);
        var tokenImage = AddImage(token, null, Color.white, false);
        tokenImage.preserveAspect = true;

        var number = TextChild(root, "Number", "1", 13, theme.muted, TextAlignmentOptions.Center);
        var numberRect = number.rectTransform;
        numberRect.anchorMin = new Vector2(0f, 0f);
        numberRect.anchorMax = new Vector2(1f, 0f);
        numberRect.pivot = new Vector2(0.5f, 0f);
        numberRect.anchoredPosition = new Vector2(0f, 6f);
        numberRect.sizeDelta = new Vector2(0f, 18f);

        var button = MakeButton(root, background);
        var view = root.gameObject.AddComponent<ShapePieceView>();
        view.button = button;
        view.background = background;
        view.tokenImage = tokenImage;
        view.numberText = number;
        view.selectionOutline = outline;
        return SavePrefab(root.gameObject, "ShapePiece");
    }

    static GameObject BuildMenuCardPrefab()
    {
        var root = NewRect("Menu Card", null);
        root.sizeDelta = new Vector2(420, 89);
        var background = Sliced(AddImage(root, theme.panelCard, theme.surface));
        Size(root, height: 89);
        Horizontal(root, 18, new RectOffset(22, 22, 18, 18), TextAnchor.MiddleLeft);

        var icon = NewRect("Icon", root);
        Size(icon, 34, 34);
        var iconImage = AddImage(icon, null, Color.white, false);
        iconImage.preserveAspect = true;

        var column = NewRect("Text", root);
        Size(column, flexibleWidth: true);
        Vertical(column, 4, null, TextAnchor.MiddleLeft);
        var title = TextChild(column, "Title", "클래식", 19, theme.ink, TextAlignmentOptions.Left, FontStyles.Bold);
        var description = TextChild(column, "Description", "설명", 14, theme.muted);

        var arrow = TextChild(root, "Arrow", "›", 22, theme.muted, TextAlignmentOptions.Center);
        Size(arrow.rectTransform, 14, 30);

        root.gameObject.AddComponent<CanvasGroup>();
        var button = MakeButton(root, background);
        var view = root.gameObject.AddComponent<MenuCardView>();
        view.button = button;
        view.background = background;
        view.icon = iconImage;
        view.titleText = title;
        view.descriptionText = description;
        view.arrow = arrow.gameObject;
        return SavePrefab(root.gameObject, "MenuCard");
    }

    static GameObject BuildChapterSectionPrefab(GameObject stageButton)
    {
        var root = NewRect("Chapter Section", null);
        root.sizeDelta = new Vector2(620, 300);
        Sliced(AddImage(root, theme.panelCard, theme.surface, false));
        Vertical(root, 8, new RectOffset(18, 18, 18, 18), TextAnchor.UpperLeft);
        FitVertical(root);

        var header = NewRect("Header", root);
        Size(header, height: 26);
        Horizontal(header, 8, null, TextAnchor.MiddleLeft);
        var chapterIcon = NewRect("Icon", header);
        Size(chapterIcon, 24, 24);
        var chapterIconImage = AddImage(chapterIcon, theme.ChapterIcon(1), Color.white, false);
        chapterIconImage.preserveAspect = true;
        var title = TextChild(header, "Title", "1장 · 입문", 18, theme.ink, TextAlignmentOptions.Left, FontStyles.Bold);
        Size(title.rectTransform, flexibleWidth: true);
        var range = TextChild(header, "Range", "1–20", 13, theme.muted, TextAlignmentOptions.Right);
        Size(range.rectTransform, 70, 22);

        var tagline = TextChild(root, "Tagline", "설명", 13, theme.muted);
        Size(tagline.rectTransform, height: 20);

        var grid = NewRect("Grid", root);
        var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.spacing = new Vector2(9, 9);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 5;
        var square = grid.gameObject.AddComponent<BoardGrid>();
        square.columns = 5;
        FitVertical(grid);

        var view = root.gameObject.AddComponent<ChapterSectionView>();
        view.icon = chapterIconImage;
        view.titleText = title;
        view.rangeText = range;
        view.taglineText = tagline;
        view.gridRoot = grid;
        view.stageButtonPrefab = stageButton.GetComponent<StageButtonView>();
        return SavePrefab(root.gameObject, "ChapterSection");
    }

    static GameObject BuildConfettiPiecePrefab()
    {
        var root = NewRect("Confetti Piece", null);
        root.sizeDelta = new Vector2(18, 18);
        var image = AddImage(root, null, Color.white, false);
        image.preserveAspect = true;
        return SavePrefab(root.gameObject, "ConfettiPiece");
    }

    // ── 씬 ──────────────────────────────────────────────────
    static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var property = so.FindProperty(field);
        if (property == null) { Debug.LogWarning($"{target.GetType().Name}.{field} 를 찾지 못했습니다."); return; }
        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BuildScene(GameObject cellPrefab, GameObject actionPrefab, GameObject piecePrefab,
        GameObject cardPrefab, GameObject chapterPrefab, GameObject confettiPrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 카메라 (오버레이 캔버스라 렌더링엔 불필요하지만 배경색과 오디오 리스너를 맡는다)
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
        var camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = theme.backdrop;
        camera.orthographic = true;
        camera.cullingMask = 0;

        // 효과음 매니저를 씬에 둔다. 제작된 오디오 클립을 인스펙터에서 꽂을 수 있어야 하고,
        // 비워 두면 SFXManager 가 합성 톤으로 대체한다.
        var audioObject = new GameObject("Samgakwon Audio", typeof(AudioSource), typeof(SFXManager));
        var sfx = audioObject.GetComponent<SFXManager>();
        sfx.sfxSource = audioObject.GetComponent<AudioSource>();
        sfx.sfxSource.playOnAwake = false;

        var events = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        events.AddComponent<StandaloneInputModule>();
#endif

        // 캔버스
        var canvasObject = new GameObject("Samgakwon UI", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(680, 1000);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        var canvasRect = (RectTransform)canvasObject.transform;

        // 화면 맨 뒤. 칸·버튼이 가져가지 않은 클릭만 여기로 떨어져 칸 선택을 푼다.
        var backdrop = NewRect("Backdrop", canvasRect);
        Stretch(backdrop);
        AddImage(backdrop, null, theme.backdrop, true);
        var catcher = backdrop.gameObject.AddComponent<BackgroundClickCatcher>();

        // 세로형 페이지: 폭 680 고정, 높이는 화면에 맞춰 늘어난다 (CSS #app)
        var page = NewRect("Page", canvasRect);
        page.anchorMin = new Vector2(0.5f, 0f);
        page.anchorMax = new Vector2(0.5f, 1f);
        page.pivot = new Vector2(0.5f, 0.5f);
        page.sizeDelta = new Vector2(680, 0);
        AddImage(page, null, theme.paper, false);

        Blob(page, new Vector2(0f, 0.5f), new Vector2(-110, 40));
        Blob(page, new Vector2(1f, 0f), new Vector2(120, 160));

        var content = NewRect("Content", page);
        Stretch(content);
        var contentGroup = Vertical(content, 0, new RectOffset(30, 30, 20, 24), TextAnchor.UpperCenter);
        contentGroup.childForceExpandWidth = true;

        // 상단바
        var topBar = BuildTopBar(content);

        var screens = NewRect("Screens", content);
        var screensElement = screens.gameObject.AddComponent<LayoutElement>();
        screensElement.flexibleHeight = 1;

        var home = BuildHomeScreen(screens, actionPrefab, cardPrefab);
        var stages = BuildStagesScreen(screens, chapterPrefab);
        var game = BuildGameScreen(screens, cellPrefab, actionPrefab, piecePrefab);

        // 모달 · 토스트 · 색종이는 페이지 위에 겹친다
        var transition = BuildChapterTransition(canvasRect);
        var modals = BuildModalLayer(canvasRect, actionPrefab);
        var toast = BuildToast(canvasRect);
        var confetti = BuildConfetti(canvasRect, confettiPrefab);

        var music = canvasObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;

        var router = canvasObject.AddComponent<SamgakwonRouter>();
        SetRef(router, "theme", theme);
        SetRef(router, "topBar", topBar);
        SetRef(router, "modals", modals);
        SetRef(router, "toast", toast);
        SetRef(router, "confetti", confetti);
        SetRef(router, "chapterTransition", transition);
        SetRef(router, "backgroundCatcher", catcher);
        SetRef(router, "music", music);
        SetRef(router, "sfx", sfx);
        SetRef(router, "homeScreen", home);
        SetRef(router, "stagesScreen", stages);
        SetRef(router, "gameScreen", game);
        SetRef(modals, "theme", theme);

        // 저장 전에 레이아웃을 한 번 돌린다. 그래야 BoardGrid 가 실제 폭으로 칸 크기를
        // 계산한 값이 직렬화되고, 씬을 열자마자 보드/격자가 제 크기로 보인다.
        // 블록이 중첩돼 있어 바깥 그리드가 확정된 뒤에야 안쪽 칸 크기가 정해진다.
        // 바깥 -> 안쪽 순으로 수렴하도록 몇 번 반복한다.
        Canvas.ForceUpdateCanvases();
        for (int pass = 0; pass < 3; pass++)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(page);
            foreach (var grid in canvasObject.GetComponentsInChildren<BoardGrid>(true)) grid.Apply();
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(page);

        // 씬을 열었을 때 화면이 겹쳐 보이지 않도록 메인만 켠 상태로 저장한다.
        stages.gameObject.SetActive(false);
        game.gameObject.SetActive(false);
        home.gameObject.SetActive(true);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        RegisterFirstScene();
    }

    static void Blob(RectTransform parent, Vector2 anchor, Vector2 offset)
    {
        var blob = NewRect("Blob", parent);
        blob.anchorMin = blob.anchorMax = anchor;
        blob.pivot = new Vector2(0.5f, 0.5f);
        blob.anchoredPosition = offset;
        blob.sizeDelta = new Vector2(220, 220);
        var image = AddImage(blob, Load("particles/soft_glow"), theme.blob, false);
        image.raycastTarget = false;
    }

    static void RegisterFirstScene()
    {
        var list = EditorBuildSettings.scenes.ToList();
        list.RemoveAll(s => s.path == ScenePath);
        list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
    }

    static TopBarView BuildTopBar(RectTransform parent)
    {
        var bar = NewRect("Top Bar", parent);
        Size(bar, height: 46);
        Horizontal(bar, 6, null, TextAnchor.MiddleLeft);

        var brand = NewRect("Brand", bar);
        Size(brand, flexibleWidth: true);
        var brandGroup = Horizontal(brand, 9, null, TextAnchor.MiddleLeft);
        brandGroup.childForceExpandWidth = false;

        var back = IconButton(brand, "Back", theme.iconBack);
        var marks = NewRect("Marks", brand);
        Size(marks, 72, 22);
        Horizontal(marks, 4, null, TextAnchor.MiddleLeft);
        var markImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var mark = NewRect("Mark " + (i + 1), marks);
            Size(mark, 20, 20);
            markImages[i] = AddImage(mark, theme.Token((ShapeType)(i + 1)), Color.white, false);
            markImages[i].preserveAspect = true;
        }
        var brandText = TextChild(brand, "Name", "삼각원", 18, theme.ink, TextAlignmentOptions.Left, FontStyles.Bold);
        Size(brandText.rectTransform, 90, 26);

        var actions = NewRect("Actions", bar);
        Size(actions, 96, 44);
        var actionGroup = Horizontal(actions, 3, null, TextAnchor.MiddleRight);
        actionGroup.childForceExpandWidth = false;
        var info = IconButton(actions, "Info", theme.iconInfo);
        var settings = IconButton(actions, "Settings", theme.iconSettings);

        var view = bar.gameObject.AddComponent<TopBarView>();
        view.backButton = back;
        view.infoButton = info;
        view.settingsButton = settings;
        view.brandMarks = marks.gameObject;
        view.brandText = brandText;
        view.markImages = markImages;
        return view;
    }

    static Button IconButton(RectTransform parent, string name, Sprite sprite)
    {
        var rect = NewRect(name, parent);
        Size(rect, 42, 42);
        var background = AddImage(rect, null, new Color(1f, 1f, 1f, 0f));
        var icon = NewRect("Icon", rect);
        icon.anchorMin = new Vector2(0.5f, 0.5f);
        icon.anchorMax = new Vector2(0.5f, 0.5f);
        icon.sizeDelta = new Vector2(23, 23);
        var image = AddImage(icon, sprite, Color.white, false);
        image.preserveAspect = true;
        return MakeButton(rect, background);
    }

    // ── 메인 ────────────────────────────────────────────────
    static HomeScreen BuildHomeScreen(RectTransform parent, GameObject actionPrefab, GameObject cardPrefab)
    {
        var root = NewRect("Home Screen", parent);
        Stretch(root);
        var group = Vertical(root, 0, null, TextAnchor.UpperCenter);
        group.childForceExpandWidth = false;

        var hero = NewRect("Hero", root);
        Size(hero, flexibleWidth: true);
        Vertical(hero, 9, new RectOffset(0, 0, 42, 25), TextAnchor.UpperCenter);

        var marks = NewRect("Marks", hero);
        Size(marks, height: 62);
        var markGroup = Horizontal(marks, 10, null, TextAnchor.MiddleCenter);
        markGroup.childForceExpandWidth = false;
        var heroMarks = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var mark = NewRect("Mark " + (i + 1), marks);
            Size(mark, 62, 62);
            heroMarks[i] = AddImage(mark, theme.Token((ShapeType)(i + 1)), Color.white, false);
            heroMarks[i].preserveAspect = true;
        }

        var eyebrow = TextChild(hero, "Eyebrow", "A LITTLE SHAPE OF JOY", 12, theme.muted,
            TextAlignmentOptions.Center, FontStyles.Bold);
        eyebrow.characterSpacing = 12f;
        Size(eyebrow.rectTransform, height: 18);

        var title = TextChild(hero, "Title", "삼각원", 57, theme.ink, TextAlignmentOptions.Center, FontStyles.Bold);
        Size(title.rectTransform, height: 70);

        var subtitle = TextChild(hero, "Subtitle", "같은 도형은 두 개까지.\n작은 퍼즐로 가볍게 쉬어가요.", 15,
            theme.muted, TextAlignmentOptions.Center);
        Size(subtitle.rectTransform, height: 46);

        var menu = NewRect("Menu", root);
        Size(menu, 420);
        Vertical(menu, 12, new RectOffset(0, 0, 10, 0), TextAnchor.UpperCenter);

        var resume = (GameObject)PrefabUtility.InstantiatePrefab(actionPrefab, menu);
        resume.name = "Resume";
        Size((RectTransform)resume.transform, height: 60);

        var classic = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab, menu);
        classic.name = "Classic Card";
        var stages = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab, menu);
        stages.name = "Stages Card";
        var endless = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab, menu);
        endless.name = "Endless Card";

        var spacer = NewRect("Spacer", root);
        var spacerElement = spacer.gameObject.AddComponent<LayoutElement>();
        spacerElement.flexibleHeight = 1;

        var footnote = TextChild(root, "Footnote", "오프라인 플레이 · 기록은 이 기기에 저장돼요", 12,
            theme.muted, TextAlignmentOptions.Center);
        Size(footnote.rectTransform, 560, 24);

        var view = root.gameObject.AddComponent<HomeScreen>();
        view.heroMarks = heroMarks;
        view.eyebrowText = eyebrow;
        view.titleText = title;
        view.subtitleText = subtitle;
        view.footnoteText = footnote;
        view.resumeButton = resume.GetComponent<LabeledButton>();
        view.classicCard = classic.GetComponent<MenuCardView>();
        view.stagesCard = stages.GetComponent<MenuCardView>();
        view.endlessCard = endless.GetComponent<MenuCardView>();
        return view;
    }

    // ── 스테이지 ────────────────────────────────────────────
    static StagesScreen BuildStagesScreen(RectTransform parent, GameObject chapterPrefab)
    {
        var root = NewRect("Stages Screen", parent);
        Stretch(root);
        var scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 26f;

        var viewport = NewRect("Viewport", root);
        Stretch(viewport);
        AddImage(viewport, null, new Color(1f, 1f, 1f, 0.004f));
        viewport.gameObject.AddComponent<RectMask2D>();
        scroll.viewport = viewport;

        var content = NewRect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;
        Vertical(content, 12, new RectOffset(0, 0, 8, 24), TextAnchor.UpperCenter);
        FitVertical(content);
        scroll.content = content;

        var eyebrow = TextChild(content, "Eyebrow", "A JOURNEY OF 81 PUZZLES", 12, theme.muted,
            TextAlignmentOptions.Left, FontStyles.Bold);
        eyebrow.characterSpacing = 12f;
        Size(eyebrow.rectTransform, height: 18);

        var title = TextChild(content, "Title", "한 칸씩, 차근차근.", 31, theme.ink, TextAlignmentOptions.Left, FontStyles.Bold);
        Size(title.rectTransform, height: 42);

        var subtitle = TextChild(content, "Subtitle", "0개 완성 · 다음 퍼즐이 기다리고 있어요.", 13, theme.muted);
        Size(subtitle.rectTransform, height: 22);

        var sections = new ChapterSectionView[4];
        for (int i = 0; i < 4; i++)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(chapterPrefab, content);
            instance.name = $"Chapter {i + 1}";
            var section = instance.GetComponent<ChapterSectionView>();
            section.chapter = i + 1;
            sections[i] = section;
        }

        var view = root.gameObject.AddComponent<StagesScreen>();
        view.eyebrowText = eyebrow;
        view.titleText = title;
        view.subtitleText = subtitle;
        view.sections = sections;
        return view;
    }

    // ── 게임 ────────────────────────────────────────────────
    static GameScreen BuildGameScreen(RectTransform parent, GameObject cellPrefab,
        GameObject actionPrefab, GameObject piecePrefab)
    {
        var root = NewRect("Game Screen", parent);
        Stretch(root);
        Vertical(root, 12, new RectOffset(0, 0, 8, 8), TextAnchor.UpperCenter);

        // 머리말
        var head = NewRect("Head", root);
        Size(head, height: 56);
        Horizontal(head, 8, null, TextAnchor.MiddleLeft);

        var titles = NewRect("Titles", head);
        Size(titles, flexibleWidth: true);
        Vertical(titles, 2, null, TextAnchor.MiddleLeft);
        var meta = TextChild(titles, "Meta", "1장 · 입문", 13, theme.muted);
        Size(meta.rectTransform, height: 18);
        var title = TextChild(titles, "Title", "스테이지 1", 32, theme.ink, TextAlignmentOptions.Left, FontStyles.Bold);
        Size(title.rectTransform, height: 38);

        var timeGroup = NewRect("Time", head);
        Size(timeGroup, 150, 44);
        var timeLayout = Horizontal(timeGroup, 8, null, TextAnchor.MiddleRight);
        timeLayout.childForceExpandWidth = false;
        var timer = TextChild(timeGroup, "Timer", "00:00", 25, theme.ink, TextAlignmentOptions.Right);
        Size(timer.rectTransform, 96, 34);
        var pause = IconButton(timeGroup, "Pause", theme.iconPause);

        // 보드
        var frame = NewRect("Board Frame", root);
        AddImage(frame, null, theme.line, false);
        Vertical(frame, 0, new RectOffset(4, 4, 4, 4), TextAnchor.UpperCenter);
        FitVertical(frame);

        // 3×3 블록 9개를 실제로 중첩한다. 9×9 하나에 균일 간격을 주면
        // 구분선을 덧그려도 결국 한 덩어리로 읽힌다.
        var blocks = NewRect("Blocks", frame);
        AddImage(blocks, null, theme.line, false);          // 블록 사이 굵은 간격의 색
        var gridLayout = blocks.gameObject.AddComponent<GridLayoutGroup>();
        gridLayout.spacing = new Vector2(5, 5);             // 블록 간격 (굵게)
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;
        blocks.gameObject.AddComponent<BoardGrid>().columns = 3;
        FitVertical(blocks);

        var blockParents = new Transform[9];
        for (int b = 0; b < 9; b++)
        {
            var block = NewRect("Block " + (b + 1), blocks);
            AddImage(block, null, theme.cellHairline, false); // 칸 사이 헤어라인 색
            var inner = block.gameObject.AddComponent<GridLayoutGroup>();
            inner.spacing = new Vector2(1, 1);               // 칸 간격 (가늘게)
            inner.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            inner.constraintCount = 3;
            block.gameObject.AddComponent<BoardGrid>().columns = 3;
            blockParents[b] = block;
        }

        var rule = TextChild(root, "Rule", "행 · 열 · 블록마다 같은 도형은 최대 2개", 13, theme.ink, TextAlignmentOptions.Center);
        Size(rule.rectTransform, height: 22);

        // 도형 선택
        var rack = NewRect("Rack", root);
        Size(rack, height: 86);
        var rackGroup = Horizontal(rack, 12, null, TextAnchor.MiddleCenter);
        rackGroup.childForceExpandWidth = false;
        var pieces = new ShapePieceView[5];
        for (int i = 0; i < 5; i++)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(piecePrefab, rack);
            instance.name = "Piece " + (i + 1);
            pieces[i] = instance.GetComponent<ShapePieceView>();
        }

        // 도구
        var tools = NewRect("Tools", root);
        Size(tools, height: 48);
        var toolGroup = Horizontal(tools, 14, null, TextAnchor.MiddleCenter);
        toolGroup.childForceExpandWidth = false;
        var erase = (GameObject)PrefabUtility.InstantiatePrefab(actionPrefab, tools);
        erase.name = "Erase";
        Size((RectTransform)erase.transform, 140, 46);
        var hint = (GameObject)PrefabUtility.InstantiatePrefab(actionPrefab, tools);
        hint.name = "Hint";
        Size((RectTransform)hint.transform, 140, 46);
        var memo = (GameObject)PrefabUtility.InstantiatePrefab(actionPrefab, tools);
        memo.name = "Memo";
        Size((RectTransform)memo.transform, 140, 46);

        // 진행도
        var progress = NewRect("Progress", root);
        Size(progress, height: 4);
        Sliced(AddImage(progress, theme.progressTrack, Color.white, false));
        var fillRect = NewRect("Fill", progress);
        Stretch(fillRect);
        var fill = Sliced(AddImage(fillRect, theme.progressFill, Color.white, false));
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 0f;

        // 꼬리말
        var footer = NewRect("Footer", root);
        Size(footer, height: 22);
        Horizontal(footer, 8, null, TextAnchor.MiddleLeft);
        var footerText = TextChild(footer, "Hint Text", "도형을 고른 뒤 빈칸을 클릭 · 1–5 선택 · Del 지우기", 11, theme.muted);
        Size(footerText.rectTransform, flexibleWidth: true);
        var filled = TextChild(footer, "Filled", "0 / 81", 11, theme.muted, TextAlignmentOptions.Right);
        Size(filled.rectTransform, 80, 20);

        // 퍼즐 엔진
        var engine = new GameObject("Puzzle Engine", typeof(PuzzleGenerator), typeof(BoardManager));
        engine.transform.SetParent(root, false);
        var board = engine.GetComponent<BoardManager>();
        board.cellPrefab = cellPrefab;
        board.boardParent = blocks;
        board.blockParents = blockParents;
        board.gridLayout = gridLayout;
        board.placeOnCellClick = true;
        board.triangleSprite = theme.Token(ShapeType.Triangle);
        board.circleSprite = theme.Token(ShapeType.Circle);
        board.squareSprite = theme.Token(ShapeType.Square);
        board.pentagonSprite = theme.Token(ShapeType.Pentagon);  // 4번 슬롯 = 마름모 비주얼
        board.starSprite = theme.Token(ShapeType.Star);          // 5번 슬롯 = 오각형 비주얼

        var view = root.gameObject.AddComponent<GameScreen>();
        view.board = board;
        view.metaText = meta;
        view.titleText = title;
        view.timerText = timer;
        view.pauseButton = pause;
        view.rack = pieces;
        view.eraseButton = erase.GetComponent<LabeledButton>();
        view.hintButton = hint.GetComponent<LabeledButton>();
        view.memoButton = memo.GetComponent<LabeledButton>();
        view.ruleText = rule;
        view.filledText = filled;
        view.footerText = footerText;
        view.progressFill = fill;
        return view;
    }


    // ── 모달 · 토스트 · 색종이 ──────────────────────────────
    static ModalService BuildModalLayer(RectTransform parent, GameObject actionPrefab)
    {
        var layer = NewRect("Modal Layer", parent);
        Stretch(layer);

        var scrim = NewRect("Scrim", layer);
        Stretch(scrim);
        AddImage(scrim, theme.modalScrim, Color.white);

        var dialogRect = NewRect("Dialog", layer);
        dialogRect.anchorMin = dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.pivot = new Vector2(0.5f, 0.5f);
        dialogRect.sizeDelta = new Vector2(410, 300);
        Sliced(AddImage(dialogRect, theme.panelPopup, theme.paper, false));
        Vertical(dialogRect, 10, new RectOffset(32, 32, 32, 32), TextAnchor.UpperCenter);
        FitVertical(dialogRect);

        var icon = NewRect("Icon", dialogRect);
        Size(icon, 60, 60);
        var iconImage = AddImage(icon, null, Color.white, false);
        iconImage.preserveAspect = true;

        // 높이를 고정하지 않는다. 고정하면 긴 본문이 박스 밖으로 넘쳐(Overflow)
        // 아래 그림·버튼과 겹친다. TMP 는 ILayoutElement 라 스스로 필요한 높이를 보고한다.
        var title = TextChild(dialogRect, "Title", "제목", 28, theme.ink, TextAlignmentOptions.Center, FontStyles.Bold);

        var metric = TextChild(dialogRect, "Metric", "00:00", 48, theme.ink, TextAlignmentOptions.Center, FontStyles.Bold);

        var body = TextChild(dialogRect, "Body", "설명", 14, theme.muted, TextAlignmentOptions.Center);

        // 행 / 열 / 블록 설명 그림 (가로 5:1, 세로 1:5, 정사각 1:1 이라 정사각 칸에 비율 유지로 넣는다)
        var samples = NewRect("Samples", dialogRect);
        Size(samples, height: 104);
        var sampleGroup = Horizontal(samples, 8, null, TextAnchor.MiddleCenter);
        sampleGroup.childForceExpandWidth = false;
        var tutorials = new[] { theme.tutorialRow, theme.tutorialColumn, theme.tutorialBlock };
        var sampleImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            var sample = NewRect("Sample " + (i + 1), samples);
            Size(sample, 104, 104);
            sampleImages[i] = AddImage(sample, tutorials[i], Color.white, false);
            sampleImages[i].preserveAspect = true;
        }

        var invalid = NewRect("Invalid Example", dialogRect);
        Size(invalid, 208, 92);
        var invalidImage = AddImage(invalid, theme.tutorialInvalid, Color.white, false);
        invalidImage.preserveAspect = true;

        var actions = NewRect("Actions", dialogRect);
        Vertical(actions, 10, new RectOffset(0, 0, 15, 0), TextAnchor.UpperCenter);
        FitVertical(actions);

        var view = dialogRect.gameObject.AddComponent<DialogView>();
        view.panel = dialogRect.GetComponent<Image>();
        view.iconImage = iconImage;
        view.titleText = title;
        view.bodyText = body;
        view.metricText = metric;
        view.actionsRoot = actions;
        view.actionPrefab = actionPrefab.GetComponent<LabeledButton>();
        view.sampleRow = samples;
        view.sampleImages = sampleImages;
        view.invalidImage = invalidImage;

        // 컴포넌트는 항상 켜져 있는 캔버스에 둔다. 레이어 자신에 붙이면
        // 레이어가 꺼져 있는 동안 Awake 가 돌지 않아 생명주기가 꼬인다.
        var service = parent.gameObject.AddComponent<ModalService>();
        service.root = layer.gameObject;
        service.dialog = view;
        layer.gameObject.SetActive(false);
        return service;
    }

    static ToastView BuildToast(RectTransform parent)
    {
        var rect = NewRect("Toast", parent);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 28f);
        rect.sizeDelta = new Vector2(540, 48);
        var background = Sliced(AddImage(rect, theme.panelToast, Color.white, false));
        var group = rect.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        var label = TextChild(rect, "Label", "", 13, theme.ink, TextAlignmentOptions.Center);
        Stretch(label.rectTransform);

        var view = rect.gameObject.AddComponent<ToastView>();
        view.group = group;
        view.label = label;
        view.background = background;
        return view;
    }

    static ConfettiView BuildConfetti(RectTransform parent, GameObject piecePrefab)
    {
        var rect = NewRect("Confetti", parent);
        Stretch(rect);
        var view = rect.gameObject.AddComponent<ConfettiView>();
        view.root = rect;
        view.piecePrefab = piecePrefab.GetComponent<Image>();
        return view;
    }

    static ChapterTransitionView BuildChapterTransition(RectTransform parent)
    {
        // 항상 켜 두고 CanvasGroup 알파로만 보였다 사라진다.
        var rect = NewRect("Chapter Transition", parent);
        Stretch(rect);
        var background = AddImage(rect, null, theme.mint);
        var group = rect.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        var column = NewRect("Text", rect);
        column.anchorMin = new Vector2(0f, 0.5f);
        column.anchorMax = new Vector2(1f, 0.5f);
        column.pivot = new Vector2(0.5f, 0.5f);
        column.anchoredPosition = Vector2.zero;
        column.sizeDelta = new Vector2(0f, 240f);
        Vertical(column, 10, new RectOffset(40, 40, 0, 0), TextAnchor.MiddleCenter);

        var iconRect = NewRect("Icon", column);
        Size(iconRect, 72, 72);
        var iconImage = AddImage(iconRect, theme.ChapterIcon(2), Color.white, false);
        iconImage.preserveAspect = true;

        var title = TextChild(column, "Title", "2장 · 집중", 44, theme.ink, TextAlignmentOptions.Center, FontStyles.Bold);
        Size(title.rectTransform, height: 60);
        var tagline = TextChild(column, "Tagline", "", 16, theme.muted, TextAlignmentOptions.Center);
        Size(tagline.rectTransform, height: 26);

        var view = rect.gameObject.AddComponent<ChapterTransitionView>();
        view.group = group;
        view.background = background;
        view.icon = iconImage;
        view.titleText = title;
        view.taglineText = tagline;
        return view;
    }
}
