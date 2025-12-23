using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private static PauseMenu Instance;

    [Header("Référence au root du menu pause (ton objet vide 'PauseMenu')")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Input (optionnel)")]
    [SerializeField] private InputActionReference togglePauseAction;

    [Header("Audio (optionnel)")]
    [SerializeField] private MusicManager musicManager;

    private bool isPaused;

    private void Awake()
    {
        // --- Singleton + persistance ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cache le menu au boot
        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(false);

        // Trouver MusicManager au départ (peut changer selon scène)
        if (musicManager == null)
            musicManager = FindFirstObjectByType<MusicManager>(FindObjectsInactive.Include);

        // Re-scan quand on change de scène
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si une scène reload pendant pause, on sort de pause proprement
        if (isPaused)
            Resume();

        // Rebind MusicManager de la nouvelle scène
        musicManager = FindFirstObjectByType<MusicManager>(FindObjectsInactive.Include);

        // (Optionnel) Si ton pauseMenuRoot est un enfant du même objet persistant, il reste valide.
        // Si jamais tu l’as mis dans la scène (pas enfant), il va être détruit -> faudra le re-link.
        if (pauseMenuRoot == null)
            pauseMenuRoot = gameObject; // fallback: au moins pas null (à ajuster si besoin)
        
        Debug.Log($"[PauseMenu] SceneLoaded={scene.name} | musicManager={(musicManager!=null)}");
    }

    private void OnEnable()
    {
        if (togglePauseAction != null)
            togglePauseAction.action.Enable();
    }

    private void OnDisable()
    {
        if (togglePauseAction != null)
            togglePauseAction.action.Disable();
    }

    private void Update()
    {
        bool pressed = false;

        // ✅ FIX: on détecte le PRESS, pas le PERFORM
        if (togglePauseAction != null && togglePauseAction.action != null)
            pressed = togglePauseAction.action.WasPressedThisFrame();

        // fallback clavier direct
        if (!pressed && Keyboard.current != null)
            pressed = Keyboard.current.escapeKey.wasPressedThisFrame;

        if (!pressed)
            return;

        if (!isPaused) Pause();
        else Resume();
    }

    public void Pause()
    {
        if (pauseMenuRoot == null)
        {
            Debug.LogWarning("[PauseMenu] pauseMenuRoot non assigné.");
            return;
        }

        isPaused = true;
        pauseMenuRoot.SetActive(true);
        Time.timeScale = 0f;

        musicManager?.PauseMusic();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (pauseMenuRoot == null)
            return;

        isPaused = false;

        Time.timeScale = 1f;
        pauseMenuRoot.SetActive(false);

        musicManager?.ResumeMusic();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // safe: flush action pour éviter trigger résiduel
        togglePauseAction?.action?.Reset();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
