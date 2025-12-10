using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class AboutToUseToState : State<BattleSystem>
{
    // Input
    public Pokemon NewPokemon { get; set; }

    bool aboutToUseChoice;
    public static AboutToUseToState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        StartCoroutine(StartState());
    }

    IEnumerator StartState()
    {
        yield return bs.DialogBox.TypeDialog($"{bs.Trainer.Name}(이)가 {NewPokemon.Base.Name}으로 바꾸려고합니다.");
        yield return bs.DialogBox.TypeDialog($"포켓몬을 바꾸시겠습니까?");
        bs.DialogBox.EnabledChoiceBox(true);
    }

    public override void Execute()
    {
        if (!bs.DialogBox.IsChoiceBoxEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        bs.DialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            bs.DialogBox.EnabledChoiceBox(false);
            if (aboutToUseChoice)
            {
                StartCoroutine(SwitchAndContinueBattle());
            }
            else
            {
                StartCoroutine(ContinueBattle());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            bs.DialogBox.EnabledChoiceBox(false);
            StartCoroutine(ContinueBattle());
        }
    }

    IEnumerator SwitchAndContinueBattle()
    {
       yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
        var selectedPokemon = PartyState.i.SelectedPokemon;

        if(selectedPokemon != null)
        {
            yield return bs.SwitchPokemon(selectedPokemon);
        }

        yield return ContinueBattle();
    }

    IEnumerator ContinueBattle()
    {
        yield return bs.SendNextTrainerPokemon();
        bs.StateMachine.Pop();
    }
}
