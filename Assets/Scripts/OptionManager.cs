// Scripts/UI/OptionsManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("AudioMixer 참조")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("슬라이더 참조")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("BGM 선택 드롭다운")]
    [SerializeField] private TMP_Dropdown bgmDropdown;  // TMP Dropdown 연결
    [SerializeField] private AudioSource bgmAudioSource; // BGM 재생용 AudioSource
    [SerializeField] private BGMTrack[] bgmTracks;       // 인스펙터에서 트랙 목록 등록
    [SerializeField] private float fadeDuration = 1.0f;  // 페이드 시간 (초)

    // PlayerPrefs 키 상수
    private const string KEY_BGM        = "OptionBGM";
    private const string KEY_SFX        = "OptionSFX";
    private const string KEY_BGM_TRACK  = "OptionBGMTrack";  // 선택한 트랙 인덱스 저장

    private Coroutine fadeCoroutine;

    void Start()
    {
        SetupBGMDropdown();   // 드롭다운 항목 먼저 세팅
        LoadSettings();       // 저장값 불러오기

        // LoadSettings 이후에 이벤트 연결 (중복 발생 방지)
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        bgmDropdown.onValueChanged.AddListener(OnBGMTrackChanged);
    }

    // 드롭다운 항목을 bgmTracks 배열 기반으로 자동 생성
    private void SetupBGMDropdown()
    {
        bgmDropdown.ClearOptions();

        var options = new List<string>();
        foreach (var track in bgmTracks)
            options.Add(track.displayName);

        bgmDropdown.AddOptions(options);
    }

    private void LoadSettings()
    {
        // BGM 볼륨
        float bgm = PlayerPrefs.GetFloat(KEY_BGM, 0.5f);
        bgmSlider.value = bgm;
        ApplyMixerVolume("BGMVolume", bgm);

        // SFX 볼륨
        float sfx = PlayerPrefs.GetFloat(KEY_SFX, 0.5f);
        sfxSlider.value = sfx;
        ApplyMixerVolume("SFXVolume", sfx);

        // BGM 트랙 (저장된 인덱스로 드롭다운 선택 + 즉시 재생)
        int trackIndex = PlayerPrefs.GetInt(KEY_BGM_TRACK, 0);
        trackIndex = Mathf.Clamp(trackIndex, 0, bgmTracks.Length - 1);

        bgmDropdown.value = trackIndex;
        bgmDropdown.RefreshShownValue();
        PlayBGMImmediate(trackIndex);  // 로드 시에는 페이드 없이 바로 재생
    }

    private void OnBGMChanged(float value)
    {
        ApplyMixerVolume("BGMVolume", value);
        PlayerPrefs.SetFloat(KEY_BGM, value);
        PlayerPrefs.Save();
    }

    private void OnSFXChanged(float value)
    {
        ApplyMixerVolume("SFXVolume", value);
        PlayerPrefs.SetFloat(KEY_SFX, value);
        PlayerPrefs.Save();
    }

    // 드롭다운에서 트랙 선택 시 호출
    private void OnBGMTrackChanged(int index)
    {
        PlayerPrefs.SetInt(KEY_BGM_TRACK, index);
        PlayerPrefs.Save();

        // 페이드 전환
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeToTrack(index));
    }

    // 앱 시작 시 즉시 재생 (페이드 없음)
    private void PlayBGMImmediate(int index)
    {
        if (bgmTracks == null || bgmTracks.Length == 0) return;
        bgmAudioSource.clip = bgmTracks[index].clip;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
    }

    // 현재 트랙 Fade Out → 새 트랙 Fade In
    private IEnumerator FadeToTrack(int index)
    {
        // Fade Out: AudioSource 볼륨을 0으로
        float startVolume = bgmAudioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        // 트랙 교체
        bgmAudioSource.Stop();
        bgmAudioSource.clip = bgmTracks[index].clip;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();

        // Fade In: 볼륨을 1로 복원
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        bgmAudioSource.volume = 1f;
    }

    private void ApplyMixerVolume(string paramName, float sliderValue)
    {
        if (mainMixer == null) return;
        float dB = sliderValue > 0.001f
            ? Mathf.Log10(sliderValue) * 20f
            : -80f;
        mainMixer.SetFloat(paramName, dB);
    }
}

// BGM 트랙 데이터 구조 (같은 파일 하단 또는 별도 파일)
[System.Serializable]
public class BGMTrack
{
    public string displayName;
    public AudioClip clip;
}