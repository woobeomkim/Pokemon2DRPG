using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Menu,Buying,Selling,Busy}
public class ShopController : MonoBehaviour
{
    [SerializeField] Vector2 shopCameraOffset;
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
            yield return GameController.i.MoveCamera(shopCameraOffset);
            walletUI.Show();
            shopUI.Show(merchant.AvailableItems,(item) => StartCoroutine(BuyItem(item)),
                () => StartCoroutine(OnBackFromBuying()));
            state = ShopState.Buying;
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
        if (state == ShopState.Buying)
        {
            shopUI.HandleUpdate();
        }
        else if (state == ShopState.Selling)
        {
            inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => { StartCoroutine(SellItem(selectedItem)); });
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

        if (!item.IsSellable)
        {
            yield return DialogManager.i.ShowDialogText($"이 아이템은 팔 수 없어!");
            state = ShopState.Selling;
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
        state = ShopState.Selling;
    }

    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        yield return DialogManager.i.ShowDialogText("몇개를 구매하시겠어요?",
            waitForInput: false, autoClose: false);

        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price,
            (selectedCount) => countToBuy = selectedCount);

        DialogManager.i.CloseDialog();

        float totalPrice = item.Price* countToBuy;

        if(Wallet.i.HasMoney(totalPrice))
        {
            int seletedChoice = 0;
            yield return DialogManager.i.ShowDialogText($"{totalPrice}원 입니다! 사시겠습니까?",
                waitForInput: false,
                choices: new List<string>() { "산다", "사지않는다" },
                onChoiceSelected: choicesIndex => seletedChoice = choicesIndex);
            if(seletedChoice == 0)
            {
                // BUY ITEM
                inventory.AddItem(item, countToBuy);
                Wallet.i.TakeMoney(totalPrice);
                yield return DialogManager.i.ShowDialogText("이용해 주셔서 감사합니다!");
            }
            else if (seletedChoice == 1)
            {

            }
        }
        else
        {
            yield return DialogManager.i.ShowDialogText("돈이 충분하지 않은거같아!");
        }

        state = ShopState.Buying;
    }

    IEnumerator OnBackFromBuying()
    {
        yield return GameController.i.MoveCamera(-shopCameraOffset, true);
        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMenuState());
    }
}
