using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;

    public MoveBase Move => move;

    public override bool Use(Pokemon pokemon)
    {
        // Leraning move is handled from InventoryUI, If it was learned the return true
        return pokemon.HasMove(move);
    }

}
