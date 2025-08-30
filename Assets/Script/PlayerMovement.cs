using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // nouveau système

public class PlayerMovement : MonoBehaviour
{
    public float movespeed = 5f;

    private bool isMoving;
    private Vector2 input;

    [Header("Input System")]
    public InputActionReference moveAction; // référence à l’action "Move"

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
    }

    void Update()
    {
        if (!isMoving)
        {
            // lire la valeur de l’action
            input = moveAction.action.ReadValue<Vector2>();

            // enlever les diagonales → priorité horizontale
            if (input.x != 0) input.y = 0;

            // si le joueur appuie sur une touche
            if (input != Vector2.zero)
            {
                Vector3 targetPos = transform.position;
                targetPos.x += Mathf.Sign(input.x);
                targetPos.y += Mathf.Sign(input.y);

                StartCoroutine(Move(targetPos));
            }
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                movespeed * Time.deltaTime
            );
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }
}
