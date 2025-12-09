using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class BattleState : State<GameController>
{
    [SerializeField] BattleSystem bs;

    // Input
    public BattleTrigger Trigger { get; set; }
    
    public TrainerController Trainer { get; set; }
    
    public static BattleState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        gc.WorldCamera.gameObject.SetActive(false);
        bs.gameObject.SetActive(true);

        var playerParty = gc.Player.GetComponent<PokemonParty>();

        if (Trainer == null)
        {
            var wildPokemon = gc.CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(Trigger);
            var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
            bs.StartBattle(playerParty, wildPokemonCopy, Trigger);
        }
        else
        {
            var trainerParty = Trainer.GetComponent<PokemonParty>();
            bs.StartTrainerBattle(playerParty, trainerParty);
        }

        bs.onBattleOver += OnBattleOver;
    }

    public override void Execute()
    {
        bs.HandleUpdate();
    }

    public override void Exit()
    {
        gc.WorldCamera.gameObject.SetActive(true);
        bs.gameObject.SetActive(false);
        bs.onBattleOver -= OnBattleOver;
    }

    void OnBattleOver(bool won)
    {
        if (Trainer != null && won == true)
        {
            Trainer.BattleLost();
            Trainer = null;
        }

        gc.StateMachine.Pop();
    }
}
