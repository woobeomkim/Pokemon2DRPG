using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using Utils.StateMachine;

public class ShopMenuState : State<GameController>
{
   public static ShopMenuState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    // Input
    public List<ItemBase> AvailableItems { get; set; }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        StartCoroutine(StartMenuState());
    }

    IEnumerator StartMenuState()
    {
        int seletedChoice = 0;
        yield return DialogManager.i.ShowDialogText($"무엇을 도와드릴까요?",
            waitForInput: false,
            choices: new List<string>() { "산다", "판다", "나간다" },
            onChoiceSelected: choicesIndex => seletedChoice = choicesIndex);

        if (seletedChoice == 0)
        {
            ShopBuyingState.i.AvailableItems = AvailableItems;
            yield return gc.StateMachine.PushAndWait(ShopBuyingState.i);
        }
        else if (seletedChoice == 1)
        {
            yield return gc.StateMachine.PushAndWait(ShopSellingState.i);
        }
        else if (seletedChoice == 2)
        {
       
        }
        gc.StateMachine.Pop();
    }

}
