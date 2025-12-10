using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public enum BattleStates { Start, ActionSelection, MoveSelection, RunningTurn, PartyScreen ,Bag,BattleOver,AboutToUse ,MoveToForget ,Busy }

public enum BattleAction { Move,SwitchPokemon,UseItem,Run}

public enum BattleTrigger { Longgrass,Water}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialog dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveToForgetSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("Background Images")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite grassBackground;
    [SerializeField] Sprite waterBackground;

    public StateMachine<BattleSystem> StateMachine { get; private set; }

    public event Action<bool> onBattleOver;

    public int SelectedMove { get; set; }
    public BattleAction SelectedAction { get; set; }
    public Pokemon SelectedPokemon { get; set; }

    public bool IsBattleOver { get; private set; }
    BattleStates state;
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    public PokemonParty PlayerParty { get; private set;}
    public PokemonParty TrainerParty {get; private set;}
    public Pokemon WildPokemon { get; private set; }

    public bool IsTrainerBattle { get; private set; } = false;
    PlayerController player;
    public TrainerController Trainer { get; private set; }

    public int EscapeAttempts { get; set; }
    MoveBase moveToLearn;

    BattleTrigger battleTrigger;

    public BattleDialog DialogBox => dialogBox;
    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;

    public PartyScreen PartyScreen => partyScreen;
    public AudioClip BattleVictoryMusic => battleVictoryMusic;
    
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon,
        BattleTrigger trigger = BattleTrigger.Longgrass)
    {
        this.PlayerParty = playerParty;
        this.WildPokemon = wildPokemon;

        IsTrainerBattle = false;
        player = playerParty.GetComponent<PlayerController>();

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty,
        BattleTrigger trigger = BattleTrigger.Longgrass)
    {
        this.PlayerParty = playerParty;
        this.TrainerParty = trainerParty;

        IsTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        Trainer = trainerParty.GetComponent<TrainerController>();

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(trainerBattleMusic);
        StartCoroutine(SetupBattle());
    }


    IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);

        playerUnit.Clear();
        enemyUnit.Clear();

        backgroundImage.sprite = (battleTrigger == BattleTrigger.Longgrass) ? grassBackground : waterBackground;

        if(!IsTrainerBattle)
        {
            // Wild Pokemon Battle
            playerUnit.Setup(PlayerParty.GetHealthPokemon());
            enemyUnit.Setup(WildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            yield return dialogBox.TypeDialog($"야생의 {enemyUnit.Pokemon.Base.Name}(이)가 나타났다!");
        }
        else
        {
            // Trainer Battle

            // Show trainer and player Image
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = Trainer.Sprite;

            yield return dialogBox.TypeDialog($"{Trainer.Name}(이)가 배틀을 걸어왔다!");

            // send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = TrainerParty.GetHealthPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{Trainer.Name}(이)가 {enemyPokemon.Base.Name}을 내보냈다!");

            // send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = PlayerParty.GetHealthPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"가라! {playerPokemon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        IsBattleOver = false;
        EscapeAttempts = 0;
        partyScreen.Init();

        StateMachine.ChangeState(ActionSelectionState.i);
    }

   

    void ActionSeletion()
    {
        state = BattleStates.ActionSelection;
        dialogBox.EnabledDialogText(true);
        dialogBox.EnabledActionSelector(true);

        dialogBox.SetDialog("행동을 고르세요!");
    }

    void MoveSelection()
    {
        state = BattleStates.MoveSelection;
        dialogBox.EnabledActionSelector(false);
        dialogBox.EnabledDialogText(false);
        dialogBox.EnabledMoveSelector(true);
    }
    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleStates.Busy;
        yield return dialogBox.TypeDialog($"{Trainer.Name}(이)가 {newPokemon.Base.Name}으로 바꾸려고합니다.");
        yield return dialogBox.TypeDialog($"포켓몬을 바꾸시겠습니까?");

        state = BattleStates.AboutToUse;
        dialogBox.EnabledChoiceBox(true);
    }
    
    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleStates.Busy;
        yield return dialogBox.TypeDialog("잊으려는 기술을 고르세요!");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleStates.MoveToForget;
    }

    void OpenBag()
    {
        state = BattleStates.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    void OpenPartyScreen()
    {
        //partyScreen.CalledFrom = state;
        state = BattleStates.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    public void BattleOver(bool won)
    {
        IsBattleOver = true;
        PlayerParty.Pokemons.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        onBattleOver?.Invoke(won);
    }
    public void HandleUpdate()
    {
        StateMachine.Execute();

        if(state == BattleStates.PartyScreen)
        {
            HandlePartySelection();
        }
        else if(state == BattleStates.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleStates.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            //inventoryUI.HandleUpdate(onBack,onItemUsed);
        }
        else if (state == BattleStates.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleStates.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    // Dont' learn move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {moveToLearn.Name}을 배우지 않았다!"));
                }
                else
                {
                    // forget selecetedmove and learn new move
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {selectedMove.Name}을 잊고 {moveToLearn.Name}을 배웠다!"));

                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);

                }
                moveToLearn = null;
                state = BattleStates.RunningTurn;
            };

            //moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
        
    }

    void HandleActionUpdate()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentAction += 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentAction -= 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, dialogBox.ActionTexts.Count - 1);

        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(currentAction == 0)
            {
                MoveSelection();
            }
            else if(currentAction == 1)
            {
                // Bag
                OpenBag();
            }
            else if(currentAction == 2)
            {
                // PartyScreen
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                // Run
                //StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove += 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove -= 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);
    
        if(Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0)
                return;
            // ATTACK
            //Debug.Log($"Attack {playerUnit.Pokemon.Moves[currentMove].Base.Name}");
            dialogBox.EnabledMoveSelector(false);
            dialogBox.EnabledDialogText(true);
            //StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnabledMoveSelector(false);
            dialogBox.EnabledDialogText(true);
            ActionSeletion();
        }
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText($"기절한 포켓몬은 내보낼 수 없습니다!");
                return;
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText($"같은 포켓몬은 내보낼 수 없습니다!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            //if (partyScreen.CalledFrom == BattleState.ActionSelection)
            //{
            //    StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            //}
            //else
            //{
            //    state = BattleState.Busy;
            //    bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
            //    StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            //}
            //partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("계속하려면 포켓몬을 고르세요!");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            //if (partyScreen.CalledFrom == BattleState.AboutToUse)
            //{
            //    StartCoroutine(SendNextTrainerPokemon());
            //}
            //else
            //    ActionSeletion();

            //partyScreen.CalledFrom = null;

        };

        //partyScreen.HandleUpdate(onSelected, onBack);

    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnabledChoiceBox(false);
            if(aboutToUseChoice)
            {
                OpenPartyScreen();
            }
            else
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnabledChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    public IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"돌아와! {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAniamtion();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newPokemon);
        playerUnit.Hud.SetData(newPokemon);

        dialogBox.SetMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"가라! {newPokemon.Base.Name}");
    }

    public IEnumerator SendNextTrainerPokemon()
    {
        state = BattleStates.Busy;

        var nextPokemon = TrainerParty.GetHealthPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{Trainer.Name}(이)가 {nextPokemon.Base.Name}을 내보냈다!");

        state = BattleStates.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleStates.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is PokeballItem)
        {
            yield return ThrowPokeball((PokeballItem)usedItem);
        }

       // StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleStates.Busy;

        if(IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"트레이너의 포켓몬은 잡을수없어!");
            state = BattleStates.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name}(이)가 {pokeballItem.Name}을 사용했다!");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        // Animations
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOLocalMoveY(enemyUnit.transform.position.y - 1.8f, 0.5f).WaitForCompletion();

        int shakeCount = TryCatchPokemon(enemyUnit.Pokemon, pokeballItem);

        for (int i = 0; i < Mathf.Min(shakeCount,3); i++) 
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if(shakeCount == 4)
        {
            yield return dialogBox.TypeDialog($"축하합니다! {enemyUnit.Pokemon.Base.Name}을 잡았습니다!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            PlayerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}가 파티에 추가되었다!");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name}(이)가 몬스터볼에서 튀어나왔다!");
            else
                yield return dialogBox.TypeDialog($"아깝다! 거의 다 잡았는데!");

            Destroy(pokeball);
            state = BattleStates.RunningTurn;
        }
    }

    int TryCatchPokemon(Pokemon pokemon, PokeballItem pokeballItem)
    {
        float a = (3 * pokemon.MaxHP - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBounus(pokemon.Status) * pokeballItem.CatchRateModifier / (3 * pokemon.MaxHP);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while(shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;
            ++shakeCount;
        }

        return shakeCount;
    }
    IEnumerator TryToEscape()
    {
        state = BattleStates.Busy;

        if(IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"트레이너 배틀에선 도망갈 수 없어!");
            state = BattleStates.RunningTurn;
            yield break;
        }

        ++EscapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if(enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog("안전하게 도망쳤다!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * EscapeAttempts;
            f = f % 250;

            if(UnityEngine.Random.Range(0,256) < f)
            {
                yield return dialogBox.TypeDialog("안전하게 도망쳤다!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog("도망칠수앖다!");
                state = BattleStates.RunningTurn;
            }
        }

    }
}
