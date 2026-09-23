using System;
using UnityEngine;

/// <summary>
/// 화면 전환과 앱 전역 동작을 담당한다.
/// 화면은 파괴·재생성하지 않고 활성/비활성으로만 오간다.
/// </summary>
public class SamgakwonRouter : MonoBehaviour
{
    [Header("Design")]
    [SerializeField] SamgakwonTheme theme;

    [Header("Chrome")]
    [SerializeField] TopBarView topBar;
    [SerializeField] ModalService modals;
    [SerializeField] ToastView toast;
    [SerializeField] ConfettiView confetti;
    [SerializeField] ChapterTransitionView chapterTransition;
    [SerializeField] BackgroundClickCatcher backgroundCatcher;
    [SerializeField] AudioSource music;
    [Tooltip("씬에 배치한 효과음 매니저. 제작된 오디오 클립을 여기에 꽂는다. " +
             "비워 두면 런타임에 만들고 합성 톤으로 대체한다.")]
    [SerializeField] SFXManager sfx;

    [Header("Screens")]
    [SerializeField] HomeScreen homeScreen;
    [SerializeField] StagesScreen stagesScreen;
    [SerializeField] GameScreen gameScreen;

    public SamgakwonTheme Theme => theme;
    public ModalService Modals => modals;

    /// <summary>모달이나 전환 연출이 화면을 덮고 있는가. 타이머·입력을 멈출 기준.</summary>
    public bool IsOverlayOpen =>
        (modals != null && modals.IsOpen) || (chapterTransition != null && chapterTransition.IsPlaying);

    UIScreen current;

    const string KeySound = "SamgakwonSound";
    const string KeyMusic = "SamgakwonMusic";
    const string KeyMotion = "SamgakwonMotion";
    const string KeyTutorial = "TutorialDone_v1";

    void Awake()
    {
        EnsureManagers();
        if (modals != null) modals.SetTheme(theme);
        if (topBar != null) topBar.Bind(this);
        if (homeScreen != null) homeScreen.Bind(this);
        if (stagesScreen != null) stagesScreen.Bind(this);
        if (gameScreen != null) gameScreen.Bind(this);
        if (backgroundCatcher != null)
            backgroundCatcher.Clicked = () => { if (gameScreen != null) gameScreen.ClearCellSelection(); };

        if (homeScreen != null) homeScreen.gameObject.SetActive(false);
        if (stagesScreen != null) stagesScreen.gameObject.SetActive(false);
        if (gameScreen != null) gameScreen.gameObject.SetActive(false);

        if (music != null)
        {
            music.playOnAwake = false;
            music.loop = true;
            music.clip = SFXManager.MakeTone(new float[]
            {
                261.63f, 329.63f, 392f, 329.63f, 220f, 261.63f, 329.63f, 261.63f,
                174.61f, 220f, 261.63f, 220f, 196f, 246.94f, 293.66f, 246.94f
            }, 0.6f, 0.6f, 0.06f);
        }
        UpdateMusic();

        // 씬에 이미 있는 버튼 전체에 클릭음을 붙인다.
        // (빌더를 다시 돌리지 않은 씬도 이걸로 동작한다)
        ButtonClickSound.BindAll(gameObject);
    }

    void Start() => ShowHome();

