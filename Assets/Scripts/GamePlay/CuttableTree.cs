using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CuttableTree : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.i.ShowDialogText("이 나무는 자를수 있어 보인다..");

        var pokemonWithCut = initiator.GetComponent<PokemonParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "자르기"));
    
        if (pokemonWithCut != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.i.ShowDialogText($"{pokemonWithCut.Base.Name}의 자르기를 사용하시겠습니까?",
                choices: new List<string>() { "네", "아니오" },
                onChoiceSelected: (selection) => { selectedChoice = selection; });

            if(selectedChoice == 0)
            {
                // CutTree
                yield return DialogManager.i.ShowDialogText($"{pokemonWithCut.Base.Name}(이)가 자르기를 사용하였다!");
                gameObject.SetActive(false);
            }
        }
    }

}
