using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonStorageBoxes : MonoBehaviour
{
    const int numberOfBoxes = 16;
    const int numberOfSlots = 30;

    public int NumOfBoxes => numberOfBoxes;
    public int NumOfSlots => numberOfSlots;

    Pokemon[,] boxes = new Pokemon[numberOfBoxes, numberOfSlots];

    public void AddPokemon(Pokemon pokemon, int boxIndex, int slotIndex)
    {
        boxes[boxIndex, slotIndex] = pokemon;
    }

    public void RemovePokemon(int boxIndex, int slotIndex)
    {
        boxes[boxIndex, slotIndex] = null;
    }

    public Pokemon GetPokemon(int boxIndex, int slotIndex)
    {
        return boxes[boxIndex, slotIndex];
    }

    public void AddPokemonToEmptySlot(Pokemon pokemon)
    {
        for(int boxIndex = 0; boxIndex < numberOfBoxes; boxIndex++)
        {
            for (int slotIndex = 0; slotIndex < numberOfSlots; slotIndex++)
            {
                if (boxes[boxIndex, slotIndex] == null)
                {
                    boxes[boxIndex, slotIndex] = pokemon;
                    return;
                }
            }
        }
    }

    public static PokemonStorageBoxes GetPlayerStorageBoxes()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonStorageBoxes>();
    }
}
