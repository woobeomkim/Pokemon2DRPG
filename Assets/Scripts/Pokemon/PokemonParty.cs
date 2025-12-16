using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdate;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }

        set
        {
            pokemons = value;
            OnUpdate?.Invoke();
        }
    }

    PokemonStorageBoxes storageBoxes;
    private void Awake()
    {
        storageBoxes = PokemonStorageBoxes.GetPlayerStorageBoxes();
        foreach (var pokemon in pokemons)
            pokemon.Init();
    }

    public Pokemon GetHealthPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }
    public List<Pokemon> GetHealthPokemons(int unitCount)
    {
        return pokemons.Where(x => x.HP > 0).Take(unitCount).ToList();
    }

    public void AddPokemon(Pokemon pokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(pokemon);
            OnUpdate?.Invoke();
        }
        else
        {
            // TODO Send PC
            storageBoxes.AddPokemonToEmptySlot(pokemon);
        }
    }

    public bool CheckForEvolution()
    {
        return pokemons.Any(p => p.CheckForEvolution() != null);
    }

    public IEnumerator RunEvolution()
    {
        foreach(var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if( evolution!=null)
            {
                yield return EvolutionState.i.Evolve(pokemon, evolution);
            }
        }
    }

    public void PartyUpdate()
    {
        OnUpdate?.Invoke();
    }
    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