    void EnsureManagers()
    {
        if (GameManager.Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
        if (RecordManager.Instance == null) new GameObject("RecordManager").AddComponent<RecordManager>();
        // 씬에 배치된 SFXManager 가 있으면 그것을 쓴다. 여기서 새로 만들면
        // Awake 순서에 따라 인스펙터에서 클립을 꽂아 둔 쪽이 파괴될 수 있다.
        if (SFXManager.Instance == null && sfx == null) new GameObject("SFXManager").AddComponent<SFXManager>();
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (modals != null && modals.IsOpen) modals.Dismiss();
        else if (current == gameScreen && gameScreen != null) gameScreen.Pause();
        else ShowHome();
#endif
    }

    // ── 화면 전환 ───────────────────────────────────────────
    void Show(UIScreen target)
    {
        if (target == null) return;
        if (current != null && current != target) current.SetVisible(false);
        current = target;
        target.SetVisible(true);
        // OnShow 에서 스테이지 버튼 등이 새로 만들어지므로 그 뒤에 훑는다.
        ButtonClickSound.BindAll(target.gameObject);
        if (topBar != null) topBar.ShowBack(target != (UIScreen)homeScreen);
    }

    public void ShowHome()
    {
        if (gameScreen != null) gameScreen.EndSession();
        if (modals != null) modals.Close();
        Show(homeScreen);
    }

    public void ShowStages()
    {
        if (gameScreen != null) gameScreen.EndSession();
        if (modals != null) modals.Close();
        Show(stagesScreen);
    }

    /// <summary>상단바 뒤로가기. 모달이 열려 있으면 모달부터 닫는다.</summary>
    public void Back()
    {
        if (modals != null && modals.IsOpen) { modals.Dismiss(); return; }
        ShowHome();
    }

    public void StartGame(GameMode mode, int stage, GameDifficulty difficulty, PuzzleSave saved = null)
    {
        if (mode == GameMode.Endless && !GameManager.IsEndlessUnlocked()
            && !StageSelectManager.IsStageClear(Chapters.TotalStages)) return;
        if (gameScreen == null) return;

        if (modals != null) modals.Close();
        Show(gameScreen);
        gameScreen.StartSession(mode, stage, difficulty, saved);

        if (PlayerPrefs.GetInt(KeyTutorial, 0) == 0) ShowTutorial(PlayChapterTransition);
        else PlayChapterTransition();
    }

    /// <summary>
    /// 챕터 경계(21 · 41 · 61)에 들어설 때마다 짧은 전환 연출을 재생하고,
    /// 연출이 끝나면 아직 안 본 챕터라면 안내 팝업을 띄운다.
    /// </summary>
    void PlayChapterTransition()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.currentMode != GameMode.Stage || !Chapters.IsChapterStart(gm.currentStage))
            return;
        int chapter = Chapters.GetChapter(gm.currentStage);
        if (chapter <= 1) { ShowChapterIntro(); return; } // 1장은 기본 규칙 안내가 대신한다

