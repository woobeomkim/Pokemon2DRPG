using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
   [SerializeField] GameObject evolutionUI;
   [SerializeField] Image pokemonImage;

   [SerializeField] AudioClip evolutionMusic;

   public event Action onStartEvolution;
   public event Action onCompleteEvolution;
   public static EvolutionManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        onStartEvolution?.Invoke();
        evolutionUI.gameObject.SetActive(true);

        AudioManager.i.PlayMusic(evolutionMusic);
        pokemonImage.sprite = pokemon.Base.FrontSprite;

        yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name}(이)가 진화하고 있는중이다..");

        var oldPokemon = pokemon.Base;
        pokemon.evolve(evolution);
        pokemonImage.sprite = pokemon.Base.FrontSprite;

        yield return DialogManager.i.ShowDialogText($"축하합니다! {oldPokemon.Name}(이)가 {pokemon.Base.Name}으로 진화했습니다!");

        evolutionUI.gameObject.SetActive(false);
        onCompleteEvolution?.Invoke();
    }
}
