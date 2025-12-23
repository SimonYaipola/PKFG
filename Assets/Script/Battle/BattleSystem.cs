using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using JetBrains.Annotations;
using Unity.VisualScripting;

public enum BattleState
{
    Start,
    ActionSelection,
    MoveSelection,
    RunningTurn,
    Busy,
    PartyScreen,
    BattleOver
}
public enum BattleAction {Move, SwitchPokemon, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit EnnemyUnit;

    [SerializeField] BattleDialogBox DialogBox;
    [SerializeField] PartyScreen partyScreen;

    [SerializeField] InputActionReference ActionSelectRight;
    [SerializeField] InputActionReference ActionSelectLeft;
    [SerializeField] InputActionReference ActionSelectUp;
    [SerializeField] InputActionReference ActionSelectDown;
    [SerializeField] InputActionReference ConfirmAction;
    [SerializeField] InputActionReference BackAction;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;

    int currentAction = 0;
    int currentMove = 0;
    int currentMember;
    int maxActionIndex = 4;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        Debug.Log($"[GameController] StartBattle CALLED | this={gameObject.scene.name}/{name} id={GetInstanceID()} state={state}");
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        EnnemyUnit.Setup(wildPokemon);

        partyScreen.Init();
        DialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return DialogBox.TypeDialog($"A wild {EnnemyUnit.Pokemon.Base.Name} appeared. ");
        ActionSelection();
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        StartCoroutine(DialogBox.TypeDialog("Choose an action"));
        DialogBox.EnableActionSelector(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        DialogBox.EnableActionSelector(false);
        DialogBox.EnableDialogText(false);
        DialogBox.EnablemoveSelector(true);
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return DialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} is unable to move !");
            yield break;
        }

        if (!move.TryConsumePP())
        {
            yield return DialogBox.TypeDialog($"No PP left for {move.Base.Name}!");
            ActionSelection();
            yield break;
        }

        yield return DialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PLayerAttackAnimation();
            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(
                    move.Base.Effects,
                    sourceUnit.Pokemon,
                    targetUnit.Pokemon,
                    move.Base.TargetType
                );
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return DialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return DialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed !");
        }
    }

    IEnumerator RunMoveEffects(MoveEffect effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }



    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return DialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
        }
    }
