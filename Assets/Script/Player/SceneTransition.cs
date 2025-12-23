using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string spawnIdInNextScene;

    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[SceneTransition] Trigger ENTER by '{other.name}' tag='{other.tag}' parent='{other.transform.root.name}'");

        if (used)
        {
            Debug.Log("[SceneTransition] Already used -> return");
            return;
        }

        // IMPORTANT: FeetCollider est child, donc on remonte au Player (root)
        var playerRoot = other.transform.root;
        if (playerRoot == null)
        {
            Debug.LogWarning("[SceneTransition] playerRoot is null -> return");
            return;
        }

        if (!playerRoot.CompareTag("Player"))
        {
            Debug.Log($"[SceneTransition] Root isn't Player (root tag='{playerRoot.tag}') -> return");
            return;
        }

        used = true;

        var pm = playerRoot.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            Debug.Log("[SceneTransition] Disabling movement BEFORE load");
            pm.SetMovementEnabled(false);
        }
        else
        {
            Debug.LogWarning("[SceneTransition] PlayerMovement not found on Player root");
        }

        if (PlayerSpawnManager.Instance == null)
        {
            Debug.LogError("[SceneTransition] PlayerSpawnManager.Instance is NULL. (Il existe pas dans ta scène / pas DontDestroyOnLoad?)");
        }
        else
        {
            Debug.Log($"[SceneTransition] Setting NextSpawnId='{spawnIdInNextScene}'");
            PlayerSpawnManager.Instance.NextSpawnId = spawnIdInNextScene;
        }

        Debug.Log($"[SceneTransition] Loading scene '{sceneToLoad}'");
        SceneManager.LoadScene(sceneToLoad);
    }
}
