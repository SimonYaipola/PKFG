using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle }

public class GameController : MonoBehaviour
{
    GameState state = GameState.FreeRoam;

    PlayerMovement playerController;
    BattleSystem battleSystem;
    Camera worldCamera;
    void StartBattle(MapArea area)
{
    var hook = battleSystem.GetComponent<BattleMusicHook>();
    if (hook != null) hook.OnBattleStart();

    state = GameState.Battle;

    if (battleSystem == null)
    {
        Debug.LogWarning("[GameController] StartBattle appelé mais BattleSystem est null.");
        return;
    }

    battleSystem.gameObject.SetActive(true);
    if (worldCamera != null) worldCamera.gameObject.SetActive(false);

    var playerParty = playerController.GetComponent<PokemonParty>();

    if (area == null)
    {
        Debug.LogWarning("[GameController] MapArea null, fallback sur FindFirstObjectByType.");
        area = FindFirstObjectByType<MapArea>();
    }

    var wildPokemon = area.GetRandomWildPokemon();
    battleSystem.StartBattle(playerParty, wildPokemon);
}


    private void Awake()
    {
        ConditionsDB.Init();

        // Si ton GameController est persistant, ça garantit qu'il se rebranche à chaque scène
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // setup initial
        RefreshSceneReferences();


    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        UnhookPlayer();
        UnhookBattleSystem();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si on change de scène pendant un combat, on revient proprement en FreeRoam
        state = GameState.FreeRoam;

        // Refaire les liens vers les objets de la nouvelle scène
        RefreshSceneReferences();

        // Si la scène n'a pas de BattleSystem, s'assurer que la caméra est active
        if (battleSystem == null && worldCamera != null)
            worldCamera.gameObject.SetActive(true);
    }

    void RefreshSceneReferences()
    {
        // 1) Player persistant
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            var pm = playerGO.GetComponent<PlayerMovement>();

            // Si on avait un ancien playerController, on le décroche proprement
            if (playerController != null && playerController != pm)
                UnhookPlayer();

            playerController = pm;

            if (playerController != null)
            {
                // éviter double subscription
                playerController.OnEncountered -= StartBattle;
                playerController.OnEncountered += StartBattle;
            }
        }

        // 2) BattleSystem (peut être absent dans PokemonCenter)
        var bs = FindFirstObjectByType<BattleSystem>(FindObjectsInactive.Include);

        if (battleSystem != null && battleSystem != bs)
            UnhookBattleSystem();

        battleSystem = bs;

        if (battleSystem != null)
        {
            battleSystem.OnBattleOver -= EndBattle;
            battleSystem.OnBattleOver += EndBattle;
        }

        // 3) Caméra de la scène
        worldCamera = Camera.main;

        Debug.Log($"[GameController] Scene={SceneManager.GetActiveScene().name} | player={(playerController!=null)} | battleSystem={(battleSystem!=null)} | cam={(worldCamera!=null)}");
    }

    void UnhookPlayer()
    {
        if (playerController != null)
            playerController.OnEncountered -= StartBattle;
    }

    void UnhookBattleSystem()
    {
        if (battleSystem != null)
            battleSystem.OnBattleOver -= EndBattle;
    }

    void StartBattle()
    {
        // Si la scène n'a pas de BattleSystem, on ignore la rencontre
        if (battleSystem == null)
        {
            Debug.LogWarning("[GameController] Rencontre détectée mais aucun BattleSystem dans cette scène. (Normal si PokemonCenter)");
            return;
        }

        // Il faut aussi un MapArea pour tirer un wild
        var mapArea = FindFirstObjectByType<MapArea>();
        if (mapArea == null)
        {
            Debug.LogWarning("[GameController] BattleSystem présent mais aucun MapArea trouvé. Impossible de spawn un wild.");
            return;
        }

        if (playerController == null)
        {
            Debug.LogError("[GameController] PlayerMovement introuvable au StartBattle.");
            return;
        }

        var playerParty = playerController.GetComponent<PokemonParty>();
        if (playerParty == null)
        {
            Debug.LogError("[GameController] PokemonParty introuvable sur le Player.");
            return;
        }

        var wildPokemon = mapArea.GetRandomWildPokemon();
        if (wildPokemon == null)
        {
            Debug.LogWarning("[GameController] MapArea a retourné un wildPokemon null.");
            return;
        }

        state = GameState.Battle;

        battleSystem.gameObject.SetActive(true);
        if (worldCamera != null)
            worldCamera.gameObject.SetActive(false);

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;

        if (battleSystem != null)
            battleSystem.gameObject.SetActive(false);

        if (worldCamera != null)
            worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
            playerController?.HandleUpdate();
        else if (state == GameState.Battle)
            battleSystem?.HandleUpdate();
    }
}
