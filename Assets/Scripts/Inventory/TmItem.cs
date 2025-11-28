using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Pokemon pokemon)
    {
        // Leraning move is handled from InventoryUI, If it was learned the return true
        return pokemon.HasMove(move);
    }

    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnableByItems.Contains(Move);
        
    }

    public override bool IsReusable => isHM;
    public override bool CanUseInBattle => false;
    public bool IsHM => isHM;
    public MoveBase Move => move;
}
