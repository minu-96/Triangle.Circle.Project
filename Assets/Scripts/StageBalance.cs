using UnityEngine;

/// <summary>
/// 빈칸 수(난이도) 곡선과 메모 단서 제공량을 정의한다.
///
/// 설계 근거 — 승리 조건은 "규칙을 만족하며 모두 채움"이지 생성된 정답과 일치하는 것이 아니다.
/// 따라서 빈칸이 많다고 어려워지지 않는다. 단서가 줄면 제약도 함께 줄어서,
/// 아무 유효한 배치나 만들어도 이기기 때문이다. 실측상 빈칸 44칸 부근에서 난이도가 포화된다.
///
/// 그래서 난이도를 **단독확정 진행률**(놓을 수 있는 도형이 1개뿐인 칸만 반복해 채웠을 때
/// 메워지는 비율)로 잡고, 이 값이 1단계에서 80단계까지 직선으로 내려가도록
/// 각 장의 빈칸 구간을 역산했다. 장마다 배치가 달라 난이도 기여가 다르므로 기울기도 다르다.
///
/// 41단계에서 빈칸이 27 → 19로 줄어드는 것은 의도된 설계다.
/// 중심부 배치가 같은 빈칸에서 12%p 이상 어렵기 때문에, 빈칸을 줄여야 곡선이 직선을 유지한다.
/// 장마다 컨셉이 다르므로 빈칸 수의 증감이 아니라 난이도가 오르는 것이 기준이다.
/// </summary>
public static class StageBalance
{
    /// <summary>장별 빈칸 구간 (시작 단계, 끝 단계, 시작 빈칸, 끝 빈칸).</summary>
    static readonly (int from, int to, int startBlanks, int endBlanks)[] Curve =
    {
        (1,  20,  6, 16),   // 1장 입문 · 랜덤     — 단독확정 83% → 62%
        (21, 40, 22, 27),   // 2장 균형 · 블록균등 — 57% → 37% (31단계부터 메모)
        (41, 60, 23, 28),   // 3장 집중 · 중심부   — 36% → 14% (50단계까지 메모)
        (61, 80, 33, 42),   // 4장 복합 · 통합     — 10% →  3%
    };

    /// <summary>마지막 한 판. 곡선 바깥에 따로 둔다.</summary>
    public const int FinalStageBlanks = 44;

    /// <summary>엔드리스 빈칸 범위(포함). 4장 수준.</summary>
    public const int EndlessMinBlanks = 38;
    public const int EndlessMaxBlanks = 46;

    /// <summary>메모 단서를 주는 구간과 개수(시작 8칸 → 끝 5칸).</summary>
    public const int MemoFirstStage = 31;
    public const int MemoLastStage = 50;
    const int MemoStartCount = 8;
    const int MemoEndCount = 5;

    /// <summary>클래식 난이도별 빈칸 수 — 기획서 값(15 / 35 / 55).</summary>
    public static int ClassicBlanks(GameDifficulty difficulty) => difficulty switch
    {
        GameDifficulty.Easy => 15,
        GameDifficulty.Normal => 35,
        GameDifficulty.Hard => 55,
        _ => 35
    };

    /// <summary>스테이지 번호 → 빈칸 수.</summary>
    public static int StageBlanks(int stage)
    {
        stage = Mathf.Clamp(stage, 1, Chapters.TotalStages);
        if (stage >= Chapters.TotalStages) return FinalStageBlanks;

        foreach (var seg in Curve)
        {
            if (stage < seg.from || stage > seg.to) continue;
            float t = seg.to == seg.from ? 0f : (stage - seg.from) / (float)(seg.to - seg.from);
            return Mathf.RoundToInt(Mathf.Lerp(seg.startBlanks, seg.endBlanks, t));
        }
        return Curve[Curve.Length - 1].endBlanks;
    }

    public static int EndlessBlanks() => Random.Range(EndlessMinBlanks, EndlessMaxBlanks + 1);

    /// <summary>이 단계에서 처음부터 채워 줄 메모 칸 수. 해당 없으면 0.</summary>
    public static int MemoCount(int stage)
    {
        if (stage < MemoFirstStage || stage > MemoLastStage) return 0;
        float t = (stage - MemoFirstStage) / (float)(MemoLastStage - MemoFirstStage);
        return Mathf.RoundToInt(Mathf.Lerp(MemoStartCount, MemoEndCount, t));
    }
}