        var info = Chapters.GetInfo(chapter);
        if (chapterTransition != null) chapterTransition.Play(info, theme, ShowChapterIntro);
        else ShowChapterIntro();
    }

    public void Resume()
    {
        var saved = PuzzleSave.Load();
        if (saved != null) StartGame(saved.mode, saved.stage, saved.difficulty, saved);
        else ShowHome();
    }

    // ── 모달 ────────────────────────────────────────────────
    public void ShowClassicPicker()
    {
        if (modals == null) return;
        var request = new ModalRequest("오늘은 어떤 한 판?", "모든 난이도에서 빈칸은 무작위로 배치돼요.")
            .Dismiss(modals.Close);
        for (int i = 0; i < 3; i++)
        {
            var difficulty = (GameDifficulty)i;
            float best = RecordManager.Instance != null ? RecordManager.Instance.GetClassicRecord(difficulty) : 0f;
            string record = best > 0f ? $"   최고 {SamgakwonFormat.Time(best)}" : "";
            request.Button($"{GameScreen.Difficulty(difficulty)} · {StageBalance.ClassicBlanks(difficulty)}칸{record}",
                () => StartGame(GameMode.Classic, 1, difficulty));
        }
        request.Button("돌아가기", modals.Close);
        modals.Show(request);
    }

    public void ShowHelp() => ShowTutorial(null);

    /// <summary>
    /// 한 번에 다 읽히지 않는 긴 글 대신 3단계로 나눠 보여 준다.
    /// 어느 단계에서든 건너뛰기로 빠져나갈 수 있다.
    /// </summary>
    const int TutorialSteps = 3;

    void ShowTutorial(Action next) => ShowTutorialStep(0, next);

    void ShowTutorialStep(int step, Action next)
    {
        if (modals == null) { next?.Invoke(); return; }

        Action finish = () =>
        {
            PlayerPrefs.SetInt(KeyTutorial, 1);
            PlayerPrefs.Save();
            modals.Close();
            next?.Invoke();
        };

        ModalRequest request;
        switch (step)
        {
            case 0:
                request = new ModalRequest("같은 도형은 두 개까지",
                    "가로 한 줄, 세로 한 줄, 굵은 선 안의 3×3 블록.\n" +
                    "각 구역에 같은 도형은 최대 2개까지 놓을 수 있어요.")
                    .Icon(theme != null ? theme.iconSparkle : null)
                    .Samples();
                break;
            case 1:
                request = new ModalRequest("세 번째는 놓을 수 없어요",
                    "규칙을 어기는 자리에는 도형이 놓이지 않아요.\n" +
                    "칸이 잠깐 붉게 깜빡이면 다른 자리를 찾아보세요.")
                    .Invalid();
                break;
            default:
                request = new ModalRequest("놓는 방법",
                    "빈칸과 아래 도형을 하나씩 고르면 놓여요. 순서는 상관없어요.\n" +
                    "같은 것을 다시 누르면 선택이 풀리고, 빈 곳을 누르면 칸 선택이 풀려요.\n\n" +
                    "점이 찍힌 도형은 고정된 단서예요.\n" +
                    "정답 모양과 달라도 규칙에 맞게 채우면 완성!");
                break;
        }

        bool last = step >= TutorialSteps - 1;
        request.Button(last ? "시작하기" : $"다음  {step + 1} / {TutorialSteps}",
            last ? finish : () => ShowTutorialStep(step + 1, next));
        request.Button("건너뛰기", finish);
        request.Dismiss(finish);
        modals.Show(request);
    }

    void ShowChapterIntro()
    {
        var gm = GameManager.Instance;
        if (gm == null || modals == null) return;
        if (gm.currentMode != GameMode.Stage || !Chapters.IsChapterStart(gm.currentStage)) return;

        int chapter = Chapters.GetChapter(gm.currentStage);
        string key = "ChapterIntro_" + chapter;
        if (chapter <= 1 || PlayerPrefs.GetInt(key, 0) == 1) return;

        var info = Chapters.GetInfo(chapter);
        Action done = () =>
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            modals.Close();
        };
        var request = new ModalRequest(info.title, info.description).Dismiss(done);
        request.Button("시작하기", done);
        modals.Show(request);
    }

    public void ShowSettings()
    {
        if (modals == null) return;
        var request = new ModalRequest("편안하게 즐기기",
            "음악은 가벼운 신스 루프로 재생돼요.\n기록과 설정은 이 기기에만 남아요.")
            .Dismiss(modals.Close);
        AddToggle(request, "효과음", KeySound, 1);
        AddToggle(request, "배경음악", KeyMusic, 0);
        AddToggle(request, "작은 움직임", KeyMotion, 1);
        request.Button("완료", modals.Close);
        modals.Show(request);
    }

    void AddToggle(ModalRequest request, string label, string key, int fallback)
    {
        bool enabled = PlayerPrefs.GetInt(key, fallback) == 1;
        request.Button($"{label}   {(enabled ? "켜짐" : "꺼짐")}", () =>
        {
            PlayerPrefs.SetInt(key, enabled ? 0 : 1);
            PlayerPrefs.Save();
            UpdateMusic();
            ShowSettings();
        });
    }

    // ── 보조 ────────────────────────────────────────────────
    public void ShowToast(string message)
    {
        if (toast != null) toast.Show(message);
    }

    public void Celebrate()
    {
        if (PlayerPrefs.GetInt(KeyMotion, 1) == 0) return;
        if (confetti != null) confetti.Play(theme);
    }

    void UpdateMusic()
    {
        if (music == null) return;
        if (PlayerPrefs.GetInt(KeyMusic, 0) == 1) { if (!music.isPlaying) music.Play(); }
        else music.Stop();
    }

    void OnApplicationFocus(bool focused)
    {
        if (focused || gameScreen == null) return;
        if (current == (UIScreen)gameScreen && gameScreen.SessionActive
            && (modals == null || !modals.IsOpen)) gameScreen.Pause();
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused || gameScreen == null) return;
        if (current == (UIScreen)gameScreen && gameScreen.SessionActive
            && (modals == null || !modals.IsOpen)) gameScreen.Pause();
    }

    void OnApplicationQuit()
    {
        if (gameScreen != null) gameScreen.EndSession();
    }
}
