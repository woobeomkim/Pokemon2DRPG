using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action onUpdate;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }

        set
        {
            pokemons = value;
            onUpdate?.Invoke();
        }
    }

    private void Start()
    {
        foreach (var pokemon in pokemons)
            pokemon.Init();
    }

    public Pokemon GetHealthPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Pokemon pokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(pokemon);
            onUpdate?.Invoke();
        }
        else
        {
            // TODO Send PC
        }
    }

    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
