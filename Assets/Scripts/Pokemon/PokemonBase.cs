using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "New Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    // stat
    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    // Type
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name => name;
    public string Description => description;

    public Sprite FrontSprite => frontSprite;
    public Sprite BackSprite => backSprite;

    public int MaxHP => maxHP;
    public int Attack => attack;
    public int Defense => defense;

    public int SpAttack => spAttack;
    public int SpDefense => spDefense;

    public int Speed => speed;
    public PokemonType Type1 => type1;
    public PokemonType Type2 => type2;

    public List<LearnableMove> LearnableMoves => learnableMoves;

}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase pBase;
    [SerializeField] int level;

    public MoveBase Base => pBase;
    public int Level => level;
}

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}
