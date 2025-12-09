using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils.StateMachine;

public class UseItemState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    public static UseItemState i { get; private set; }

    Inventory inventory;
    private void Awake()
    {
        i = this;
        inventory = Inventory.GetInventory();
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        StartCoroutine(UseItem());
    }

    IEnumerator UseItem()
    {
        var item = inventoryUI.SelectedItem;
        var pokemon = partyScreen.SelectedMember;

        if(item is TmItem)
        {
            yield return HandlTmItems();
        }
        else
        {
            if (item is EvolutionItem)
            {
                var evolution = pokemon.CheckForEvolution(item);
                if (evolution != null)
                {
                    yield return EvolutionManager.i.Evolve(pokemon, evolution);
                }
                else
                {
                    yield return DialogManager.i.ShowDialogText($"효과가 없을것 같다!");
                    gc.StateMachine.Pop();
                    yield break;
                }
            }

            var usedItem = inventory.UseItem(item, partyScreen.SelectedMember);

            if (usedItem != null)
            {
                if (usedItem is RecoveryItem)
                    yield return DialogManager.i.ShowDialogText($"{usedItem.Name}을 사용하였다!");
            }
            else
            {
                if (inventoryUI.SelectedCategory == (int)ItemCategory.Items)
                    yield return DialogManager.i.ShowDialogText($"효과가 없을것 같다!");
            }
        }

        gc.StateMachine.Pop();
    }

    IEnumerator HandlTmItems()
    {
        var tmItem = inventoryUI.SelectedItem as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name}(이)가 이미 {tmItem.Move.Name}을 배웠다!");
            yield break;
        }

        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name}(이)가 {tmItem.Move.Name}을 배울 수 없다!");
            yield break;
        }
        if (pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name}(이)가 {tmItem.Move.Name}을 배웠다!");
        }
        else
        {
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name}(이)가 {tmItem.Move.Name}을 배우려고한다!");
            yield return DialogManager.i.ShowDialogText($"그러나, 기술을 {PokemonBase.MaxNumOfMoves}개만큼 배우지 못한다.");
            yield return DialogManager.i.ShowDialogText($"{tmItem.Move.Name}을 배우시겠습니까?");
            // yield return ChooseMoveToForget(pokemon, tmItem.Move);
            // yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);

        }

    }
}
