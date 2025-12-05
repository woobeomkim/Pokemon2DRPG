using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurfableWater : MonoBehaviour,Interactable,IPlayerTriggerable
{
    bool isJumpingToWater = false;

    public bool TriggerRepeatedly => true;

    public IEnumerator Interact(Transform initiator)
    {
        var animator = initiator.GetComponent<CharacterAnimator>();
        if (animator.IsSurfing || isJumpingToWater)
            yield break;

        yield return DialogManager.i.ShowDialogText("이 물은 깊어 보인다..");

        var pokemonWithSurf = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "파도타기"));

        if (pokemonWithSurf != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.i.ShowDialogText($"{pokemonWithSurf.Base.Name}의 파도타기를 사용하시겠습니까?",
                choices: new List<string>() { "네", "아니오" },
                onChoiceSelected: (selection) => { selectedChoice = selection; });

            if (selectedChoice == 0)
            {
                // CutTree
                yield return DialogManager.i.ShowDialogText($"{pokemonWithSurf.Base.Name}(이)가 파도타기를 사용하였다!");

                var dir = new Vector3(animator.MoveX, animator.MoveY, 0);
                var targetPos = initiator.transform.position + dir;

                animator.IsJumping = true;
                isJumpingToWater = true;
                yield return initiator.DOJump(targetPos, 0.3f, 1, 0.5f).WaitForCompletion();
                isJumpingToWater = false;
                animator.IsJumping = false;
                
                animator.IsSurfing = true;
            }
        }
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            GameController.i.StartBattle(BattleTrigger.Water);
        }
    }
}
