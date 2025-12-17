using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Move", menuName = "Pokemon/Create new Move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;

    [SerializeField] bool isMultiHitMove = false;
    [SerializeField] Vector2Int hitRange = new Vector2Int(2, 0);

    [SerializeField] AudioClip sound;

    public int GetHitTimes()
    {
        if (isMultiHitMove)
        {
            if (hitRange.y == 0)
                return hitRange.x;

            return UnityEngine.Random.Range(hitRange.x, hitRange.y + 1);
        }
        else
            return 1;
    }

    public string Name => name;

    public string Description => description;

    public PokemonType Type => type;

    public int Power => power;

    public int Accuracy => accuracy;
    public bool AlwaysHits => alwaysHits;
    public int PP => pp;
    public int Priority => priority;
    public MoveCategory Category => category;

    public MoveEffects Effects => effects;
    public List<SecondaryEffects> Secondaries => secondaries;
    public MoveTarget Target => target;
    public AudioClip Sound => sound;

    public bool IsMultiHitMove => isMultiHitMove;
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] StatusConditionID status;
    [SerializeField] StatusConditionID volatileStatus;
    [SerializeField] WeatherConditonID weather;
    public List<StatBoost> Boosts => boosts;
    public StatusConditionID Status => status;
    public StatusConditionID VolatileStatus => volatileStatus;
    public WeatherConditonID Weather => weather;
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance => chance;
    public MoveTarget Target => target;
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveCategory
{
    Physical,Special,Status
}

public enum MoveTarget
{
    Foe,Self
}