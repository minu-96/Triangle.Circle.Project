using UnityEngine;

/// <summary>
/// 빈칸 수(난이도) 곡선을 한 곳에서 정의한다.
///
/// 설계 근거 — 이 게임의 승리 조건은 "모든 칸이 차고 규칙을 만족"이지,
/// 생성된 정답과 일치하는 것이 아니다(PuzzleSolver.IsValid(board, true)).
/// 따라서 난이도는 빈칸 수에 비례하지 않고 중간에서 정점을 찍는다.
///
///  · 빈칸이 아주 적으면   → 놓을 칸이 몇 개뿐이라 쉽다.
///  · 빈칸이 아주 많으면   → 단서가 거의 없어 "아무 유효한 배치나" 만들면 이긴다.
///                          한 번 익힌 패턴을 그대로 재현할 수 있어 오히려 빠르게 끝난다.
///                          (빈 보드 81칸이 가장 쉬운 이유. 길기만 하고 추론이 없다.)
///  · 중간 구간           → 고정 단서가 외운 패턴을 깨뜨려 매번 새로 판단해야 하고,
///                          선택지가 남아 있어 잘못 고르면 막힌다. 여기가 진짜 난이도 정점.
///
/// 5종 도형 × 구역당 최대 2개(구역 9칸)라 스도쿠보다 제약이 느슨하므로
/// "자유 배치" 구간이 비교적 일찍 시작된다. 정점을 50~60칸 부근으로 본다.
/// </summary>
public static class StageBalance
{
    /// <summary>스테이지 1의 빈칸 수. 규칙을 배우되 시시하지 않은 최소치.</summary>
    public const int StageFirstBlanks = 12;

    /// <summary>스테이지 81의 빈칸 수. 빈 보드(81)가 아니라 난이도 정점 구간에 둔다.</summary>
    public const int StageFinalBlanks = 60;

    /// <summary>엔드리스 빈칸 범위(포함). 최종 스테이지와 같거나 조금 더 높은 정점 구간.</summary>
    public const int EndlessMinBlanks = 52;
    public const int EndlessMaxBlanks = 62;

    /// <summary>클래식 난이도별 빈칸 수 — 기획서 값(15 / 35 / 55)을 그대로 따른다.</summary>
    public static int ClassicBlanks(GameDifficulty difficulty) => difficulty switch
    {
        GameDifficulty.Easy => 15,
        GameDifficulty.Normal => 35,
        GameDifficulty.Hard => 55,
        _ => 35
    };

    /// <summary>스테이지 번호 → 빈칸 수. 12칸에서 60칸까지 선형으로 오른다.</summary>
    /// <remarks>1장 시작 12 · 2장 24 · 3장 36 · 4장 48 · 마지막 60.</remarks>
    public static int StageBlanks(int stage)
    {
        stage = Mathf.Clamp(stage, 1, Chapters.TotalStages);
        float t = (stage - 1) / (float)(Chapters.TotalStages - 1);
        return Mathf.RoundToInt(Mathf.Lerp(StageFirstBlanks, StageFinalBlanks, t));
    }

    public static int EndlessBlanks() => Random.Range(EndlessMinBlanks, EndlessMaxBlanks + 1);
}
