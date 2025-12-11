using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils.StateMachine;

public class SummaryState : State<GameController>
{
    [SerializeField] SummaryScreenUI summaryScreen;
    public static SummaryState i { get; set; }

    // Input
    public int SelectedPokemonIndex { get; set; }

    private void Awake()
    {
        i = this;
    }

    List<Pokemon> playerParty;

    private void Start()
    {
        playerParty = PlayerController.i.GetComponent<PokemonParty>().Pokemons;
    }
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        summaryScreen.gameObject.SetActive(true);
        summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
        summaryScreen.SetSkills();
    }

    public override void Execute()
    {
        if (Input.GetButtonDown("Back"))
        {
            gc.StateMachine.Pop();
            return;
        }

        int prevSelection = SelectedPokemonIndex;
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectedPokemonIndex++;
            if (SelectedPokemonIndex >= playerParty.Count)
                SelectedPokemonIndex = 0;

        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            SelectedPokemonIndex -= 1;
            if (SelectedPokemonIndex <= 0)
                SelectedPokemonIndex = playerParty.Count - 1;
        }

        if(SelectedPokemonIndex != prevSelection)
        {
            summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
            summaryScreen.SetSkills();

        }
    }

    public override void Exit()
    {
        summaryScreen.gameObject.SetActive(false);
    }
}
