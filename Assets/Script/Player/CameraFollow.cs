using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] string targetTag = "Player";
    [SerializeField] Vector3 offset = new Vector3(0f, 0f, -10f);

    Transform target;

    void Awake()
    {
        Debug.Log($"[CameraFollow] Attached to: {gameObject.name} | enabled={enabled}");
    }

    void Start()
    {
        FindTarget();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        transform.position = target.position + offset;
    }

    void FindTarget()
    {
        var go = GameObject.FindGameObjectWithTag(targetTag);
        if (go == null)
        {
            Debug.LogWarning("[CameraFollow] Player not found by tag: " + targetTag);
            return;
        }

        target = go.transform;
        Debug.Log("[CameraFollow] Target set to: " + go.name);
    }
    public void SnapNow()
{
    if (target == null) return;
    transform.position = target.position + offset;
}

}
