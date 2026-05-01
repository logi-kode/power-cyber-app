using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings")]
    [SerializeField] private float musicFadeDuration = 1.0f;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1.0f;

    private Coroutine _fadeCoroutine;

    // ──────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (musicSource) { musicSource.loop = true; musicSource.volume = musicVolume; }
        if (sfxSource) { sfxSource.loop = false; sfxSource.volume = sfxVolume; }
    }

    // ─────────────────────────────────────────────
    //  Music
    // ─────────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip) return; // Sudah diputar

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeMusic(clip));
    }

    public void StopMusic()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out
        if (musicSource.isPlaying)
        {
            float t = 0;
            float startVol = musicSource.volume;
            while (t < musicFadeDuration / 2)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0, t / (musicFadeDuration / 2));
                yield return null;
            }
        }

        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        float t2 = 0;
        while (t2 < musicFadeDuration / 2)
        {
            t2 += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, musicVolume, t2 / (musicFadeDuration / 2));
            yield return null;
        }
        musicSource.volume = musicVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        float t = 0;
        float startVol = musicSource.volume;
        while (t < musicFadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0, t / musicFadeDuration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = musicVolume;
    }

    // ─────────────────────────────────────────────
    //  SFX
    // ─────────────────────────────────────────────

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ─────────────────────────────────────────────
    //  Volume Control (untuk Settings menu)
    // ─────────────────────────────────────────────

    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        if (musicSource) musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
}