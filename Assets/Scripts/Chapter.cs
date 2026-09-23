// Chapter.cs
// 스테이지 모드의 챕터 구성과 빈칸 제거 전략을 한 곳에서 정의한다.
// 기획서(정식 버전 풀 스코프) 기준:
//   1장 입문   : 1~20  · 완전 랜덤 제거
//   2장 집중   : 21~40 · 중심부 위주 제거
//   3장 균형   : 41~60 · 블록별 균등 배분
//   4장 종합   : 61~81 · 세 전략을 스테이지별로 랜덤 조합
// 빈칸 수 곡선은 StageBalance 참조(빈 보드는 오히려 쉬워지므로 쓰지 않는다).

public enum RemovalStrategy
{
    Random,        // 완전 랜덤
    CenterFocused, // 블록 중앙 및 인접 칸부터 제거
    BlockEven,     // 블록별로 고르게 분산 제거
    Mixed          // 위 세 전략 중 하나를 매 생성 시 랜덤 선택
}

public struct ChapterInfo
{
    public int chapter;          // 1~4
    public int startStage;       // 챕터 시작 스테이지 (1/21/41/61)
    public int endStage;         // 챕터 끝 스테이지 (20/40/60/81)
    public RemovalStrategy strategy;
    public string title;         // "1장 · 입문"
    public string name;          // "입문"
    public string tagline;       // 스테이지 목록용 한 줄 (사이트 문구)
    public string description;   // 챕터 전환 팝업용 설명
}

public static class Chapters
{
    public const int TotalStages = 81;

    // 각 챕터가 시작되는 스테이지 (챕터 전환 팝업 트리거)
    public static readonly int[] StartStages = { 1, 21, 41, 61 };

    // 스테이지 번호 → 챕터 번호(1~4)
    public static int GetChapter(int stage)
    {
        if (stage < 1) stage = 1;
        int c = 1 + (stage - 1) / 20;
        return c > 4 ? 4 : c; // Stage 81 도 4장에 포함
    }

    // 해당 스테이지가 챕터의 첫 스테이지인가 (전환 연출/안내 트리거)
    public static bool IsChapterStart(int stage)
    {
        foreach (int s in StartStages)
            if (s == stage) return true;
        return false;
    }

    // 챕터별 제거 전략
    public static RemovalStrategy GetStrategy(int chapter)
    {
        switch (chapter)
        {
            case 1: return RemovalStrategy.Random;
            case 2: return RemovalStrategy.CenterFocused;
            case 3: return RemovalStrategy.BlockEven;
            default: return RemovalStrategy.Mixed; // 4장
        }
    }

    // 스테이지 번호로 바로 전략을 얻는다.
    public static RemovalStrategy GetStrategyForStage(int stage)
    {
        return GetStrategy(GetChapter(stage));
    }

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
                    description = "규칙을 익히는 구간입니다.\n빈칸이 조금씩 늘어나며 무작위로 비워집니다.\n행·열·블록마다 같은 도형은 최대 2개!"
                };
            case 2:
                return new ChapterInfo {
                    chapter = 2, startStage = 21, endStage = 40,
                    strategy = RemovalStrategy.CenterFocused,
                    title = "2장 · 집중", name = "집중",
                    tagline = "블록 중심부에 빈칸이 모여 있어요.",
                    description = "블록 중앙과 인접 칸부터 비워집니다.\n좁은 범위를 집중해서 추론해 보세요."
                };
            case 3:
                return new ChapterInfo {
                    chapter = 3, startStage = 41, endStage = 60,
                    strategy = RemovalStrategy.BlockEven,
                    title = "3장 · 균형", name = "균형",
                    tagline = "여러 블록을 함께 살펴보세요.",
                    description = "모든 블록에 빈칸이 고르게 분산됩니다.\n보드 전체를 함께 고려해야 합니다."
                };
            default:
                return new ChapterInfo {
                    chapter = 4, startStage = 61, endStage = 81,
                    strategy = RemovalStrategy.Mixed,
                    title = "4장 · 종합", name = "종합",
                    tagline = "지금까지의 세 가지 배치를 함께 만나요.",
                    description = "세 가지 전략이 무작위로 조합됩니다.\n빈칸이 가장 많은 구간, 천천히 살펴보세요."
                };
        }
    }

    public static ChapterInfo GetInfoForStage(int stage)
    {
        return GetInfo(GetChapter(stage));
    }
}
