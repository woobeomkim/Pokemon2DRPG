using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Menu,Buying,Selling,Busy}
public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    [SerializeField] ShopUI shopUI;

    public event Action onStart;
    public event Action onFinish;

    ShopState state;
    Inventory inventory;
    Merchant merchant;
    public static ShopController i { get; private set; }
    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        inventory = Inventory.GetInventory();
    }
    public IEnumerator StartTrading(Merchant merchant)
    {
        this.merchant = merchant;
        onStart?.Invoke();
        yield return StartMenuState();
    }
    IEnumerator StartMenuState()
    {
        state = ShopState.Menu;
        int seletedChoice = 0;
        yield return DialogManager.i.ShowDialogText($"무엇을 도와드릴까요?",
            waitForInput: false,
            choices: new List<string>() { "산다", "판다", "나간다" },
            onChoiceSelected: choicesIndex => seletedChoice = choicesIndex);

        if (seletedChoice == 0)
        {
            state = ShopState.Buying;
            walletUI.Show();
            shopUI.Show(merchant.AvailableItems);
        }
        else if (seletedChoice == 1)
        {
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if (seletedChoice == 2)
        {
            onFinish?.Invoke();
            yield break;
        }

    }

    public void HandleUpdate()
    {
        if(state == ShopState.Buying)
        {
            shopUI.HandleUpdate();
        }
        else if(state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling,(selectedItem) => { StartCoroutine(SellItem(selectedItem)); });
        }
    }

    void OnBackFromSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenuState());
    }

    IEnumerator SellItem(ItemBase item)
    {
        state = ShopState.Busy;

        if(!item.IsSellable)
        {
            yield return DialogManager.i.ShowDialogText($"이 아이템은 팔 수 없어!");
            state = ShopState.Selling;
            yield break;
        }

        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        int itemCount = inventory.GetItemCount(item);
        if(itemCount > 1)
        {
            yield return DialogManager.i.ShowDialogText($"몇개를 파실건가요?", waitForInput:false,autoClose:false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice, (selectedCount) => { countToSell = selectedCount; });

            DialogManager.i.CloseDialog();
        }

        sellingPrice = countToSell * sellingPrice;

        int seletedChoice = 0;
        yield return DialogManager.i.ShowDialogText($"{sellingPrice}원을 드릴수있어요! 파시겠습니까?",
            waitForInput: false,
            choices: new List<string>() { "판다", "팔지않는다"},
            onChoiceSelected: choicesIndex => seletedChoice = choicesIndex);
    
        if(seletedChoice == 0)
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
        state = ShopState.Selling;
    }
}
