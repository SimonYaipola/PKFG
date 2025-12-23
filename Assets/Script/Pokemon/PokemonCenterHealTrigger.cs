using UnityEngine;
using UnityEngine.InputSystem;

public class PokemonCenterHealTrigger : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference confirmAction; // ton action "Z" (Confirm)

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip healSfx;

    [Header("Optional")]
    [SerializeField] private bool disableMovementDuringHeal = true;
    [SerializeField] private float freezeSeconds = 1.0f;

    private bool playerInZone;
    private PlayerMovement playerMovement;
    private PokemonParty party;

    private bool used;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        confirmAction?.action?.Enable();
    }

    private void OnDisable()
    {
        confirmAction?.action?.Disable();
    }

    private void Update()
    {
        if (!playerInZone || used) return;

        // Input: Z (via Input System)
        if (confirmAction != null && confirmAction.action.WasPerformedThisFrame())
        {
            TryHeal();
        }
        else
        {
            // Fallback si jamais t’as pas assigné l’InputAction
            if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame)
                TryHeal();
        }
    }

    private void TryHeal()
    {
        if (party == null)
        {
            Debug.LogWarning("[PokemonCenter] No PokemonParty found on Player.");
            return;
        }

        used = true;

        if (disableMovementDuringHeal && playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        party.HealAll();

        if (audioSource != null && healSfx != null)
            audioSource.PlayOneShot(healSfx);

        Debug.Log("[PokemonCenter] Team healed (Z pressed).");

        if (disableMovementDuringHeal && playerMovement != null)
            Invoke(nameof(ReEnableMovement), freezeSeconds);

        // petit cooldown anti-spam (au cas où)
        Invoke(nameof(ResetUse), 0.2f);
    }

    private void ReEnableMovement()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);
    }

    private void ResetUse()
    {
        used = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ton setup: c’est souvent FeetCollider qui touche, tag pas toujours Player.
        // On va remonter au parent "Player"
        var playerRoot = other.transform.root;

        if (playerRoot.CompareTag("Player"))
        {
            playerInZone = true;
            playerMovement = playerRoot.GetComponent<PlayerMovement>();
            party = playerRoot.GetComponent<PokemonParty>();

            Debug.Log("[PokemonCenter] Player entered heal zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var playerRoot = other.transform.root;

        if (playerRoot.CompareTag("Player"))
        {
            playerInZone = false;
            playerMovement = null;
            party = null;

            Debug.Log("[PokemonCenter] Player exited heal zone.");
        }
    }
}
