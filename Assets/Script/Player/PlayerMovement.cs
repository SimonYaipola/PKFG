using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System;

public class PlayerMovement : MonoBehaviour
{
    private const float INPUT_DEADZONE = 0.3f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.8f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;

    [Header("Direction par défaut")]
    [SerializeField] private Vector2 defaultFacing = Vector2.down;

    [Header("Collision")]
    [SerializeField] private LayerMask solidObjects;
    [SerializeField] private BoxCollider2D hitbox;

    [Header("Grass")]
    [SerializeField] private LayerMask grassLayer;
    [SerializeField, Range(0f, 1f)] private float encounterChance = 0.08f;

    public event Action<MapArea> OnEncountered;

    private Animator animator;
    private bool isMoving;
    private bool canMove = true;
    private Vector2 lastMoveDir;
    private Coroutine moveRoutine;

    // =========================
    // INIT
    // =========================
    private void Awake()
    {
        animator = GetComponent<Animator>();

        lastMoveDir = defaultFacing.sqrMagnitude > 0.01f
            ? defaultFacing.normalized
            : Vector2.down;

        if (hitbox == null)
            hitbox = GetComponentInChildren<BoxCollider2D>();

        if (solidObjects.value == 0)
            solidObjects = LayerMask.GetMask("SolidObject");

        Debug.Log($"[PlayerMovement] Awake on '{name}'");
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        sprintAction?.action.Enable();

        Debug.Log($"[PlayerMovement] OnEnable on '{name}'");
    }

    // ⚠️ IMPORTANT : ON NE DÉSACTIVE PLUS LES INPUTS ICI
    private void OnDisable()
    {
        Debug.Log($"[PlayerMovement] OnDisable on '{name}' (inputs preserved)");
    }

    // =========================
    // EXTERNAL CONTROL
    // =========================
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        Debug.Log($"[PlayerMovement] SetMovementEnabled({enabled})");

        if (!enabled)
        {
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }

            isMoving = false;

            animator.SetBool("isWalking", false);
            animator.SetBool("isSprinting", false);
            animator.SetFloat("moveX", lastMoveDir.x);
            animator.SetFloat("moveY", lastMoveDir.y);
        }

        // flush input fantôme
        moveAction?.action?.Reset();
        sprintAction?.action?.Reset();
    }

    // =========================
    // UPDATE
    // =========================
    public void HandleUpdate()
    {
        if (!canMove || isMoving || moveAction == null)
            return;

        Vector2 inputRaw = moveAction.action.ReadValue<Vector2>();
        bool sprinting = sprintAction.action.IsPressed();

        UpdateAnimator(inputRaw, sprinting);

        if (inputRaw.magnitude < INPUT_DEADZONE)
            return;

        Vector2 input = Mathf.Abs(inputRaw.x) > Mathf.Abs(inputRaw.y)
            ? new Vector2(Mathf.Sign(inputRaw.x), 0)
            : new Vector2(0, Mathf.Sign(inputRaw.y));

        Vector3 targetPos = transform.position + (Vector3)input;

        if (!IsWalkable(targetPos))
            return;

        float speed = moveSpeed * (sprinting ? sprintMultiplier : 1f);
        moveRoutine = StartCoroutine(Move(targetPos, speed));
    }

    // =========================
    // MOVE
    // =========================
    IEnumerator Move(Vector3 targetPos, float speed)
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            if (!canMove) yield break;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                speed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
        moveRoutine = null;

        CheckForEncounters();
    }

    // =========================
    // HELPERS
    // =========================
    bool IsWalkable(Vector3 targetPos)
    {
        Vector2 center = (Vector2)targetPos + hitbox.offset;
        return Physics2D.OverlapBox(center, hitbox.size, 0f, solidObjects) == null;
    }
    private void CheckForEncounters()
{
    // On check si on est dans une herbe qui a un collider sur grassLayer
    var grassCol = Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer);

    if (grassCol == null)
        return;

    // Trouve le MapArea associé à CETTE herbe-là (sur le même GO ou un parent)
    var area = grassCol.GetComponent<MapArea>();
    if (area == null)
        area = grassCol.GetComponentInParent<MapArea>();

    if (area == null)
    {
        Debug.LogWarning("[PlayerMovement] Encounter in grass, mais aucun MapArea trouvé sur l'objet d'herbe.");
        return;
    }

    if (UnityEngine.Random.value <= encounterChance)
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isSprinting", false);

        OnEncountered?.Invoke(area);
    }
}


    void UpdateAnimator(Vector2 input, bool sprinting)
    {
        if (input.magnitude < INPUT_DEADZONE)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isSprinting", false);
            animator.SetFloat("moveX", lastMoveDir.x);
            animator.SetFloat("moveY", lastMoveDir.y);
            return;
        }

        Vector2 dir = Mathf.Abs(input.x) > Mathf.Abs(input.y)
            ? new Vector2(Mathf.Sign(input.x), 0)
            : new Vector2(0, Mathf.Sign(input.y));

        lastMoveDir = dir;

        animator.SetFloat("moveX", dir.x);
        animator.SetFloat("moveY", dir.y);
        animator.SetBool("isWalking", !sprinting);
        animator.SetBool("isSprinting", sprinting);
    }
}
