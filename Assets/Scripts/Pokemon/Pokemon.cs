using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    [SerializeField] PokemonBase pBase;
    [SerializeField] int level;

    Pokemon(PokemonBase pBase,int level)
    {
        this.pBase = pBase;
        this.level = level;
    }

    public int Attack
    {
        get
        {
            return Mathf.FloorToInt((pBase.Attack * level) / 100f) + 5;
        }
    }
    public int Defense
    {
        get
        {
            return Mathf.FloorToInt((pBase.Defense * level) / 100f) + 5;
        }
    }

    public int SpAttack
    {
        get
        {
            return Mathf.FloorToInt((pBase.SpAttack * level) / 100f) + 5;
        }
    }
    public int SpDefense
    {
        get
        {
            return Mathf.FloorToInt((pBase.SpDefense * level) / 100f) + 5;
        }
    }

    public int Speed
    {
        get
        {
            return Mathf.FloorToInt((pBase.Speed * level) / 100f) + 5;
        }
    }

    public int MaxHP
    {
        get
        {
            return Mathf.FloorToInt((pBase.MaxHP * level) / 100f) + 10;
        }
    }

}
