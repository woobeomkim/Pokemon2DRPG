using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog)
    {
        int selectedChoice = 0;
        yield return DialogManager.i.ShowDialog(dialog, new List<string> { "Yes", "No" },
          (choiceIndex) => { selectedChoice = choiceIndex; });

        if (selectedChoice == 0)
        {
            yield return Fader.i.FadeIn(0.5f);
            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.PartyUpdate();
            yield return Fader.i.FadeOut(0.5f);
            yield return DialogManager.i.ShowDialogText($"잘쉬어서 건강해보이네~ ");
        }
        else if (selectedChoice == 1)
        {
            yield return DialogManager.i.ShowDialogText($"언제든 쉬고싶으면 말걸어줘~");
        }
    }
}