using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ShopBuyingState : State<GameController>
{
    [SerializeField] Vector2 shopCameraOffset;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    public static ShopBuyingState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public List<ItemBase> AvailableItems { get; set; }

    bool browseItems = false;
    GameController gc;

    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }
    public override void Enter(GameController owner)
    {
        gc = owner;

        browseItems = false;
        StartCoroutine(StartBuyingState());

    }
    public override void Execute()
    {
        if(browseItems)
            shopUI.HandleUpdate();
    }

    IEnumerator StartBuyingState()
    {

        yield return GameController.i.MoveCamera(shopCameraOffset);
        walletUI.Show();
        shopUI.Show(AvailableItems, (item) => StartCoroutine(BuyItem(item)),
            () => StartCoroutine(OnBackFromBuying()));

        browseItems = true;
    }

    IEnumerator BuyItem(ItemBase item)
    {
        browseItems = false;

        yield return DialogManager.i.ShowDialogText("몇개를 구매하시겠어요?",
            waitForInput: false, autoClose: false);

        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price,
            (selectedCount) => countToBuy = selectedCount);

        DialogManager.i.CloseDialog();

        float totalPrice = item.Price * countToBuy;

        if (Wallet.i.HasMoney(totalPrice))
        {
            int seletedChoice = 0;
            yield return DialogManager.i.ShowDialogText($"{totalPrice}원 입니다! 사시겠습니까?",
                waitForInput: false,
                choices: new List<string>() { "산다", "사지않는다" },
                onChoiceSelected: choicesIndex => seletedChoice = choicesIndex);
            if (seletedChoice == 0)
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

        browseItems = true;
    }

    IEnumerator OnBackFromBuying()
    {
        yield return GameController.i.MoveCamera(-shopCameraOffset);
        shopUI.Close();
        walletUI.Close();
        gc.StateMachine.Pop();
    }
}
