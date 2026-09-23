# 삼각원 — 유니티 네이티브 UI

HTML 프로토타입(`http://127.0.0.1:8765`)의 디자인과 기획서를 기준으로 하되,
구현은 **HTML 방식(런타임 절대좌표 생성)이 아니라 유니티 방식**으로 다시 만들었습니다.

## 실행

1. Unity 6000.3.11f1에서 프로젝트를 엽니다.
2. 메뉴 **`Samgakwon > Build UI (Scene + Prefabs)`** 를 한 번 실행합니다.
   - `Assets/WebScene/Samgakwon.unity` 를 authored 씬으로 새로 생성합니다(Build Settings 첫 씬).
   - `Assets/Samgakwon/Prefabs/` 에 프리팹 7종, `Assets/Samgakwon/SamgakwonTheme.asset` 에 디자인 토큰,
     `Assets/Samgakwon/NanumGothic SDF.asset` 에 한글 TMP 폰트(동적 아틀라스)를 만듭니다.
3. 생성된 씬에서 Play. 이후 모든 배치·색·문구는 **인스펙터에서 직접 편집**합니다.

기존 Start / Mode / Menu / Stage / InGame 씬은 그대로 보존했습니다.

## HTML 방식 → 유니티 방식으로 바꾼 것

| 이전 (`SamgakwonApp.cs`) | 지금 |
|---|---|
| `Awake`에서 전 UI를 코드로 생성 | **authored 씬 + 프리팹**. 코드는 `[SerializeField]` 참조만 사용 |
| `Position(x, y, w, h)` 절대 픽셀 (웹 좌표계) | **앵커 + LayoutGroup + ContentSizeFitter** |
| 보드 크기를 픽셀로 계산 | `BoardGrid`가 부모 폭에서 칸 크기를 산출 (CSS `repeat(9,1fr)` + `aspect-ratio` 대응) |
| 레거시 `Text` | **TextMeshPro** (한글 동적 폰트 에셋) |
| `Resources.LoadAll("경로/문자열")` | **`SamgakwonTheme`** ScriptableObject 참조 |
| 화면 전환마다 Destroy 후 재생성 (DOM 리렌더) | 화면 패널 **활성/비활성** 전환 |
| 486줄 God 클래스 하나 | 역할별 컴포넌트로 분리 |

### 구성

- `SamgakwonRouter` — 화면 전환, 모달/토스트/연출, 설정·음악, 매니저 부트스트랩
- `UIScreen` ← `HomeScreen` / `StagesScreen` / `GameScreen`
- 뷰 컴포넌트 — `TopBarView` `MenuCardView` `ChapterSectionView` `StageButtonView`
  `ShapePieceView` `LabeledButton` `DialogView` `ModalService` `ToastView` `ConfettiView` `BoardGrid`
- `SamgakwonTheme` — CSS `:root` 변수와 1:1로 대응하는 색·스프라이트·폰트
- 퍼즐 로직(`BoardManager` `PuzzleGenerator` `PuzzleSolver` `PuzzleSave` `Chapters` `RuleChecker`)은
  그대로 재사용했습니다.

## 빈칸 수(난이도) 재설계 — `StageBalance.cs`

승리 조건은 "모든 칸이 차고 규칙을 만족"이지 **생성된 정답과 일치하는 것이 아닙니다.**
따라서 난이도는 빈칸 수에 비례하지 않고 **중간에서 정점**을 찍습니다.

- 빈칸이 아주 적으면 → 놓을 칸이 몇 개뿐이라 쉽다.
- 빈칸이 아주 많으면 → 단서가 거의 없어 아무 유효한 배치나 만들면 이긴다.
  **한 번 익힌 패턴을 그대로 재현할 수 있어 오히려 가장 빨리 끝난다.**
  (빈 보드 81칸이 가장 쉬운 이유 — 길기만 하고 추론이 없다.)
- 중간 구간 → 고정 단서가 외운 패턴을 깨뜨려 매번 새로 판단해야 하고,
  선택지가 남아 있어 잘못 고르면 막힌다. **여기가 진짜 정점.**

5종 도형 × 구역당 최대 2개(구역 9칸)라 스도쿠보다 제약이 느슨해 "자유 배치" 구간이 일찍 시작됩니다.
정점을 50~60칸 부근으로 보고 다음과 같이 정했습니다.

