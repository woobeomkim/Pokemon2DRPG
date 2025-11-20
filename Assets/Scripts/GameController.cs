using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam,Battle,Dialog,Cutscene}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] Camera mainCamera;
    [SerializeField] BattleSystem bs;

    GameState state;

    public static GameController i { get; private set; }

    private void Awake()
    {
        i = this;
        ConditionsDB.Init();
    }

    private void Start()
    {
        player.onEncounter += StartBattle;
        bs.onBattleOver += EndBattle;

        player.onEnterTrainersView += (Collider2D trainerCollider) =>
        {
            var trainer = trainerCollider.GetComponentInParent<TrainerController>();

            if (trainer != null)
            {
                state = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(player));
            }
        } ;

        DialogManager.i.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.i.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };
    }

    void StartBattle()
    {
        state = GameState.Battle;
        mainCamera.gameObject.SetActive(false);
        bs.gameObject.SetActive(true);

        var playerParty = player.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        bs.StartBattle(playerParty, wildPokemon);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        mainCamera.gameObject.SetActive(false);
        bs.gameObject.SetActive(true);

        this.trainer = trainer;
        var playerParty = player.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        bs.StartTrainerBattle(playerParty, trainerParty);
    }

    void EndBattle(bool won)
    {
        if(trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        mainCamera.gameObject.SetActive(true);
        bs.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(state == GameState.FreeRoam)
        {
            player.HandleUpdate();
        }
        else if(state == GameState.Battle)
        {
            bs.HandleUpdate();
        }
        else if(state == GameState.Dialog)
        {
            DialogManager.i.HandleUpdate();
        }
    }
}
