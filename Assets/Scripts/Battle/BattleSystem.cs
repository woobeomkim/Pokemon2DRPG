using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

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
    public ItemBase SelectedItem { get; set; }
    public bool IsBattleOver { get; private set; }

    public PokemonParty PlayerParty { get; private set;}
    public PokemonParty TrainerParty {get; private set;}
    public Pokemon WildPokemon { get; private set; }

    public bool IsTrainerBattle { get; private set; } = false;
    PlayerController player;
    public TrainerController Trainer { get; private set; }

    public int EscapeAttempts { get; set; }

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
        var nextPokemon = TrainerParty.GetHealthPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{Trainer.Name}(이)가 {nextPokemon.Base.Name}을 내보냈다!");
    }

  
    public IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        if(IsTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"트레이너의 포켓몬은 잡을수없어!");
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
}
