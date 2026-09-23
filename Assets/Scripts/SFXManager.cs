// SFXManager.cs
// 효과음 재생 전용 싱글톤. 기획서 최소 4종(배치/지우개/힌트/클리어) + 신기록 상위음.
// 클립이 비어 있어도 안전하게 동작(무음)하며, 씬 어디서든 SFXManager.Play*() 로 호출한다.
// 볼륨은 OptionManager 의 AudioMixer "SFXVolume" 그룹으로 라우팅한다(AudioSource.outputAudioMixerGroup 인스펙터 지정).

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
}
