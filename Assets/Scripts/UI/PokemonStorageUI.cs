using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.GenericSelectionUI;

public class PokemonStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] List<ImageSlot> boxSlots;

    private void Start()
    {
        SetItems(boxSlots);
        SetSelectionSettings(SelectionType.Grid, 7);
    }
}
