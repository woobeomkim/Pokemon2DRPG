using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    
    public Pokemon SelectedPokemon { get; private set; }

    public static PartyState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        SelectedPokemon = null;
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
        SelectedPokemon = partyScreen.SelectedMember;

        var prevState = gc.StateMachine.GetPrevState();
        if(prevState == InventoryState.i)
        {
            // Use Item;
            StartCoroutine(GoToUseItemState());
        }
        else if(prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;

            if (SelectedPokemon.HP <= 0)
            {
                partyScreen.SetMessageText($"기절한 포켓몬은 내보낼 수 없습니다!");
                return;
            }

            if (SelectedPokemon == battleState.BattleSystem.PlayerUnit.Pokemon)
            {
                partyScreen.SetMessageText($"같은 포켓몬은 내보낼 수 없습니다!");
                return;
            }

            gc.StateMachine.Pop();
        }
        else
        {
            // Todo : Open Summary Screen
            Debug.Log($"Selected Pokemon at index {selection}");
        }
    }

    IEnumerator GoToUseItemState()
    {
        yield return gc.StateMachine.PushAndWait(UseItemState.i);
        gc.StateMachine.Pop();
    }

    void OnBack()
    {
        SelectedPokemon = null;

        var prevState = gc.StateMachine.GetPrevState();
        if(prevState == BattleState.i)
        {
            var battleState = prevState as BattleState;
            if (battleState.BattleSystem.PlayerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("계속하려면 포켓몬을 고르세요!");
                return;
            }
        }
        gc.StateMachine.Pop();
    }
}
