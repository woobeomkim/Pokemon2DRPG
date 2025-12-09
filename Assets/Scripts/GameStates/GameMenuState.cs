using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class GameMenuState : State<GameController>
{
    [SerializeField] MenuController menuController;
    public static GameMenuState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        menuController.gameObject.SetActive(true);
        menuController.onSelected += OnMenuItemSelected;
        menuController.onBack += OnBack;
    }

    public override void Execute()
    {
        menuController.HandleUpdate();

    }

    public override void Exit()
    {
        menuController.onSelected -= OnMenuItemSelected;
        menuController.onBack -= OnBack;
        menuController.gameObject.SetActive(false);
    }
    void OnMenuItemSelected(int selection)
    {
        if(selection == 0)
        {
            gc.StateMachine.Push(GamePartyState.i);
        }
        else if(selection == 1)
        {
            gc.StateMachine.Push(InventoryState.i);
        }

    }

    void OnBack()
    {
        gc.StateMachine.Pop();
    }
}
