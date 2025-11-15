using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, PartyScreen ,BattleOver ,Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialog dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> onBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;

        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"야생의 {enemyUnit.Pokemon.Base.Name}(이)가 나타났다!");

        ChooseFirstTurn();
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if (state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PerformMove)
            ActionSeletion();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}(이)가 {move.Base.Name}(을)를 사용하였다!");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1.0f);

        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Pokemon, targetUnit.Pokemon);
        }
        else
        {
            var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }
        if (targetUnit.Pokemon.HP <= 0) 
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name}(이)가 기절했다!");
            targetUnit.PlayFaintAniamtion();
            yield return new WaitForSeconds(2.0f);

            CheckForBattleOver(targetUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        var effects = move.Base.Effects;
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self)
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0) 
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if(faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthPokemon();
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

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog($"급소를 때린것같다!");

        if(damageDetails.TypeEffective >1)
            yield return dialogBox.TypeDialog($"매우 효과적이다!");
        else if(damageDetails.TypeEffective <1)
            yield return dialogBox.TypeDialog($"효과가 없는것 같다!");
    }

    void ActionSeletion()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnabledDialogText(true);
        dialogBox.EnabledActionSelector(true);

        dialogBox.SetDialog("행동을 고르세요!");
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnabledActionSelector(false);
        dialogBox.EnabledDialogText(false);
        dialogBox.EnabledMoveSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
            ActionSeletion();
        else
            StartCoroutine(EnemyMove());
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        onBattleOver?.Invoke(won);
    }
    public void HandleUpdate()
    {
        if(state == BattleState.ActionSelection)
        {
            HandleActionUpdate();
        }
        else if(state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if(state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionUpdate()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentAction += 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentAction -= 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, dialogBox.ActionTexts.Count - 1);

        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(currentAction == 0)
            {
                MoveSelection();
            }
            else if(currentAction == 1)
            {
                // Bag
            }
            else if(currentAction == 2)
            {
                // PartyScreen
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Run
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove += 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove -= 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);
    
        if(Input.GetKeyDown(KeyCode.Z))
        {
            // ATTACK
            //Debug.Log($"Attack {playerUnit.Pokemon.Moves[currentMove].Base.Name}");
            dialogBox.EnabledMoveSelector(false);
            dialogBox.EnabledDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnabledMoveSelector(false);
            dialogBox.EnabledDialogText(true);
            ActionSeletion();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMember += 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMember -= 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText($"기절한 포켓몬은 내보낼 수 없습니다!");
                return;
            }

            if(selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText($"같은 포켓몬은 내보낼 수 없습니다!");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        bool currentPokemonFainted = true;
        if (playerUnit.Pokemon.HP > 0)
        {
            currentPokemonFainted = false;
            yield return dialogBox.TypeDialog($"돌아와! {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAniamtion();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newPokemon);
        playerUnit.Hud.SetData(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"가라! {newPokemon.Base.Name}");

        if (currentPokemonFainted)
            ChooseFirstTurn();
        else
            StartCoroutine(EnemyMove());
    }
}
