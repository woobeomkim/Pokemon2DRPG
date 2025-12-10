using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;


public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] Camera worldCamera;
    [SerializeField] BattleSystem bs;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    public StateMachine<GameController> StateMachine { get; private set; }

    public static GameController i { get; private set; }

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    public PlayerController Player => player;
    public Camera WorldCamera => worldCamera;
    public PartyScreen PartyScreen => partyScreen;
    private void Awake()
    {
        i = this;

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        StateMachine = new StateMachine<GameController>(this);
        StateMachine.ChangeState(FreeRoamState.i);

        bs.onBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.i.OnShowDialog += () =>
        {
            StateMachine.Push(DialogueState.i);
        };

        DialogManager.i.OnDialogFinished += () =>
        {
            StateMachine.Pop();
        };
    }

    public void PausedGame(bool pause)
    {
        if(pause)
        {
            StateMachine.Push(PauseState.i);           
        }
        else
        {
            StateMachine.Pop();
        }
    }

    public void StartBattle(BattleTrigger trigger)
    {
        BattleState.i.Trigger = trigger;
        StateMachine.Push(BattleState.i);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        BattleState.i.Trainer = trainer;
        StateMachine.Push(BattleState.i);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
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
        StateMachine.Execute();
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
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

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.red;

        if (bs.StateMachine != null)
        {
            foreach (var state in bs.StateMachine.StateStack)
            {
                GUILayout.Label(state.GetType().ToString(), style);
            }
        }
        foreach (var state in StateMachine.StateStack)
        {
            GUILayout.Label(state.GetType().ToString(), style);
        }
        GUILayout.Label("STATE STACK",style);
    }
}
