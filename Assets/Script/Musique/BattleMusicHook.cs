using UnityEngine;

public class BattleMusicHook : MonoBehaviour
{
    [SerializeField] private BattleSystem battleSystem;

    private void Awake()
    {
        if (battleSystem == null)
            battleSystem = GetComponent<BattleSystem>();
    }

    private void OnEnable()
    {
        if (battleSystem != null)
            battleSystem.OnBattleOver += HandleBattleOver;
    }

    private void OnDisable()
    {
        if (battleSystem != null)
            battleSystem.OnBattleOver -= HandleBattleOver;
    }

    // Appelé par GameController juste avant StartBattle() (ou par toi)
    public void OnBattleStart()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.StartBattleMusic();
    }

    private void HandleBattleOver(bool won)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.EndBattleMusic();
    }
}
