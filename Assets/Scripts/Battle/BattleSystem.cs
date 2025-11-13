using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, PartyScreen ,Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud playerHud;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialog dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> onEndBattle;

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
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(wildPokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"야생의 {enemyUnit.Pokemon.Base.Name}(이)가 나타났다!");

        PlayerAction();
    }

    IEnumerator PlayerPerformMove()
    {
        state = BattleState.Busy;

        var move = playerUnit.Pokemon.Moves[currentMove];
        move.PP--;
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {move.Base.Name}을 사용하였다!");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1.0f);

        enemyUnit.PlayHitAnimation();

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);
    
        if(damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}(이)가 기절했다!");
            enemyUnit.PlayFaintAniamtion();
            yield return new WaitForSeconds(2.0f);
            onEndBattle?.Invoke(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}(이)가 {move.Base.Name}을 사용하였다!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1.0f);

        playerUnit.PlayHitAnimation();

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 기절했다!");
            playerUnit.PlayFaintAniamtion();
            yield return new WaitForSeconds(2.0f);

            var nextPokemon = playerParty.GetHealthPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                onEndBattle?.Invoke(false);
            }
        }
        else
        {
            PlayerAction();
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

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.EnabledDialogText(true);
        dialogBox.EnabledActionSelector(true);

        dialogBox.SetDialog("행동을 고르세요!");
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
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

    public void HandleUpdate()
    {
        if(state == BattleState.PlayerAction)
        {
            HandleActionUpdate();
        }
        else if(state == BattleState.PlayerMove)
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
                PlayerMove();
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
            StartCoroutine(PlayerPerformMove());
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnabledMoveSelector(false);
            dialogBox.EnabledDialogText(true);
            PlayerAction();
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
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"돌아와! {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAniamtion();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newPokemon);
        playerHud.SetData(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"가라! {newPokemon.Base.Name}");

        StartCoroutine(EnemyMove());
    }
}
