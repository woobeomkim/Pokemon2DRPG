using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] ActionSelectionUI selectionUI;

    public static ActionSelectionState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        selectionUI.gameObject.SetActive(true);
        selectionUI.onSelected += OnActionSelected;

        bs.DialogBox.SetDialog("행동을 고르세요!");
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.onSelected -= OnActionSelected;
    }

    void OnActionSelected(int selection)
    {
        if(selection == 0)
        {
            bs.SelectedAction = BattleAction.Move;
            MoveSelectionState.i.Moves = bs.PlayerUnit.Pokemon.Moves;
            bs.StateMachine.ChangeState(MoveSelectionState.i);
        }
        else if(selection == 1)
        {
            StartCoroutine(GoToInventoryState());
        }
        else if(selection == 2)
        {
            StartCoroutine(GoToPartyState());
        }
        else if(selection == 3)
        {
            bs.SelectedAction = BattleAction.Run;
            bs.StateMachine.ChangeState(RunTrunState.i);
        }
    }

    IEnumerator GoToPartyState()
    {
        yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
        var selectedPokemon = PartyState.i.SelectedPokemon;
        if(selectedPokemon != null)
        {
            bs.SelectedAction = BattleAction.SwitchPokemon;
            bs.SelectedPokemon = selectedPokemon;
            bs.StateMachine.ChangeState(RunTrunState.i);
        }
    }

    IEnumerator GoToInventoryState()
    {
        yield return GameController.i.StateMachine.PushAndWait(InventoryState.i);
        var selectedItem = InventoryState.i.SelectedItem;
        if(selectedItem != null)
        {
            bs.SelectedAction = BattleAction.UseItem;
            bs.SelectedItem = selectedItem;
            bs.StateMachine.ChangeState(RunTrunState.i);
        }

    }
}
