// Chapter.cs
// 스테이지 모드의 챕터 구성과 빈칸 제거 배치를 한 곳에서 정의한다.
//
// 장 순서는 측정으로 정했다. 같은 빈칸 수에서의 난이도는
//   균등 < 랜덤 ≈ 통합 < 중심부
// 순이라, 가장 어려운 중심부를 뒤로 보내야 난이도가 단조 증가한다.
// 빈칸 수 곡선은 StageBalance 참조.

public enum RemovalStrategy
{
    Random,        // 완전 랜덤
    BlockEven,     // 블록별로 고르게 분산
    CenterFocused, // 블록 중앙 및 인접 칸부터
    Composite      // 위 셋을 한 판에 섞어 적용
}

public struct ChapterInfo
{
    public int chapter;          // 1~4
    public int startStage;       // 챕터 시작 스테이지 (1/21/41/61)
    public int endStage;         // 챕터 끝 스테이지 (20/40/60/81)
    public RemovalStrategy strategy;
    public string title;         // "1장 · 입문"
    public string name;          // "입문"
    public string tagline;       // 스테이지 목록용 한 줄
    public string description;   // 챕터 전환 팝업용 설명
}

public static class Chapters
{
    public const int TotalStages = 81;

    // 각 챕터가 시작되는 스테이지 (전환 연출 · 안내 트리거)
    public static readonly int[] StartStages = { 1, 21, 41, 61 };

    public static int GetChapter(int stage)
    {
        if (stage < 1) stage = 1;
        int c = 1 + (stage - 1) / 20;
        return c > 4 ? 4 : c; // 81단계도 4장에 포함
    }

    public static bool IsChapterStart(int stage)
    {
        foreach (int s in StartStages)
            if (s == stage) return true;
        return false;
    }

    public static RemovalStrategy GetStrategy(int chapter)
    {
        switch (chapter)
        {
            case 1: return RemovalStrategy.Random;
            case 2: return RemovalStrategy.BlockEven;
            case 3: return RemovalStrategy.CenterFocused;
            default: return RemovalStrategy.Composite; // 4장
        }
    }

    public static RemovalStrategy GetStrategyForStage(int stage) => GetStrategy(GetChapter(stage));

    public static ChapterInfo GetInfo(int chapter)
    {
        switch (chapter)
        {
            case 1:
                return new ChapterInfo {
                    chapter = 1, startStage = 1, endStage = 20,
                    strategy = RemovalStrategy.Random,
                    title = "1장 · 입문", name = "입문",
                    tagline = "빈칸이 골고루 흩어져 있어요.",
                    description = "규칙을 익히는 구간입니다.\n놓을 곳이 하나뿐인 칸이 많으니\n차근차근 채워 보세요."
                };
            case 2:
                return new ChapterInfo {
                    chapter = 2, startStage = 21, endStage = 40,
                    strategy = RemovalStrategy.BlockEven,
                    title = "2장 · 균형", name = "균형",
                    tagline = "모든 블록에 빈칸이 고르게 퍼져 있어요.",
                    description = "빈칸이 아홉 블록에 고르게 나뉩니다.\n한 블록만 보지 말고\n보드 전체를 함께 살펴보세요."
                };
            case 3:
                return new ChapterInfo {
                    chapter = 3, startStage = 41, endStage = 60,
                    strategy = RemovalStrategy.CenterFocused,
                    title = "3장 · 집중", name = "집중",
                    tagline = "블록 중심부에 빈칸이 모여 있어요.",
                    description = "블록 중앙부터 비워집니다.\n빈칸은 줄었지만 한곳에 몰려 있어\n좁은 범위를 깊게 파고들어야 합니다."
                };
            default:
                return new ChapterInfo {
                    chapter = 4, startStage = 61, endStage = 81,
                    strategy = RemovalStrategy.Composite,
                    title = "4장 · 복합", name = "복합",
                    tagline = "지금까지의 세 배치가 한 판에 섞여요.",
                    description = "흩어짐 · 고름 · 몰림이 한 판에 함께 나옵니다.\n지금까지 익힌 방법을 모두 써 보세요."
                };
        }
    }

    public static ChapterInfo GetInfoForStage(int stage) => GetInfo(GetChapter(stage));
}
