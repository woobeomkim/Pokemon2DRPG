using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils.StateMachine;

public class InventoryState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;
    
    // Output
    public ItemBase SelectedItem { get; private set; }
    public static InventoryState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    Inventory inventory;

    private void Start()
    {
        inventory = Inventory.GetInventory();
    }
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        SelectedItem = null;

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
        SelectedItem = inventoryUI.SelectedItem;

        if (gc.StateMachine.GetPrevState() != ShopSellingState.i)
            StartCoroutine(SelectPokemonAndUseItem());
        else
            gc.StateMachine.Pop();
    }

    public override void Exit()
    {
        inventoryUI.gameObject.SetActive(false);
        inventoryUI.onSelected -= OnItemSelected;
        inventoryUI.onBack -= OnBack;
    }
    void OnBack()
    {
        SelectedItem = null;
        gc.StateMachine.Pop();
    }

    IEnumerator SelectPokemonAndUseItem()
    {
        var prevState = gc.StateMachine.GetPrevState();
        if (prevState == BattleState.i)
        {
            // In Battle
            if (!SelectedItem.CanUseInBattle)
            {
                yield return DialogManager.i.ShowDialogText($"배틀중에는 사용할수 없어!");
                yield break;
            }
        }
        else
        {
            if (!SelectedItem.CanUseOutsideBattle)
            {
                yield return DialogManager.i.ShowDialogText($"배틀밖에서는 사용할수 없어!");
                yield break;
            }
        }
        if (SelectedItem is PokeballItem)
        {
            inventory.UseItem(SelectedItem, null);
            gc.StateMachine.Pop();
            yield break;
        }
        yield return gc.StateMachine.PushAndWait(PartyState.i);

        if(prevState == BattleState.i)
        {
            if (UseItemState.i.ItemUsed)
                gc.StateMachine.Pop();
        }
    }
}
