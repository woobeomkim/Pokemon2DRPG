using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class InventoryState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;
    public static InventoryState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        inventoryUI.gameObject.SetActive(true);
        inventoryUI.onSelected += OnItemSelected;
        inventoryUI.onBack += OnBack;
    }

    public override void Execute()
    {
        inventoryUI.HandleUpdate();
    }

    void OnItemSelected(int selection)
    {
        gc.StateMachine.Push(PartyState.i);
    }

    public override void Exit()
    {
        inventoryUI.gameObject.SetActive(false);
        inventoryUI.onSelected -= OnItemSelected;
        inventoryUI.onBack -= OnBack;
    }
    void OnBack()
    {
        gc.StateMachine.Pop();
    }
}
