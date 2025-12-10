using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ShopSellingState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public static ShopSellingState i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    // Input
    public List<ItemBase> AvailableItems { get; set; }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        StartCoroutine(StartSellingState());
    }

    IEnumerator StartSellingState()
    {
        yield return gc.StateMachine.PushAndWait(InventoryState.i);

        var selectedItem = InventoryState.i.SelectedItem;

        if (selectedItem != null)
        {
            yield return SellItem(selectedItem);
            StartCoroutine(StartSellingState());
        }
        else
            gc.StateMachine.Pop();
    }

    IEnumerator SellItem(ItemBase item)
    {
        if (!item.IsSellable)
        {
            yield return DialogManager.i.ShowDialogText($"이 아이템은 팔 수 없어!");
            yield break;
        }

        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        int itemCount = inventory.GetItemCount(item);
        if (itemCount > 1)
        {
            yield return DialogManager.i.ShowDialogText($"몇개를 파실건가요?", waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice, (selectedCount) => { countToSell = selectedCount; });

            DialogManager.i.CloseDialog();
        }

        sellingPrice = countToSell * sellingPrice;

        int seletedChoice = 0;
        yield return DialogManager.i.ShowDialogText($"{sellingPrice}원을 드릴수있어요! 파시겠습니까?",
            waitForInput: false,
            choices: new List<string>() { "판다", "팔지않는다" },
            onChoiceSelected: choicesIndex => seletedChoice = choicesIndex);

        if (seletedChoice == 0)
        {
            inventory.RemoveItem(item, countToSell);
            // 플레이어에 돈추가
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.i.ShowDialogText($"{item.Name}을 넘기고 {sellingPrice}원을 받았다!");
        }
        else
        {

        }

        walletUI.Close();
    }
}
