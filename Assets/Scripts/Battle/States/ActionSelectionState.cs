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
            MoveSelectionState.i.Moves = bs.PlayerUnit.Pokemon.Moves;
            bs.StateMachine.ChangeState(MoveSelectionState.i);
        }
    }
}
