using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam,Battle,Dialog,Menu ,PartyScreen , Bag ,Cutscene, Paused,Evolution,Shop}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] Camera worldCamera;
    [SerializeField] BattleSystem bs;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    GameState state;

    GameState prevState;
    GameState stateBeforeEvolution;

    public GameState State => state;
    public static GameController i { get; private set; }

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    MenuController menuController;
    private void Awake()
    {
        i = this;

        menuController = GetComponent<MenuController>();
        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        bs.onBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.i.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };

        DialogManager.i.OnDialogFinished += () =>
        {
            if (state == GameState.Dialog)
                state = prevState;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;

        EvolutionManager.i.onStartEvolution += () =>
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
        };

        EvolutionManager.i.onCompleteEvolution += () =>
        {
            partyScreen.SetPartyData();
            state = stateBeforeEvolution;

            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        };

        ShopController.i.onStart += () => { state = GameState.Shop; };
        ShopController.i.onFinish += () => { state = GameState.FreeRoam; };
    }

    public void PausedGame(bool pause)
    {
        if(pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartCutsceneState()
    {
        state = GameState.Cutscene;
    }

    public void StartFreeRoamState()
    {
        state = GameState.FreeRoam;
    }

    public void StartBattle(BattleTrigger trigger)
    {
        state = GameState.Battle;
        worldCamera.gameObject.SetActive(false);
        bs.gameObject.SetActive(true);

        var playerParty = player.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(trigger);

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        bs.StartBattle(playerParty, wildPokemonCopy,trigger);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        worldCamera.gameObject.SetActive(false);
        bs.gameObject.SetActive(true);

        this.trainer = trainer;
        var playerParty = player.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        bs.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(player));
    }
    void EndBattle(bool won)
    {
        if(trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        partyScreen.SetPartyData();

        state = GameState.FreeRoam;
        worldCamera.gameObject.SetActive(true);
        bs.gameObject.SetActive(false);

        var playerParty = player.GetComponent<PokemonParty>();
        bool hasEvolution = playerParty.CheckForEvolution();

        if (hasEvolution)
            StartCoroutine(playerParty.RunEvolution());
        else
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
    }

    private void Update()
    {
        if(state == GameState.FreeRoam)
        {

            player.HandleUpdate();
            if(Input.GetKeyDown(KeyCode.Return))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if(state == GameState.Cutscene)
        {
            player.Character.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            bs.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.i.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                // TODO : Summary Screen
            };

            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };
            inventoryUI.HandleUpdate(onBack);
        }
        else if (state == GameState.Shop)
        {
            ShopController.i.HandleUpdate();
        }
      
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem)
    {
        if(selectedItem == 0)
        {
            //Pokemon
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            // Bag
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            // Save
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if(selectedItem == 3)
        {
            // Load
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
    }
    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut = false)
    {
        yield return Fader.i.FadeIn();
        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        if (waitForFadeOut)
            yield return Fader.i.FadeOut();
        else
            StartCoroutine(Fader.i.FadeOut());
    }
}
