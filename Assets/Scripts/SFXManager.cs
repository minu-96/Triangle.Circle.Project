// SFXManager.cs
// 효과음 재생 전용 싱글톤. 기획서 최소 4종(배치/지우개/힌트/클리어) + 신기록 상위음.
// 클립이 비어 있어도 안전하게 동작(무음)하며, 씬 어디서든 SFXManager.Play*() 로 호출한다.
// 볼륨을 믹서로 묶으려면 AudioSource 의 Output 을 SFX 믹서 그룹으로 지정하면 된다.

using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio")]
    [Tooltip("효과음 재생용 AudioSource. Output 을 SFX 믹서 그룹으로 지정하세요.")]
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip placeClip;      // 도형 배치음
    public AudioClip eraseClip;      // 지우개/삭제음
    public AudioClip hintClip;       // 힌트 사용음
    public AudioClip clearClip;      // 클리어음
    public AudioClip newRecordClip;  // 신기록 시 상위 클리어음

    [Header("Clips — 상호작용")]
    public AudioClip errorClip;      // 규칙 위반으로 놓이지 않을 때
    public AudioClip uiClickClip;    // 버튼 전반
    public AudioClip selectClip;     // 칸 선택 · 해제
    public AudioClip memoClip;       // 메모 찍기 · 지우기
    public AudioClip chapterClip;    // 챕터 전환

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        if (placeClip == null) placeClip = MakeTone(new float[] { 660 });
        if (eraseClip == null) eraseClip = MakeTone(new float[] { 330 });
        if (hintClip == null) hintClip = MakeTone(new float[] { 523, 784 });
        if (clearClip == null) clearClip = MakeTone(new float[] { 523, 659, 784 });
        if (newRecordClip == null) newRecordClip = MakeTone(new float[] { 523, 659, 784, 1047 });
        // UI 음은 짧고 작게. 자주 울리므로 거슬리면 안 된다.
        if (errorClip == null) errorClip = MakeTone(new float[] { 180 }, 0.1f, 0.18f, 0.10f);
        if (uiClickClip == null) uiClickClip = MakeTone(new float[] { 784 }, 0.1f, 0.11f, 0.075f);
        if (selectClip == null) selectClip = MakeTone(new float[] { 523 }, 0.1f, 0.07f, 0.045f);
        if (memoClip == null) memoClip = MakeTone(new float[] { 587 }, 0.1f, 0.09f, 0.055f);
        if (chapterClip == null) chapterClip = MakeTone(new float[] { 392, 523, 659 }, 0.13f, 0.30f, 0.09f);
    }

    void PlayClip(AudioClip clip)
    {
        if (clip == null || sfxSource == null || PlayerPrefs.GetInt("SamgakwonSound", 1) == 0) return;
        sfxSource.PlayOneShot(clip);
    }

    public static AudioClip MakeTone(float[] notes, float step = 0.1f, float duration = 0.25f, float volume = 0.12f)
    {
        const int rate = 22050;
        var samples = new float[Mathf.CeilToInt(((notes.Length - 1) * step + duration) * rate)];
        for (int n = 0; n < notes.Length; n++)
            for (int i = 0; i < duration * rate; i++)
            {
                int index = Mathf.RoundToInt(n * step * rate) + i;
                if (index >= samples.Length) break;
                float t = (float)i / rate;
                float envelope = Mathf.Min(t / 0.012f, 1f) * Mathf.Exp(-6f * t / duration) * Mathf.Clamp01((duration - t) / 0.02f);
                samples[index] += Mathf.Sin(2 * Mathf.PI * notes[n] * t) * envelope * volume;
            }
        var clip = AudioClip.Create("Samgakwon Synth", samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // ── 정적 헬퍼: Instance 가 없어도 안전 ──────────────────────
    public static void PlayPlace()     { if (Instance != null) Instance.PlayClip(Instance.placeClip); }
    public static void PlayErase()     { if (Instance != null) Instance.PlayClip(Instance.eraseClip); }
    public static void PlayHint()      { if (Instance != null) Instance.PlayClip(Instance.hintClip); }
    public static void PlayClear()     { if (Instance != null) Instance.PlayClip(Instance.clearClip); }
    public static void PlayNewRecord() { if (Instance != null) Instance.PlayClip(Instance.newRecordClip); }
    public static void PlayError()     { if (Instance != null) Instance.PlayClip(Instance.errorClip); }
    public static void PlayUIClick()   { if (Instance != null) Instance.PlayClip(Instance.uiClickClip); }
    public static void PlaySelect()    { if (Instance != null) Instance.PlayClip(Instance.selectClip); }
    public static void PlayMemo()      { if (Instance != null) Instance.PlayClip(Instance.memoClip); }
    public static void PlayChapter()   { if (Instance != null) Instance.PlayClip(Instance.chapterClip); }
}
