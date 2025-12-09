using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class GamePartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    public static GamePartyState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        partyScreen.gameObject.SetActive(true);
        partyScreen.onSelected += OnPokemonSelected;
        partyScreen.onBack += OnBack;
    }

    public override void Execute()
    {
        partyScreen.HandleUpdate();
    }

    public override void Exit()
    {
        partyScreen.onSelected -= OnPokemonSelected;
        partyScreen.onBack -= OnBack;
        partyScreen.gameObject.SetActive(false);
    }
    void OnPokemonSelected(int selection)
    {
        if(gc.StateMachine.GetPrevState() == InventoryState.i)
        {
            // Use Item;
            Debug.Log("Use Items");
        }
        else
        {
            // Todo : Open Summary Screen
            Debug.Log($"Selected Pokemon at index {selection}");
        }
    }

    void OnBack()
    {
        gc.StateMachine.Pop();
    }
}