| 구간 | 빈칸 |
|---|---|
| 스테이지 1 (시작 기준) | **12칸** |
| 스테이지 21 / 41 / 61 | 24 / 36 / 48칸 |
| 스테이지 81 (마지막 기준) | **60칸** |
| 클래식 쉬움 / 보통 / 어려움 | 15 / 35 / 55칸 (기획서 그대로) |
| 엔드리스 | 52~62칸 무작위 |

> 기획서의 "Stage 81 = 완전히 빈 보드"는 위 이유로 **의도적으로 채택하지 않았습니다.**
> 그 설정은 난이도 정점이 아니라 오히려 가장 쉬운 판이 됩니다.

챕터별 제거 전략(랜덤 / 중심부 / 블록균등 / 복합)은 기획서대로 유지해 난이도의 두 번째 축으로 씁니다.

## 기획서 반영 현황

기획서의 정식 버전 스코프는 모두 들어가 있습니다.

- 클래식 15 / 35 / 55칸, 전 난이도 완전 랜덤 제거
- 스테이지 81단계 + 챕터별 제거 전략(랜덤 · 중심부 · 블록균등 · 복합)
- 엔드리스(81 클리어 해금, 누적 카운터, 클리어 즉시 새 판)
- 사운드 5종(배치 · 지우개 · 힌트 · 클리어 · 신기록), 신스 BGM 1트랙
- 클리어 색종이 연출, 신기록 금색 패널
- 최초 1회 규칙 튜토리얼(스킵 가능), 챕터 21 / 41 / 61 안내 1회
- **챕터 전환 연출** — 챕터 경계에 들어설 때마다 민트 화면이 잠깐 덮이며
  `2장 · 집중` + 한 줄 설명을 보여 준다(`ChapterTransitionView`).
  안내 팝업은 1회지만 연출은 진입할 때마다 재생한다. 모션 설정을 끄면 건너뛴다.

## 조작 규칙

- **칸과 도형을 각각 켜고 끄는 방식** — 칸 하나와 도형 하나가 모두 켜져 있을 때 놓인다.
  **순서는 상관없다**(도형 먼저 골라도, 칸 먼저 골라도 동작).
  - 같은 것을 다시 누르면 꺼진다(칸·도형 모두 동일한 토글).
  - 켜진 칸은 `board/selection_outline` 테두리로 표시해, 어떤 칸이 켜졌는지 도형 칩처럼 한눈에 보인다.
  - 도형이 꺼진 상태로 칸을 누르면 행·열·블록 강조만 되고 아무것도 놓이지 않는다.
  - 칸도 도형도 아닌 **빈 곳을 누르면 칸 선택이 풀린다**(`BackgroundClickCatcher`가 캔버스 맨 뒤에서 받는다).
    지우기·힌트 같은 도구 버튼은 켜진 칸을 그대로 두어야 동작하므로 여기서 제외한다.
  - 놓은 직후에는 칸 선택이 자동으로 풀린다. 켜진 채로 두면 다음에 다른 도형을 고르는 순간
    방금 놓은 칸이 바뀌어 버린다.
- **힌트 한 판 3회** — 힌트는 정답을 곧바로 채우므로 횟수를 제한한다(`GameScreen.maxHints`).
  버튼에 남은 횟수가 `힌트 2` 처럼 표시되고 0이 되면 비활성화된다.
  막힌 배치나 시간 초과로 힌트가 실패하면 횟수를 차감하지 않으며,
  사용 횟수는 이어하기에 함께 저장되고 완료 화면에 `힌트 N회` 로 표시된다.

기획서에 없지만 함께 넣은 것:

- **메모 모드** — 도구 줄의 `메모` 버튼. 켜면 칸을 눌렀을 때 도형 대신 작은 메모가 찍힌다.
  엔진(`BoardManager.isMemoMode`, `Cell.memos`)은 이미 있었으나 진입점이 없어 쓸 수 없었다.
- **최고 기록 표시** — 클래식 난이도 버튼에 `쉬움 · 15칸   최고 01:23`,
  스테이지 버튼 아래에 클리어 기록. 그동안 저장만 하고 보여 줄 데가 없었다.

## 보드 구성

원본 CSS 의 칸은 **각지고**(`border-radius:0`) 1px 헤어라인만 있으며,
둥근 모서리(7px)는 **선택된 칸에만** 붙는다. 이에 맞춰:

- 칸 배경은 둥근 스프라이트가 아니라 **단색**이다. 칸마다 둥글게 하면 칸이 따로 떠 보여
  격자로 읽히지 않는다.
