using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.GenericSelectionUI;

public class PokemonStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] List<ImageSlot> boxSlots;

    List<BoxPartySlotUI> partySlots = new List<BoxPartySlotUI>();
    List<BoxStorageSlotUI> storageSlots = new List<BoxStorageSlotUI>();

    PokemonParty party;
    PokemonStorageBoxes storageBoxes;

    public int SelectedBox { get; private set; } = 0;

    private void Awake()
    {
        foreach (var boxSlot in boxSlots)
        {
            var storageSlot = boxSlot.GetComponent<BoxStorageSlotUI>();
            if (storageSlot != null)
            {
                storageSlots.Add(storageSlot);
            }
            else
            {
                partySlots.Add(boxSlot.GetComponent<BoxPartySlotUI>());
            }
        }

        party = PokemonParty.GetPlayerParty();
        storageBoxes = PokemonStorageBoxes.GetPlayerStorageBoxes();

        // Test code
        storageBoxes.AddPokemon(party.Pokemons[0], SelectedBox, 0);
        storageBoxes.AddPokemon(party.Pokemons[1], SelectedBox, 20);
    }
    private void Start()
    {
        SetItems(boxSlots);
        SetSelectionSettings(SelectionType.Grid, 7);
    }

    public void SetDataInPartySlot()
    {
        for(int i=0;i<partySlots.Count;i++)
        {
            if (i < party.Pokemons.Count)
                partySlots[i].SetData(party.Pokemons[i]);
            else
                partySlots[i].ClearData();
        }
    }

    public void SetDataInStorageSlot()
    {
        for (int i = 0; i < storageSlots.Count; i++) 
        {
            var pokemon = storageBoxes.GetPokemon(SelectedBox, i);
            if (pokemon != null)
                storageSlots[i].SetData(pokemon);
            else
                storageSlots[i].ClearData();
        }
    }
}
