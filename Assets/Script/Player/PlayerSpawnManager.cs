using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager Instance { get; private set; }

    [HideInInspector] public string NextSpawnId;

    private void Awake()
    {
        Debug.Log("[SpawnManager] Awake");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SpawnManager] Duplicate instance detected -> Destroying this one");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[SpawnManager] Instance set + DontDestroyOnLoad applied");

        // Important: éviter double subscribe
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        Debug.Log("[SpawnManager] OnDestroy -> Unsub sceneLoaded");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SpawnManager] SceneLoaded: '{scene.name}' mode={mode} NextSpawnId='{NextSpawnId}'");

        if (string.IsNullOrEmpty(NextSpawnId))
        {
            Debug.Log("[SpawnManager] NextSpawnId empty -> nothing to do");
            return;
        }

        // 1) trouver player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[SpawnManager] Player tag 'Player' not found in scene after load!");
            return;
        }
        Debug.Log($"[SpawnManager] Player found: '{player.name}'");

        // 2) trouver spawnpoints
        var spawnPoints = FindObjectsOfType<SpawnPoint>(true);
        Debug.Log($"[SpawnManager] SpawnPoints found: {spawnPoints.Length}");

        SpawnPoint chosen = null;
        foreach (var sp in spawnPoints)
        {
            Debug.Log($"[SpawnManager] SpawnPoint candidate: '{sp.name}' id='{sp.spawnId}' pos={sp.transform.position}");
            if (sp.spawnId == NextSpawnId)
                chosen = sp;
        }

        if (chosen == null)
        {
            Debug.LogError($"[SpawnManager] No SpawnPoint matched id='{NextSpawnId}'. Vérifie orthographe/spaces/case.");
            return;
        }

        Debug.Log($"[SpawnManager] MATCH -> '{chosen.name}' id='{chosen.spawnId}' pos={chosen.transform.position}");

        // 3) TP
        player.transform.position = chosen.transform.position;
        Debug.Log($"[SpawnManager] TP OK -> Player at {player.transform.position}");

        // 4) réactiver mouvement
        var pm = player.GetComponent<PlayerMovement>();
        if (pm == null)
        {
            Debug.LogWarning("[SpawnManager] PlayerMovement not found, can't re-enable movement");
        }
        else
        {
            Debug.Log("[SpawnManager] Re-enabling movement AFTER spawn");
            pm.SetMovementEnabled(true);
        }

        // 5) reset
        Debug.Log("[SpawnManager] Clearing NextSpawnId");
        NextSpawnId = null;
    }
}
