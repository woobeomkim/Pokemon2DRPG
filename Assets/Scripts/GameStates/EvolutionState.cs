using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class EvolutionState : State<GameController>
{
   [SerializeField] GameObject evolutionUI;
   [SerializeField] Image pokemonImage;

   [SerializeField] AudioClip evolutionMusic;

   public static EvolutionState i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        var gc = GameController.i;
        gc.StateMachine.Push(this);
        evolutionUI.gameObject.SetActive(true);

        AudioManager.i.PlayMusic(evolutionMusic);
        pokemonImage.sprite = pokemon.Base.FrontSprite;

        yield return DialogManager.i.ShowDialogText($"{pokemon.Base.Name}(이)가 진화하고 있는중이다..");

        var oldPokemon = pokemon.Base;
        pokemon.evolve(evolution);
        pokemonImage.sprite = pokemon.Base.FrontSprite;

        yield return DialogManager.i.ShowDialogText($"축하합니다! {oldPokemon.Name}(이)가 {pokemon.Base.Name}으로 진화했습니다!");

        evolutionUI.gameObject.SetActive(false);

        gc.PartyScreen.SetPartyData();

        AudioManager.i.PlayMusic(gc.CurrentScene.SceneMusic, fade: true);
        gc.StateMachine.Pop();
    }
}
