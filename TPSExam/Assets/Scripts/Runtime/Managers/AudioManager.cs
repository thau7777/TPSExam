using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class Sound
{
    public string name;         // Key name
    public AudioClip clip;      // Clip reference
}

public class AudioManager : Singleton<AudioManager>
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer; // assign MainMixer here

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private List<Sound> musicClips = new List<Sound>();

    [Header("SFX Clips")]
    [SerializeField] private List<Sound> sfxClips = new List<Sound>();

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("SFX Pitch Randomization")]
    [Range(0.8f, 1.2f)] public float minSfxPitch = 0.95f;
    [Range(0.8f, 1.2f)] public float maxSfxPitch = 1.05f;

    private Dictionary<string, AudioClip> _musicDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> _sfxDict = new Dictionary<string, AudioClip>();

    private const string MasterKey = "MasterVolume";
    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SfxVolume";

    private void OnEnable()
    {
        //GameManager.Instance.OnQuitGame += SaveVolumes;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //GameManager.Instance.OnQuitGame -= SaveVolumes;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if(scene.name == "MainMenu")
            PlayMusic("MainMenu");
        else
            PlayMusic("Gameplay");
    }

    protected override void Awake()
    {
        base.Awake();

        // Fill dictionaries from inspector lists
        foreach (var m in musicClips)
            if (!_musicDict.ContainsKey(m.name) && m.clip != null)
                _musicDict.Add(m.name, m.clip);

        foreach (var s in sfxClips)
            if (!_sfxDict.ContainsKey(s.name) && s.clip != null)
                _sfxDict.Add(s.name, s.clip);

        LoadVolumes();
        ApplyVolumes();
    }

    // --- REGISTER (still optional for runtime adds) ---
    public void RegisterMusic(string name, AudioClip clip) => _musicDict[name] = clip;
    public void RegisterSFX(string name, AudioClip clip) => _sfxDict[name] = clip;

    // MUSIC
    public void PlayMusic(string name, bool loop = true)
    {
        if (_musicDict.TryGetValue(name, out var clip))
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music '{name}' not found!");
        }
    }
    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }
    // SFX
    public void PlaySFX(string name)
    {
        if (_sfxDict.TryGetValue(name, out var clip))
        {
            sfxSource.pitch = Random.Range(minSfxPitch, maxSfxPitch);
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{name}' not found!");
        }
    }

    // --- VOLUME SETTERS ---
    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        SetMixerVolume("Master", value);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        SetMixerVolume("Music", value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        SetMixerVolume("SFX", value);
    }

    private void ApplyVolumes()
    {
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    // --- MIXER HELPER ---
    private void SetMixerVolume(string exposedParam, float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat(exposedParam, dB);
    }

    // --- SAVE / LOAD ---
    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MasterKey, masterVolume);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterKey, 1f);
        musicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SfxKey, 1f);
    }
}