- 선택 테두리(`board/selection_outline`)는 `Sliced` 가 아니라 **`Simple`** 이다.
  Sliced 면 반경이 원본 픽셀(12px)로 고정돼 너무 둥글고, Simple 이면 100px 원본이
  칸 크기(약 66px)로 줄며 반경도 약 8px 가 되어 CSS 의 7px 와 맞는다.
- 3×3 블록 9개를 **실제로 중첩**한다(`Blocks` → `Block 1..9` → 칸).
  9×9 하나에 균일 간격을 주고 구분선을 덧그리면 결국 한 덩어리로 읽힌다.
  블록 간격 5px(`theme.line`), 칸 간격 1px(`theme.cellHairline`)로 굵기를 나눴다.
  간격은 씬의 `Blocks` / `Block n` 의 GridLayoutGroup Spacing 에서 바로 조절할 수 있다.
- `BoardManager.blockParents` 에 블록 9개를 넣으면 칸이 블록별로 나뉘어 담긴다
  (비워 두면 기존처럼 `boardParent` 한 곳에 담긴다 — 옛 씬 호환).

결과 치수: 보드 612 / 블록 200.7 / 칸 66.2 (HTML 기준 칸 66).

## 에셋 사용

팩의 아트를 그대로 쓰도록 정리했다. 사용률 28/179 → **72/182**.

- **9-slice** — 패널·버튼 원본은 128×128 정사각인데 420×60 같은 가로 막대로 쓰이므로
  `Image.Type.Simple` 로는 둥근 모서리가 뭉개진다. **패널 · 버튼 · 진행도** 스프라이트를
  `Sliced` 로 바꾸고, 보더가 비어 있던 41개 메타에 SVG 의 `rx` 기준으로 값을 넣었다
  (버튼 rx22 → 28, 패널 rx24 → 30, 진행도 rx6 → 좌우 6).
  Multiple 모드의 서브스프라이트 `border` 만 수정해 기존 참조 fileID 는 그대로 유지했다.
  **칸과 선택 테두리는 Sliced 가 아니다** — 이유는 위 "보드 구성" 참조.
- **버튼 상태** — `ButtonSprites`(normal/hover/pressed/disabled/selected)를 테마에 두고
  `Selectable.Transition.SpriteSwap` 으로 연결했다. primary · secondary · stage · selector 4세트.
- **전용 에셋으로 교체** — 토스트 `panels/toast`, 스크림 `background/modal_scrim`,
  진행도 `controls/progress_track` · `progress_fill`, 스테이지 배지 `badges/complete` ·
  `current` · `locked`, 색종이 `particles/confetti_*`, 챕터 아이콘 `chapters/chapter_1~4`.
  `board/cell_*` 는 테마에 실려 있지만 **보드에는 쓰지 않는다**(칸은 단색이 원본에 맞다).
  `Cell` 이 스프라이트 슬롯을 지원하므로 필요하면 인스펙터에서 꽂을 수 있다.
- **튜토리얼 그림** — 원 3개로 대신하던 설명을 팩의 `tutorial/row_two_allowed` ·
  `column_two_allowed` · `block_two_allowed` + `third_is_invalid` 로 교체했다.

## 튜토리얼

긴 글 한 장 대신 **3단계**로 나눈다(`SamgakwonRouter.ShowTutorialStep`).

1. **같은 도형은 두 개까지** — 행 / 열 / 블록 그림 3장
2. **세 번째는 놓을 수 없어요** — `third_is_invalid` 그림
3. **놓는 방법** — 칸·도형 선택, 해제, 고정 단서

어느 단계에서든 `건너뛰기` 또는 Esc 로 빠져나갈 수 있고, 그때 `TutorialDone_v1` 을 기록한다.
`ModalRequest` 의 `Samples()` 는 예시 3장, `Invalid()` 는 불가 그림을 각각 켠다.

> **주의 — 다이얼로그 텍스트에는 고정 높이를 주지 말 것.**
> `AddText` 가 `TextOverflowModes.Overflow` 를 쓰므로, LayoutElement 로 높이를 고정하면
> 긴 본문이 박스를 넘어 아래 그림·버튼과 **겹쳐 그려진다**. 실제로 튜토리얼 본문(약 10줄)이
> 110px 박스에서 넘쳐 전부 겹쳤다. TMP 는 `ILayoutElement` 라 스스로 필요한 높이를 보고하므로
> 제목·본문·기록 텍스트에는 `Size(...)` 를 걸지 않는다.

### 새로 만든 에셋

