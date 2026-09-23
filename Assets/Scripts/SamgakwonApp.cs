using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Portrait uGUI presentation of the approved HTML prototype, using the project's puzzle engine.</summary>
public class SamgakwonApp : MonoBehaviour
{
    static readonly Color Paper = Hex("FFFBF3"), Ink = Hex("484B57"), Teal = Hex("08AAA5"), Muted = Hex("938D83");
    static readonly string[] Shapes = { "triangle", "circle", "square", "diamond", "pentagon" };
    static readonly string[] Difficulties = { "쉬움", "보통", "어려움" };
    readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    readonly List<Image> rack = new List<Image>();
    RectTransform page, screen, modal;
    Font font;
    BoardManager board;
    Text timer, filled, toast;
    Image progress;
    AudioSource music;
    float elapsed, saveClock;
    bool won, focused = true;
    int chapter = 1;
    Coroutine toastRoutine;
    Action dismiss;

    void Awake()
    {
        Time.timeScale = 1;
        if (GameManager.Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
        if (RecordManager.Instance == null) new GameObject("RecordManager").AddComponent<RecordManager>();
        if (SFXManager.Instance == null) new GameObject("SFXManager").AddComponent<SFXManager>();
        font = Resources.Load<Font>("Fonts/NanumGothic-Regular");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (Camera.main == null)
        {
            var cameraObject = new GameObject("UI Camera", typeof(Camera));
            cameraObject.transform.SetParent(transform, false);
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("EEE9E1");
            camera.orthographic = true;
            camera.cullingMask = 0;
        }
        if (FindFirstObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
        if (EventSystem.current == null)
        {
            var events = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            events.AddComponent<StandaloneInputModule>();
#endif
        }
        var canvasObject = new GameObject("Samgakwon Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(680, 1000);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var backdrop = Box(canvasObject.transform, "Background", Hex("EEE9E1"));
        Stretch(backdrop.rectTransform);
        page = Rect(canvasObject.transform, "Portrait Page", 0, 0, 680, 1000);
        page.anchorMin = page.anchorMax = page.pivot = new Vector2(0.5f, 0.5f);
        page.anchoredPosition = Vector2.zero;
        var paper = Box(page, "Paper", Paper); Stretch(paper.rectTransform);
        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.clip = SFXManager.MakeTone(new float[] { 261.63f, 329.63f, 392, 329.63f, 220, 261.63f, 329.63f, 261.63f, 174.61f, 220, 261.63f, 220, 196, 246.94f, 293.66f, 246.94f }, 0.6f, 0.6f, 0.06f);
        UpdateMusic();
        Home();
    }

    void Update()
    {
        if (board != null && !won && modal == null && focused)
        {
            elapsed += Time.unscaledDeltaTime;
            saveClock += Time.unscaledDeltaTime;
            if (timer != null) timer.text = FormatTime(elapsed);
            if (saveClock > 5) { Save(); saveClock = 0; }
        }
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (modal != null) dismiss?.Invoke();
            else if (board != null && !won) Pause();
            else Home();
        }
        if (board == null || won || modal != null || !focused) return;
        for (int i = 0; i < 5; i++)
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) board.SelectShape(i + 1);
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace)) board.ClearCell();
#endif
    }

    void OnApplicationFocus(bool value)
    {
        focused = value;
        if (!value) { Save(); if (board != null && !won && modal == null) Pause(); }
    }
    void OnApplicationPause(bool pause) { if (pause) { Save(); if (board != null && !won && modal == null) Pause(); } }
    void OnApplicationQuit() => Save();
    void Save() { if (board != null && board.IsReady && !won) PuzzleSave.Store(board, elapsed); }

    void NewScreen(string title, string subtitle = null)
    {
        Save();
        CloseModal();
        if (screen != null) { screen.gameObject.SetActive(false); Destroy(screen.gameObject); }
        board = null; rack.Clear(); timer = null; filled = null; progress = null; won = false;
        screen = Rect(page, "Screen", 0, 0, 680, 1000);
        // A restrained header follows the light, casual reference.
        for (int i = 0; i < 3; i++) Picture(screen, "tokens/" + Shapes[i] + "_normal", 28 + i * 24, 27, 21, 21);
        Label(screen, "삼각원", 108, 24, 150, 30, 20);
        IconButton(screen, "info", 548, 20, Help);
        IconButton(screen, "settings", 600, 20, Settings);
        if (!string.IsNullOrEmpty(title))
        {
            IconButton(screen, "back", 28, 83, Home);
            Label(screen, title, 84, 87, 550, 50, 32);
            if (subtitle != null) Label(screen, subtitle, 34, 148, 612, 40, 15, Muted);
        }
    }

    public void Home()
    {
        NewScreen(null);
        for (int i = 0; i < 5; i++) Picture(screen, "tokens/" + Shapes[i] + "_normal", 170 + i * 69, 138, 62, 62);
        Label(screen, "삼각원", 40, 219, 600, 87, 60, Ink, TextAnchor.MiddleCenter);
        Label(screen, "작은 도형, 즐거운 생각", 40, 308, 600, 36, 21, Muted, TextAnchor.MiddleCenter);
        var saved = PuzzleSave.Load();
        if (saved != null) Button(screen, "이어하기", 130, 390, 420, 62, Resume, true);
        Card("클래식", "내 속도에 맞는 세 가지 난이도", "triangle", 472, Classic);
        Card("스테이지", "네 개의 챕터, 81개의 작은 도전", "circle", 575, Stages);
        bool unlocked = GameManager.IsEndlessUnlocked() || StageSelectManager.IsStageClear(81);
        var endless = Card("엔드리스", unlocked ? $"누적 {GameManager.GetEndlessClearCount()}판 완성" : "81단계 클리어 후 열려요", "pentagon", 678,
            () => StartGame(GameMode.Endless, 1, GameDifficulty.Hard));
        endless.interactable = unlocked;
        Label(screen, "서두르지 않아도 괜찮아요. 한 칸씩 채워보세요.", 40, 839, 600, 34, 16, Muted, TextAnchor.MiddleCenter);
    }

    Button Card(string title, string description, string token, float y, Action action)
    {
        var button = Button(screen, "", 130, y, 420, 89, action);
        Picture(button.transform, "tokens/" + token + "_normal", 22, 24, 39, 39);
        Label(button.transform, title, 81, 14, 300, 32, 23);
        Label(button.transform, description, 81, 50, 308, 22, 14, Muted);
        return button;
    }

    public void Classic()
    {
        NewScreen("클래식", "빈칸의 수만 달라져요. 모든 난이도는 무작위 배치입니다.");
        int[] blanks = { 15, 35, 55 };
        for (int i = 0; i < 3; i++)
        {
            int difficulty = i;
            var b = Button(screen, "", 60, 270 + i * 145, 560, 117, () => StartGame(GameMode.Classic, 1, (GameDifficulty)difficulty));
            Picture(b.transform, "tokens/" + Shapes[i] + "_normal", 25, 28, 55, 55);
            Label(b.transform, Difficulties[i], 107, 22, 390, 36, 28);
            float best = RecordManager.Instance.GetClassicRecord((GameDifficulty)i);
            Label(b.transform, $"빈칸 {blanks[i]}개 · 최고 기록 {(best > 0 ? FormatTime(best) : "아직 없어요")}", 107, 68, 420, 26, 17, Muted);
        }
    }

    public void Stages()
    {
        NewScreen("스테이지", "같은 규칙으로 만나는 네 가지 배치. 천천히 도전해 보세요.");
        for (int c = 1; c <= 4; c++)
        {
            int selectedChapter = c;
            Button(screen, $"{c}장", 34 + (c - 1) * 155, 208, 145, 52, () => { chapter = selectedChapter; Stages(); }, c == chapter);
        }
        var info = Chapters.GetInfo(chapter);
        Label(screen, info.title, 38, 299, 602, 48, 29);
        Label(screen, info.description, 38, 350, 604, 89, 17, Muted);
        int unlocked = Mathf.Clamp(PlayerPrefs.GetInt("UnlockedStages", 1), 1, 81);
        for (int stage = info.startStage; stage <= info.endStage; stage++)
        {
            int number = stage, index = stage - info.startStage;
            bool cleared = StageSelectManager.IsStageClear(stage);
            var button = Button(screen, number.ToString(), 38 + index % 5 * 123, 459 + index / 5 * 90, 111, 78,
                () => StartGame(GameMode.Stage, number, GameDifficulty.Normal));
            button.interactable = stage <= unlocked;
            button.image.color = cleared ? Hex("BCE7DC") : stage == unlocked ? Hex("FFAD98") : Color.white;
            if (stage > unlocked) Picture(button.transform, "icons/lock_dark", 81, 8, 15, 15);
            if (cleared) Picture(button.transform, "icons/check_teal", 81, 8, 17, 17);
        }
        Label(screen, "민트색: 완료    살구색: 진행 중    자물쇠: 아직 잠김", 38, 941, 604, 28, 15, Muted, TextAnchor.MiddleCenter);
    }

    void Resume()
    {
        var save = PuzzleSave.Load();
        if (save != null) StartGame(save.mode, save.stage, save.difficulty, save);
        else Home();
    }

    public void StartGame(GameMode mode, int stage, GameDifficulty difficulty, PuzzleSave saved = null)
    {
        if (mode == GameMode.Endless && !GameManager.IsEndlessUnlocked() && !StageSelectManager.IsStageClear(81)) return;
        NewScreen(null);
        var gm = GameManager.Instance;
        gm.currentMode = mode; gm.currentDifficulty = difficulty; gm.currentStage = Mathf.Clamp(stage, 1, 81);
        elapsed = saved?.elapsed ?? 0; saveClock = 0;
        string title = mode == GameMode.Classic ? "클래식 · " + Difficulties[(int)difficulty] : mode == GameMode.Stage ? $"스테이지 {stage}" : "엔드리스";
        string subtitle = mode == GameMode.Stage ? Chapters.GetInfoForStage(stage).title : mode == GameMode.Endless ? $"누적 {GameManager.GetEndlessClearCount()}판 완성" : "조금씩 채워지는 즐거움";
        Label(screen, subtitle, 34, 85, 480, 26, 16, Muted);
        Label(screen, title, 34, 119, 430, 45, 32);
        timer = Label(screen, FormatTime(elapsed), 482, 115, 102, 48, 25, Ink, TextAnchor.MiddleRight);
        IconButton(screen, "pause", 596, 117, Pause);
        var frame = Box(screen, "Board Frame", Hex("D2C8B9")); Position(frame.rectTransform, 34, 190, 612, 612);
        var gridRect = Rect(frame.transform, "Cells", 5, 5, 602, 602);
        var grid = gridRect.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(66, 66); grid.spacing = Vector2.one;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 9;
        var engine = new GameObject("Puzzle Engine"); engine.transform.SetParent(screen, false);
        engine.AddComponent<RuleChecker>(); engine.AddComponent<PuzzleGenerator>();
        board = engine.AddComponent<BoardManager>();
        var template = Rect(engine.transform, "Cell Template", 0, 0, 66, 66);
        var cell = template.gameObject.AddComponent<Cell>();
        cell.backgroundImage = template.gameObject.AddComponent<Image>();
        cell.normalColor = Paper; cell.initialColor = Hex("F9F4E9"); cell.selectedColor = Hex("BCEDE2"); cell.editableHighlight = Hex("E7F7F2");
        var shape = Box(template, "Shape", Color.white); Stretch(shape.rectTransform); shape.raycastTarget = false; shape.preserveAspect = true;
        cell.shapeImage = shape;
        cell.memoContainer = Rect(template, "Memos", 0, 0, 66, 66);
        template.gameObject.SetActive(false);
        board.cellPrefab = template.gameObject; board.boardParent = gridRect; board.gridLayout = grid; board.placeOnCellClick = true;
        board.triangleSprite = Sprite("tokens/triangle_normal"); board.circleSprite = Sprite("tokens/circle_normal");
        board.squareSprite = Sprite("tokens/square_normal");
        // Serialized legacy slots 4/5 retain their numeric IDs; the approved visuals are diamond/pentagon.
        board.pentagonSprite = Sprite("tokens/diamond_normal"); board.starSprite = Sprite("tokens/pentagon_normal");
        board.InitializeBoard();
        if (saved != null) board.Restore(PuzzleSave.Expand(saved.initial), PuzzleSave.Expand(saved.board), PuzzleSave.Expand(saved.solution));
        for (int i = 1; i <= 2; i++)
        {
            var vertical = Box(frame.transform, "Block Divider", Hex("D2C8B9")); Position(vertical.rectTransform, 3 + i * 201, 0, 4, 612); vertical.raycastTarget = false;
            var horizontal = Box(frame.transform, "Block Divider", Hex("D2C8B9")); Position(horizontal.rectTransform, 0, 3 + i * 201, 612, 4); horizontal.raycastTarget = false;
        }
        Label(screen, "행 · 열 · 블록마다 같은 도형은 최대 2개", 34, 811, 612, 26, 17, Ink, TextAnchor.MiddleCenter);
        for (int i = 0; i < 5; i++)
        {
            int value = i + 1;
            var button = Button(screen, "", 124 + i * 89, 849, 76, 73, () => board.SelectShape(value));
            Picture(button.transform, "tokens/" + Shapes[i] + "_normal", 16, 6, 44, 44);
            Label(button.transform, value.ToString(), 0, 48, 76, 20, 12, Muted, TextAnchor.MiddleCenter);
            rack.Add(button.image);
        }
        Button(screen, "지우기", 178, 934, 153, 44, () => board.ClearCell());
        Button(screen, "힌트", 349, 934, 153, 44, () => board.UseHint());
        filled = Label(screen, "", 532, 944, 114, 28, 14, Muted, TextAnchor.MiddleRight);
        progress = Box(screen, "Progress", Teal); Position(progress.rectTransform, 34, 990, 0, 3);
        board.Changed += BoardChanged; board.Completed += Complete; board.Feedback += Toast;
        BoardChanged();
        if (PlayerPrefs.GetInt("TutorialDone_v1", 0) == 0) ShowTutorial(ChapterIntro);
        else ChapterIntro();
    }

    void BoardChanged()
    {
        if (board == null) return;
        for (int i = 0; i < rack.Count; i++) rack[i].color = i + 1 == (int)board.SelectedShape ? Hex("BCEDE2") : Color.white;
        int count = 0;
        foreach (var value in board.GetBoard()) if (value != ShapeType.None) count++;
        if (filled != null) filled.text = $"{count} / 81";
        if (progress != null) progress.rectTransform.sizeDelta = new Vector2(612 * count / 81f, 3);
        Save();
    }

    void Complete()
    {
        if (won) return;
        won = true; board.InputEnabled = false;
        var gm = GameManager.Instance;
        bool endless = gm.currentMode == GameMode.Endless;
        bool record = !endless && RecordManager.Instance.IsNewRecord(elapsed);
        if (record) SFXManager.PlayNewRecord(); else SFXManager.PlayClear();
        Celebrate();
        if (endless)
        {
            int count = GameManager.IncrementEndlessClearCount();
            Toast($"완성! 누적 {count}판 · 다음 퍼즐로 이어집니다");
            StartCoroutine(NextEndless(board));
            return;
        }
        RecordManager.Instance.SaveRecord(elapsed);
        PuzzleSave.Clear();
        if (gm.currentMode == GameMode.Stage)
        {
            StageSelectManager.UnlockStage(gm.currentStage);
            if (gm.currentStage == 81) GameManager.UnlockEndless();
        }
        bool final = gm.currentMode == GameMode.Stage && gm.currentStage == 81;
        Dialog(record ? "신기록!" : "퍼즐 완성!", FormatTime(elapsed) + "\n\n" + (final ? "81개의 퍼즐을 모두 완성했어요.\n엔드리스 모드가 열렸어요!" : "작은 도형으로 완성한 한 판."), record);
        var mode = gm.currentMode; int stage = gm.currentStage; var difficulty = gm.currentDifficulty;
        ModalButton(final ? "엔드리스 시작" : mode == GameMode.Stage ? "다음 스테이지" : "한 판 더", 318,
            () => StartGame(final ? GameMode.Endless : mode, mode == GameMode.Stage ? stage + 1 : 1, difficulty), true);
        ModalButton(mode == GameMode.Stage ? "스테이지 선택" : "메인으로", 384, mode == GameMode.Stage ? (Action)Stages : Home);
        dismiss = mode == GameMode.Stage ? (Action)Stages : Home;
    }
    IEnumerator NextEndless(BoardManager completedBoard)
    {
        yield return new WaitForSecondsRealtime(1.2f);
        if (board == completedBoard && won) StartGame(GameMode.Endless, 1, GameDifficulty.Hard);
    }

    void ChapterIntro()
    {
        var gm = GameManager.Instance;
        if (gm.currentMode != GameMode.Stage || !Chapters.IsChapterStart(gm.currentStage)) return;
        int c = Chapters.GetChapter(gm.currentStage);
        if (c <= 1 || PlayerPrefs.GetInt("ChapterIntro_" + c, 0) == 1) return;
        var info = Chapters.GetInfo(c);
        Dialog(info.title, info.description);
        Action done = () => { PlayerPrefs.SetInt("ChapterIntro_" + c, 1); PlayerPrefs.Save(); CloseModal(); };
        ModalButton("시작하기", 344, done, true); dismiss = done;
    }

    void Pause()
    {
        if (board == null || won) return;
        Save();
        Dialog("잠깐 쉬어가요.", "시간도 잠시 멈춰둘게요.");
        ModalButton("계속하기", 264, CloseModal, true);
        ModalButton("다시 시작", 330, () =>
        {
            Dialog("다시 시작할까요?", "이 판에 놓은 도형과 시간이 초기화돼요.");
            ModalButton("다시 시작", 318, () => { elapsed = 0; board.RestartPuzzle(); CloseModal(); }, true);
            ModalButton("취소", 384, Pause); dismiss = Pause;
        });
        ModalButton("메인으로", 396, Home);
    }
    void Help() => ShowTutorial(null);
    void ShowTutorial(Action next)
    {
        Dialog("같은 도형은 두 개까지.", "가로 한 줄, 세로 한 줄, 3×3 블록마다\n같은 도형을 최대 2개까지 놓을 수 있어요.\n\n도형을 고른 뒤 빈칸을 클릭하세요.\n처음 놓인 도형은 고정된 단서예요.\n규칙에 맞게 모든 칸을 채우면 완성!");
        Action done = () => { PlayerPrefs.SetInt("TutorialDone_v1", 1); PlayerPrefs.Save(); CloseModal(); next?.Invoke(); };
        ModalButton("알겠어요", 344, done, true); ModalButton("건너뛰기", 410, done); dismiss = done;
    }
    void Settings()
    {
        Dialog("편안하게 즐기기", "설정과 기록은 이 기기에 저장돼요.");
        SettingButton("효과음", "SamgakwonSound", 208);
        SettingButton("배경음악", "SamgakwonMusic", 270, 0);
        SettingButton("작은 움직임", "SamgakwonMotion", 332);
        ModalButton("완료", 410, CloseModal, true);
    }
    void SettingButton(string label, string key, float y, int fallback = 1)
    {
        bool enabled = PlayerPrefs.GetInt(key, fallback) == 1;
        ModalButton(label + (enabled ? "   켜짐" : "   꺼짐"), y, () =>
        { PlayerPrefs.SetInt(key, enabled ? 0 : 1); PlayerPrefs.Save(); UpdateMusic(); Settings(); });
    }
    void UpdateMusic()
    {
        if (music == null) return;
        if (PlayerPrefs.GetInt("SamgakwonMusic", 0) == 1) { if (!music.isPlaying) music.Play(); }
        else music.Stop();
    }

    void Dialog(string title, string body, bool gold = false)
    {
        CloseModal();
        if (board != null) board.InputEnabled = false;
        modal = Rect(page, "Modal", 0, 0, 680, 1000);
        var scrim = Box(modal, "Scrim", new Color(0.2f, 0.23f, 0.26f, 0.48f)); Stretch(scrim.rectTransform);
        var panel = Box(modal, "Dialog", gold ? Hex("FFF5CF") : Paper);
        Position(panel.rectTransform, 80, 235, 520, 510);
        panel.sprite = Sprite("panels/popup"); panel.type = Image.Type.Sliced;
        Label(panel.transform, title, 26, 35, 468, 55, 30, Ink, TextAnchor.MiddleCenter);
        Label(panel.transform, body, 26, 100, 468, 195, 18, Muted, TextAnchor.MiddleCenter);
        dismiss = CloseModal;
    }
    void ModalButton(string label, float y, Action action, bool primary = false) => Button(modal.GetChild(1), label, 35, y, 450, 54, action, primary);
    void CloseModal()
    {
        if (modal != null) { modal.gameObject.SetActive(false); Destroy(modal.gameObject); modal = null; }
        dismiss = null;
        if (board != null) board.InputEnabled = !won;
    }
    void Toast(string message)
    {
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        if (toast != null) Destroy(toast.transform.parent.gameObject);
        var box = Box(page, "Toast", Ink); Position(box.rectTransform, 60, 732, 560, 66);
        toast = Label(box.transform, message, 16, 7, 528, 52, 17, Color.white, TextAnchor.MiddleCenter);
        box.raycastTarget = false;
        toastRoutine = StartCoroutine(HideToast(box.gameObject));
    }
    IEnumerator HideToast(GameObject obj) { yield return new WaitForSecondsRealtime(2.8f); if (obj != null) Destroy(obj); }
    void Celebrate()
    {
        if (PlayerPrefs.GetInt("SamgakwonMotion", 1) == 0) return;
        StartCoroutine(Confetti());
    }
    IEnumerator Confetti()
    {
        var pieces = new List<RectTransform>();
        for (int i = 0; i < 28; i++)
        {
            var piece = Picture(page, "tokens/" + Shapes[i % 5] + "_normal", UnityEngine.Random.Range(70, 610), UnityEngine.Random.Range(160, 320), 18, 18);
            pieces.Add(piece.rectTransform);
        }
        float time = 0;
        while (time < 1.2f)
        {
            time += Time.unscaledDeltaTime;
            foreach (var piece in pieces)
            {
                piece.anchoredPosition += new Vector2(Mathf.Sin(time * 5 + piece.GetSiblingIndex()) * 35, -350) * Time.unscaledDeltaTime;
                piece.Rotate(0, 0, Time.unscaledDeltaTime * 180);
            }
            yield return null;
        }
        foreach (var piece in pieces) Destroy(piece.gameObject);
    }

    Sprite Sprite(string path)
    {
        if (sprites.TryGetValue(path, out var sprite)) return sprite;
        // Existing assets use multiple-sprite import; LoadAll supports those without changing their GUIDs.
        var assets = Resources.LoadAll<Sprite>("samgakwon-assets-v1/png/" + path);
        sprite = assets.Length > 0 ? assets[0] : null;
        if (sprite == null) Debug.LogWarning("Missing UI sprite: " + path);
        sprites[path] = sprite;
        return sprite;
    }
    static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
    public static string FormatTime(float seconds) => $"{(int)seconds / 60:00}:{(int)seconds % 60:00}";
    RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false); Position(rect, x, y, w, h); return rect;
    }
    static void Position(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
    }
    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
    Image Box(Transform parent, string name, Color color)
    {
        var image = Rect(parent, name, 0, 0, 100, 100).gameObject.AddComponent<Image>(); image.color = color; return image;
    }
    Image Picture(Transform parent, string path, float x, float y, float w, float h)
    {
        var image = Box(parent, path, Color.white); Position(image.rectTransform, x, y, w, h);
        image.sprite = Sprite(path); image.preserveAspect = true; image.raycastTarget = false; return image;
    }
    Text Label(Transform parent, string value, float x, float y, float w, float h, int size, Color? color = null, TextAnchor alignment = TextAnchor.MiddleLeft)
    {
        var text = Rect(parent, value, x, y, w, h).gameObject.AddComponent<Text>();
        text.font = font; text.text = value; text.fontSize = size; text.color = color ?? Ink;
        text.alignment = alignment; text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }
    Button Button(Transform parent, string label, float x, float y, float w, float h, Action action, bool primary = false)
    {
        var image = Box(parent, string.IsNullOrEmpty(label) ? "Button" : label, Color.white);
        Position(image.rectTransform, x, y, w, h);
        image.sprite = Sprite(primary ? "buttons/primary_normal" : "buttons/secondary_normal"); image.type = Image.Type.Sliced;
        var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        var colors = button.colors; colors.highlightedColor = Hex("E7F7F2"); colors.pressedColor = Hex("BCE7DC");
        colors.disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.48f); button.colors = colors;
        button.onClick.AddListener(() => action());
        if (label.Length > 0) Label(button.transform, label, 12, 0, w - 24, h, 20, primary ? Color.white : Ink, TextAnchor.MiddleCenter);
        return button;
    }
    void IconButton(Transform parent, string icon, float x, float y, Action action)
    {
        var button = Button(parent, "", x, y, 44, 44, action);
        button.image.sprite = null; button.image.color = Color.clear;
        Picture(button.transform, "icons/" + icon + "_dark", 10, 10, 24, 24);
    }
}