IEnumerator RunTurns(BattleAction playerAction)
{
    state = BattleState.RunningTurn;

    if (playerAction == BattleAction.Move)
    {
        playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
        EnnemyUnit.Pokemon.CurrentMove = EnnemyUnit.Pokemon.GetRandomMove();
        int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
        int enemyMovePriority = EnnemyUnit.Pokemon.CurrentMove.Base.Priority;

        bool playerGoesFirst = true;
        if (enemyMovePriority > playerMovePriority)
            playerGoesFirst = false;
        else if (enemyMovePriority == playerMovePriority)
            playerGoesFirst = playerUnit.Pokemon.Speed >= EnnemyUnit.Pokemon.Speed;


        var firstUnit = playerGoesFirst ? playerUnit : EnnemyUnit;
        var secondUnit = playerGoesFirst ? EnnemyUnit : playerUnit;

        var secondPokemon = secondUnit.Pokemon;

        yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
        yield return RunAfterTurns(firstUnit);
        if (state == BattleState.BattleOver) yield break;

        if (secondPokemon.HP > 0)
        {
            yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
            yield return RunAfterTurns(secondUnit);
            if (state == BattleState.BattleOver) yield break;
        }
    }
    else
    {
        if (playerAction == BattleAction.SwitchPokemon)
        {
            var selectedPokemon = playerParty.Pokemons[currentMember];
            state = BattleState.Busy;
            yield return SwitchPokemon(selectedPokemon);
        }

        var enemyMove = EnnemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(EnnemyUnit, playerUnit, enemyMove);
        yield return RunAfterTurns(EnnemyUnit);
        if (state == BattleState.BattleOver) yield break;
    }

    if (state != BattleState.BattleOver)
        ActionSelection();
}

    IEnumerator RunAfterTurns(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return DialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return DialogBox.TypeDialog("A critical hit !");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return DialogBox.TypeDialog("It's super effective");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return DialogBox.TypeDialog("It's not very effective");
    }
    IEnumerator TryRun()
    {
    state = BattleState.Busy;

    DialogBox.EnableActionSelector(false);
    DialogBox.EnablemoveSelector(false);
    DialogBox.EnableDialogText(true);

    // Option 1: fuite toujours réussie (pour tester)
    yield return DialogBox.TypeDialog("Got away safely!");
    yield return new WaitForSeconds(0.2f);

    BattleOver(false);
    }


    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
            HandleActionSelection();
        else if (state == BattleState.MoveSelection)
            HandleMoveSelection();
        else if (state == BattleState.PartyScreen)
            HandlePartyScreenSelection();
    }

    void HandleActionSelection()
    {
        if (ActionSelectRight.action.WasPerformedThisFrame())
        {
            if (currentAction < maxActionIndex)
                currentAction++;
        }
        else if (ActionSelectLeft.action.WasPerformedThisFrame())
        {
            if (currentAction > 0)
                currentAction--;
        }
        else if (ActionSelectDown.action.WasPerformedThisFrame())
        {
            if (currentAction < maxActionIndex - 2)
                currentAction += 2;
        }
        else if (ActionSelectUp.action.WasPerformedThisFrame())
        {
            if (currentAction > 1)
                currentAction -= 2;
        }

        DialogBox.UpdateActionSelection(currentAction);

        if (ConfirmAction.action.WasPerformedThisFrame())
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 2)
            {
            }
            else if (currentAction == 3)
            {
                StartCoroutine(TryRun());
            }
        }
    }

    void HandleMoveSelection()
    {
        if (ActionSelectRight.action.WasPerformedThisFrame())
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                currentMove++;
        }
        else if (ActionSelectLeft.action.WasPerformedThisFrame())
        {
            if (currentMove > 0)
                currentMove--;
        }
        else if (ActionSelectDown.action.WasPerformedThisFrame())
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (ActionSelectUp.action.WasPerformedThisFrame())
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        DialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (ConfirmAction.action.WasPerformedThisFrame())
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0)  return;
            
            DialogBox.EnablemoveSelector(false);
            DialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }

        if (BackAction.action.WasPerformedThisFrame())
        {
            ActionSelection();
            DialogBox.EnablemoveSelector(false);
        }
    }

    void HandlePartyScreenSelection()
    {
        if (ActionSelectRight.action.WasPerformedThisFrame())
        {
            if (currentMember < playerParty.Pokemons.Count - 1)
                currentMember++;
        }
        else if (ActionSelectLeft.action.WasPerformedThisFrame())
        {
            if (currentMember > 0)
                currentMember--;
        }
        else if (ActionSelectDown.action.WasPerformedThisFrame())
        {
            if (currentMember < playerParty.Pokemons.Count - 2)
                currentMember += 2;
        }
        else if (ActionSelectUp.action.WasPerformedThisFrame())
        {
            if (currentMember > 1)
                currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);

        if (ConfirmAction.action.WasPerformedThisFrame())
        {
            var selectedMember = playerParty.Pokemons[currentMember];

            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted Pokemon !");
                return;
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch a Pokemon with itself !");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
            }
           
        }
        else if (BackAction.action.WasPerformedThisFrame())
        {
            partyScreen.gameObject.SetActive(false);
            state = BattleState.ActionSelection;
            DialogBox.EnableActionSelector(true);
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        DialogBox.EnableActionSelector(false);
        DialogBox.EnablemoveSelector(false);
        DialogBox.EnableDialogText(true);
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return DialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name} !");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1f);
        }
        playerUnit.Setup(newPokemon);
        DialogBox.SetMoveNames(newPokemon.Moves);
        yield return DialogBox.TypeDialog($"Go {newPokemon.Base.Name} !");
        state =BattleState.RunningTurn;
    }

    void OnEnable()
    {
        ActionSelectRight.action.Enable();
        ActionSelectLeft.action.Enable();
        ActionSelectUp.action.Enable();
        ActionSelectDown.action.Enable();
        ConfirmAction.action.Enable();
        BackAction.action.Enable();
    }

    void OnDisable()
    {
        ActionSelectRight.action.Disable();
        ActionSelectLeft.action.Disable();
        ActionSelectUp.action.Disable();
        ActionSelectDown.action.Disable();
        ConfirmAction.action.Disable();
        BackAction.action.Disable();
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy >= 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion >= 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }
}


