using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection,PartySelection,MoveToForget,Busy}
public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;
    [SerializeField] Text categoryText;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    int selectedItem = 0;
    int selectedCategory = 0;

    Inventory inventory;

    InventoryUIState state;

    Action<ItemBase> onItemUsed;

    const int itemsInViewport = 8;

    MoveBase moveToLearn;

    List<ItemSlotUI> slotUIList;
    RectTransform itemListRect;
    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.onUpdated += UpdateItemList;
    }

    public void UpdateItemList()
    {
        // Clear All Items

        foreach(Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow))
                ++selectedItem;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                --selectedItem;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                ++selectedCategory;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --selectedCategory;

            if (selectedCategory > Inventory.ItemCategories.Count - 1)
                selectedCategory = 0;
            else if (selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;

                selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if (prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }
            else if (prevSelection != selectedItem)
                UpdateItemSelection();
            
            if(Input.GetKeyDown(KeyCode.Z))
            {
                StartCoroutine(ItemSelected());
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                // Todo : Use Item
                StartCoroutine(UseItem());
            };

            Action onBackPartySeletion = () =>
            {
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelected, onBackPartySeletion);
        }
        else if(state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if(GameController.i.State == GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }

        if(GameController.i.State == GameState.Battle)
        {
            // In Battle
            if(!item.CanUseInBattle)
            {
                yield return DialogManager.i.ShowDialogText($"이 아이템은 배틀에서 쓸수없어!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            // Outside 
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.i.ShowDialogText($"이 아이템은 밖에서 쓸수없어!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        if (selectedCategory == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();

            if (item is TmItem)
                partyScreen.ShowIfTmIsUsable(item as TmItem);

        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        yield return HandlTmItems();

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var pokemon = partyScreen.SelectedMember;

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
                ClosePartyScreen();
                yield break;
            }
        }
        
        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);

        if(usedItem != null)
        {
            if(usedItem is RecoveryItem)
                yield return DialogManager.i.ShowDialogText($"{usedItem.Name}을 사용하였다!");
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if(selectedCategory == (int)ItemCategory.Items)
                yield return DialogManager.i.ShowDialogText($"효과가 없을것 같다!");
        }

        ClosePartyScreen();
    }

    IEnumerator HandlTmItems()
    {
       var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TmItem;
        if (tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;
        
        if(pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name}(이)가 이미 {tmItem.Move.Name}을 배웠다!");
            yield break;
        }

        if(!tmItem.CanBeTaught(pokemon))
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
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);

        }
        
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.i.ShowDialogText("잊으려는 기술을 고르세요!", autoClose: false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;
    }

    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);
        
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }
        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport / 2;
        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;

        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void ResetSelection()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }
    
    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.ClearMemberSlotMessage();
        partyScreen.gameObject.SetActive(false);
    }
    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        DialogManager.i.CloseDialog();
        moveSelectionUI.gameObject.SetActive(false);
        if (moveIndex == PokemonBase.MaxNumOfMoves)
        {
            // Dont' learn move
            yield return DialogManager.i.ShowDialogText($"{partyScreen.SelectedMember.Base.Name}(이)가 {moveToLearn.Name}을 배우지 않았다!");
        }
        else
        {
            // forget selecetedmove and learn new move
            var selectedMove = partyScreen.SelectedMember.Moves[moveIndex].Base;
            yield return DialogManager.i.ShowDialogText($"{partyScreen.SelectedMember.Base.Name}(이)가 {selectedMove.Name}을 잊고 {moveToLearn.Name}을 배웠다!");

            partyScreen.SelectedMember.Moves[moveIndex] = new Move(moveToLearn);

        }
        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}
