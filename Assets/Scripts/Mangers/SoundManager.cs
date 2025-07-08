using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] preloadClips;
    
    [Header("Volume Settings")]
    [SerializeField] private float defaultMasterVolume = 1.0f;
    [SerializeField] private float defaultBGMVolume = 0.7f;
    [SerializeField] private float defaultSFXVolume = 1.0f;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private Dictionary<string, AudioClip> clipDictionary;
    private float currentMasterVolume = 1f;
    private float currentBGMVolume = 1f;
    private float currentSFXVolume = 1f;
    
    // 현재 재생 중인 BGM
    private string currentBGMName = "";
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Init();
        LoadVolumeSettings();
        PlayBGM("MainBGM");
    }

    private void InitializeAudioSources()
    {
        // BGM용 AudioSource 생성
        if (bgmSource == null)
        {
            GameObject bgmObject = new GameObject("BGM AudioSource");
            bgmObject.transform.SetParent(transform);
            bgmSource = bgmObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        // SFX용 AudioSource 생성
        if (sfxSource == null)
        {
            GameObject sfxObject = new GameObject("SFX AudioSource");
            sfxObject.transform.SetParent(transform);
            sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    public void Init()
    {
        clipDictionary = new Dictionary<string, AudioClip>();
        
        if (preloadClips != null)
        {
            foreach (AudioClip clip in preloadClips)
            {
                if (clip != null)
                {
                    clipDictionary.Add(clip.name, clip);
                }
            }
        }
        
        Debug.Log($"SoundManager 초기화 완료. 로드된 클립: {clipDictionary.Count}개");
    }

    #region BGM 관련 메서드
    public void PlayBGM(string clipName, bool fadeIn = true, float fadeTime = 1f)
    {
        AudioClip clip = GetClip(clipName);
        if (clip == null) return;

        if (currentBGMName == clipName && bgmSource.isPlaying) return;

        currentBGMName = clipName;
        
        if (fadeIn)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeInBGM(clip, fadeTime));
        }
        else
        {
            bgmSource.clip = clip;
            bgmSource.volume = currentBGMVolume;
            bgmSource.Play();
        }
    }

    public void StopBGM(bool fadeOut = true, float fadeTime = 1f)
    {
        if (!bgmSource.isPlaying) return;

        if (fadeOut)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutBGM(fadeTime));
        }
        else
        {
            bgmSource.Stop();
            currentBGMName = "";
        }
    }

    public void PauseBGM()
    {
        if (bgmSource.isPlaying)
            bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource.clip != null && !bgmSource.isPlaying)
            bgmSource.UnPause();
    }
    #endregion

    #region SFX 관련 메서드
    public void PlaySFX(string clipName, float volume = 1f)
    {
        AudioClip clip = GetClip(clipName);
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, volume * GetEffectiveSFXVolume());
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip, volume * GetEffectiveSFXVolume());
    }

    public void PlaySFXAtPosition(string clipName, Vector3 position, float volume = 1f)
    {
        AudioClip clip = GetClip(clipName);
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, volume * GetEffectiveSFXVolume());
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, volume * GetEffectiveSFXVolume());
    }
    #endregion

    #region 볼륨 제어
    public void SetMasterVolume(float volume)
    {
        currentMasterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        PlayerPrefs.SetFloat("MasterVolume", currentMasterVolume);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float volume)
    {
        currentBGMVolume = Mathf.Clamp01(volume);
        UpdateBGMVolume();
        PlayerPrefs.SetFloat("BGMVolume", currentBGMVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        currentSFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", currentSFXVolume);
        PlayerPrefs.Save();
    }

    public float GetMasterVolume() => currentMasterVolume;
    public float GetBGMVolume() => currentBGMVolume;
    public float GetSFXVolume() => currentSFXVolume;

    // 실제 적용되는 볼륨 계산
    public float GetEffectiveBGMVolume() => currentMasterVolume * currentBGMVolume;
    public float GetEffectiveSFXVolume() => currentMasterVolume * currentSFXVolume;

    private void UpdateAllVolumes()
    {
        UpdateBGMVolume();
    }

    private void UpdateBGMVolume()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = GetEffectiveBGMVolume();
        }
    }

    private void LoadVolumeSettings()
    {
        currentMasterVolume = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        currentBGMVolume = PlayerPrefs.GetFloat("BGMVolume", defaultBGMVolume);
        currentSFXVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
        
        UpdateAllVolumes();
    }

    // 전체 음소거/음소거 해제 (MasterVolume을 0으로 설정)
    public void SetMasterMute(bool isMuted)
    {
        SetMasterVolume(isMuted ? 0f : 1f);
    }

    // 음소거 상태 확인
    public bool IsMasterMuted()
    {
        return currentMasterVolume <= 0f;
    }
    #endregion

    #region 유틸리티 메서드
    private AudioClip GetClip(string clipName)
    {
        if (clipDictionary.ContainsKey(clipName))
        {
            return clipDictionary[clipName];
        }
        
        Debug.LogWarning($"AudioClip을 찾을 수 없습니다: {clipName}");
        return null;
    }

    public bool IsClipLoaded(string clipName)
    {
        return clipDictionary.ContainsKey(clipName);
    }

    public void AddClip(AudioClip clip)
    {
        if (clip != null && !clipDictionary.ContainsKey(clip.name))
        {
            clipDictionary.Add(clip.name, clip);
        }
    }
    #endregion

    #region 코루틴
    private IEnumerator FadeInBGM(AudioClip clip, float fadeTime)
    {
        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float targetVolume = GetEffectiveBGMVolume();
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / fadeTime);
            yield return null;
        }
        
        bgmSource.volume = targetVolume;
    }

    private IEnumerator FadeOutBGM(float fadeTime)
    {
        float startVolume = bgmSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeTime);
            yield return null;
        }

        bgmSource.Stop();
        currentBGMName = "";
    }
    #endregion

    // 디버그용 메서드
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintLoadedClips()
    {
        Debug.Log("=== 로드된 오디오 클립 목록 ===");
        foreach (var kvp in clipDictionary)
        {
            Debug.Log($"클립명: {kvp.Key}");
        }
    }
}
