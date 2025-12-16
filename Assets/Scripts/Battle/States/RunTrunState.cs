using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;
using static UnityEditor.Progress;

public class RunTrunState : State<BattleSystem>
{
    public static RunTrunState i { get; private set; }

    // Input
    public List<BattleAction> Actions { get; set; }

    private void Awake()
    {
        i = this;
    }

    BattleSystem bs;

    BattleDialog dialogBox;
    PartyScreen partyScreen;
    bool isTrainerBattle;
    PokemonParty playerParty;
    PokemonParty trainerParty;

    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        dialogBox = bs.DialogBox;
        partyScreen = bs.PartyScreen;
        isTrainerBattle = bs.IsTrainerBattle;
        playerParty = bs.PlayerParty;
        trainerParty = bs.TrainerParty;

        StartCoroutine(RunTurns());
    }

    IEnumerator RunTurns()
    {
        foreach(var action in Actions)
        {
            if(action.Type == BattleActionType.Move)
            {
                action.User.Pokemon.CurrentMove = action.SelectedMove;

                yield return RunMove(action.User, action.Target, action.SelectedMove);
                yield return RunAfterTurn(action.User);
            }
            else if (action.Type == BattleActionType.SwitchPokemon)
            {
                yield return bs.SwitchPokemon(action.SelectedPokemon, action.User);
            }
            else if(action.Type == BattleActionType.UseItem)
            {
                if (action.SelectedItem is PokeballItem)
                {
                    yield return bs.ThrowPokeball(action.SelectedItem as PokeballItem);
                }
                else
                {
                    // State Machine에서 자동처리
                }
            }
            else if(action.Type == BattleActionType.Run)
            {
                yield return TryToEscape();
            }
            if (bs.IsBattleOver) break;
        }

        bs.ClearTurnData();

        if (!bs.IsBattleOver)
            bs.StateMachine.ChangeState(ActionSelectionState.i);
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}(이)가 {move.Base.Name}(을)를 사용하였다!");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);
            yield return new WaitForSeconds(1.0f);


            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioID.Hit);

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.WaitForHPUpdate();
                yield return ShowDamageDetails(damageDetails);

            }
            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}의 공격이 빗나갔다!");
        }

    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {
        // Stat Boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts);
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (bs.IsBattleOver) yield break;

        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.WaitForHPUpdate();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
        }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits) return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        float[] boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name}(이)가 기절했다!");
        faintedUnit.PlayFaintAniamtion();
        yield return new WaitForSeconds(2.0f);

        if (!faintedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if (isTrainerBattle)
                battleWon = trainerParty.GetHealthPokemon() == null;

            if (battleWon)
                AudioManager.i.PlayMusic(bs.BattleVictoryMusic);

            // Exp Gain
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;
            float trainerBounus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBounus) / 7);
            expGain = expGain / bs.UnitCount;

            for (int i = 0; i < bs.UnitCount; i++) 
            {
                var playerUnit = bs.PlayerUnits[i];

                playerUnit.Pokemon.Exp += expGain;
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {expGain}EXP를 얻었다!");
                yield return playerUnit.Hud.SetExpSmooth();
                // Checek Level Up

                while (playerUnit.Pokemon.CheckForLevelUp())
                {
                    playerUnit.Hud.SetLevel();
                    yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {playerUnit.Pokemon.Level}Level이 되었다!");

                    //Try to learn a new move
                    var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();
                    if (newMove != null)
                    {
                        if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                        {
                            playerUnit.Pokemon.LearnMove(newMove.Base);
                            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {newMove.Base.Name}을 배웠다!");
                            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
                        }
                        else
                        {
                            // TODO : Forgot Move
                            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {newMove.Base.Name}을 배우려고한다");
                            yield return dialogBox.TypeDialog($"그러나, 기술을 {PokemonBase.MaxNumOfMoves}개만큼 배우지 못한다.");
                            yield return dialogBox.TypeDialog($"잊을 기술을 선택하세요!");

                            MoveToForgetState.i.NewMove = newMove.Base;
                            MoveToForgetState.i.CurrentMoves = playerUnit.Pokemon.Moves.Select(m => m.Base).ToList();
                            yield return GameController.i.StateMachine.PushAndWait(MoveToForgetState.i);

                            int moveIndex = MoveToForgetState.i.Selection;
                            if (moveIndex == PokemonBase.MaxNumOfMoves || moveIndex == -1)
                            {
                                // Dont' learn move
                                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {newMove.Base.Name}을 배우지 않았다!");
                            }
                            else
                            {
                                // forget selecetedmove and learn new move
                                var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name}(이)가 {selectedMove.Name}을 잊고 {newMove.Base.Name}을 배웠다!");

                                playerUnit.Pokemon.Moves[moveIndex] = new Move(newMove.Base);

                            }
                        }
                    }

                    yield return playerUnit.Hud.SetExpSmooth(true);
                }
            }
        

            yield return new WaitForSeconds(1f);
        }
        yield return CheckForBattleOver(faintedUnit);
    }

    IEnumerator CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthPokemon();
            if (nextPokemon != null)
            {
                yield return GameController.i.StateMachine.PushAndWait(PartyState.i);
                yield return bs.SwitchPokemon(PartyState.i.SelectedPokemon, faintedUnit);
            }
            else
            {
               bs.BattleOver(false);
            }
        }
        else
        {
            if (isTrainerBattle)
            {
                var nextPokemon = trainerParty.GetHealthPokemon();
                if (nextPokemon != null)
                {
                    AboutToUseToState.i.NewPokemon = nextPokemon;
                    yield return bs.StateMachine.PushAndWait(AboutToUseToState.i);
                }
                else
                {
                    bs.BattleOver(true);
                }
            }
            else
                bs.BattleOver(true);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog($"급소를 때린것같다!");

        if (damageDetails.TypeEffective > 1)
            yield return dialogBox.TypeDialog($"매우 효과적이다!");
        else if (damageDetails.TypeEffective < 1)
            yield return dialogBox.TypeDialog($"효과가 없는것 같다!");
    }

    IEnumerator TryToEscape()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"트레이너 배틀에선 도망갈 수 없어!");
            yield break;
        }

        ++bs.EscapeAttempts;

        int playerSpeed = bs.PlayerUnits[0].Pokemon.Speed;
        int enemySpeed = bs.EnemyUnits[0].Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog("안전하게 도망쳤다!");
            bs.BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * bs.EscapeAttempts;
            f = f % 250;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog("안전하게 도망쳤다!");
                bs.BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog("도망칠수앖다!");
            }
        }
    }
}