`icons/memo_{dark,light,teal}` — 팩에 연필/메모 계열이 없어 새로 그렸다.
팩과 같은 규격(64×64, stroke 4, round cap, `#484B57` / `#FFFBF3` / `#08AAA5`)으로
SVG 와 PNG 를 함께 넣었고, 자동 분할을 막기 위해 `.meta` 를 Single 모드로 지정했다.

## 사운드 현황

효과음 5종(배치 · 지우개 · 힌트 · 클리어 · 신기록)과 BGM 1트랙이 연결돼 있으나,
**현재 소리는 제작된 오디오가 아니라 `SFXManager.MakeTone` 이 만드는 합성 톤**이다.
프로젝트에 게임용 오디오 에셋이 없기 때문이다.

제작된 오디오로 교체하려면 씬의 **`Samgakwon Audio`** 오브젝트에 있는 `SFXManager` 의
`placeClip` / `eraseClip` / `hintClip` / `clearClip` / `newRecordClip` 슬롯에 꽂으면 된다.
비어 있는 슬롯만 합성 톤으로 대체되므로 일부만 교체해도 된다.
BGM 은 `SamgakwonRouter` 의 `music` AudioSource 에서 클립을 바꾸면 된다.

BGM 기본값은 **꺼짐**이다(`SamgakwonMusic`, 원본 사이트와 동일).
기획서는 BGM 을 필수 피드백으로 두므로, 첫 실행부터 들려줄지는 결정이 필요하다.

## 저장 키

`UnlockedStages`, `Stage_{n}_Cleared`, `Record_Classic_*`, `Record_Stage_*`,
`EndlessUnlocked`, `EndlessClearCount`, `TutorialDone_v1`, `ChapterIntro_*`,
`SamgakwonResume_v1`, `SamgakwonSound/Music/Motion` — 기존 키를 그대로 재사용합니다.

## 검증

- `Samgakwon > Validate Puzzle and Save` — 기존 퍼즐/저장 검증.
- 런타임·에디터 스크립트는 Unity 어셈블리 대상으로 컴파일 검증했습니다(에러 0).
- **아직 에디터에서 Play로 실물 확인은 하지 않았습니다.** 위 빌드 메뉴 실행 후 확인이 필요합니다.

## 정리 현황

구현이 **하나로 합쳐졌다.** 옛 씬 경로를 전부 제거했다.

삭제:

- 씬 5개 — `InGame` · `Menu` · `Mode` · `Stage` · `Start` (남은 씬은 `Samgakwon` 하나)
- 스크립트 12개 — `SamgakwonApp`(486줄, HTML 이식) · `GameController`(327줄) ·
  `ChangScene` · `MenuManager` · `TutorialManager` · `ChapterIntroController` ·
  `OptionManager` · `QuitGame` · `SettingManager` · `EditablIndicator` ·
  `RuleChecker` · `GameManagerInitializer`
- `Assets/Prefeb/` 전체 (옛 Cell · Image · SettingManager · StageLevel 프리팹)

함께 정리한 결합:

- `BoardManager.AfterChange` 의 `FindFirstObjectByType<GameController>()` 폴백 →
  `Completed?.Invoke()` 로 단순화. 새 화면은 항상 이벤트를 구독한다.
- `RuleChecker` — 공개 메서드 3개가 어디서도 호출되지 않았다(판정은 `PuzzleSolver`).
  씬에 남아 있던 컴포넌트까지 제거했다.
- Build Settings — 존재하지 않는 `Assets/Scenes/*` 항목 3개와 옛 씬 5개를 빼고
  `Samgakwon.unity` 하나만 남겼다.

검증: 컴파일 0 에러, 씬 dangling 0, 깨진 스크립트 참조 0(미해결 GUID 는 전부 Unity 패키지).

### 남은 것 — 빌드 용량

`Assets/Resources/` 안의 파일은 **참조 여부와 무관하게 전부 빌드에 포함**된다.
옛 씬 삭제로 아래 19개가 고아가 됐고 합계 약 51 MB 다.

- `33.5s Recording ... .wav` (46 MB) · `MainMixer.mixer`
- 옛 UI 이미지 17장 (`Group *.png` · `Frame *.png` · `Rectangle 35.png` · `Pallete.png` 등)

런타임 코드는 더 이상 `Resources.Load` 를 쓰지 않는다(에디터 검증 스크립트만 사용).
에셋 팩을 `Resources/` 밖으로 옮기면 실제로 참조되는 것만 빌드에 들어간다.
