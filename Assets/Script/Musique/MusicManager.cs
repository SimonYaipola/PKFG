using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;   // musique normale (scène)
    [SerializeField] private AudioSource sfxSource;     // optionnel (petits SFX/UI)

    [Header("Scene Music")]
    [Tooltip("Associe le nom de scène -> clip")]
    [SerializeField] private List<SceneMusic> sceneMusics = new();

    [Header("Battle Music")]
    [SerializeField] private AudioClip battleMusic;

    [Header("Fade")]
    [SerializeField] private float fadeTime = 0.35f;
    [SerializeField] private float musicVolume = 0.8f;

    private Dictionary<string, AudioClip> sceneMusicMap;
    private Coroutine fadeRoutine;

    private bool inBattle = false;
    private string lastSceneName = "";

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = musicVolume;

        BuildMap();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void BuildMap()
    {
        sceneMusicMap = new Dictionary<string, AudioClip>();
        foreach (var sm in sceneMusics)
        {
            if (sm != null && !string.IsNullOrWhiteSpace(sm.sceneName) && sm.clip != null)
                sceneMusicMap[sm.sceneName] = sm.clip;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lastSceneName = scene.name;

        // Si on est en combat, on laisse la musique de combat
        if (inBattle)
            return;

        PlaySceneMusic(scene.name);
    }

    public void PlaySceneMusic(string sceneName)
    {
        if (sceneMusicMap == null || sceneMusicMap.Count == 0)
            BuildMap();

        if (!sceneMusicMap.TryGetValue(sceneName, out var clip) || clip == null)
        {
            Debug.LogWarning($"[MusicManager] Aucun clip assigné pour la scène '{sceneName}'.");
            return;
        }

        CrossfadeTo(clip, loop: true);
    }

    public void StartBattleMusic()
    {
        if (battleMusic == null)
        {
            Debug.LogWarning("[MusicManager] battleMusic est null.");
            return;
        }

        inBattle = true;
        CrossfadeTo(battleMusic, loop: true);
    }

    public void EndBattleMusic()
    {
        inBattle = false;

        // Retour à la musique de la scène active
        var sceneName = SceneManager.GetActiveScene().name;
        PlaySceneMusic(sceneName);
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    private void CrossfadeTo(AudioClip newClip, bool loop)
    {
        if (musicSource == null) return;

        if (musicSource.clip == newClip && musicSource.isPlaying)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(newClip, loop));
    }

    private IEnumerator FadeRoutine(AudioClip newClip, bool loop)
    {
        float startVol = musicSource.volume;

        // fade out
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        // fade in
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeTime);
            yield return null;
        }

        musicSource.volume = musicVolume;
        fadeRoutine = null;
    }
    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Pause();
    }
    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.UnPause();
    }
    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}
