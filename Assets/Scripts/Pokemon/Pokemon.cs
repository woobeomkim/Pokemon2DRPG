using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _pBase;
    [SerializeField] int level;

    public PokemonBase Base { get { return _pBase; } set { _pBase = value; } }
    public int Level { get { return level; } set { level = value; } }

    public int HP { get; set; }
    public bool HPChanged { get; set; }
    public List<Move> Moves { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public Condition VolatileStatus { get; private set; }
    public int StatusTime { get; set; }
    public int VolatileStatusTime { get; set; }

    public event Action OnStatusChanged;
    public void Init()
    {
        Moves = new List<Move>();

        foreach(var move in Base.LearnableMoves)
        {
            if(move.Level <= level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= 4)
                break;
        }

        CalculateStats();

        HP = MaxHP;
        ResetBoost();

        Status = null;
        VolatileStatus = null;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;
    }

    void ResetBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack,0 },
            {Stat.Defense,0 },
            {Stat.SpAttack,0 },
            {Stat.SpDefense,0 },
            {Stat.Speed,0 },
        };
    }

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];

        // Apply Stat Boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 2.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        return statVal;
    }

    public void ApplyBoost(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}의 {stat}이 증가하였다!");
            else
                StatusChanges.Enqueue($"{Base.Name}의 {stat}이 감소하였다!");

            Debug.Log($"{stat} 이 {StatBoosts[stat]}만큼 부스트 되었다");
        }
    }

    public DamageDetails TakeDamage(Move move,Pokemon attacker)
    {
        float critical = 1f;
        if (UnityEngine.Random.value * 100f <= 6.25f)
            critical = 2f;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            Fainted = false,
            Critical = critical,
            TypeEffective = type
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? this.SpDefense : this.Defense;

        float modifiers = UnityEngine.Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HPChanged = true;
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;

        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}(이)가 {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }
    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name}(이)가 {VolatileStatus.StartMessage}");
    }

    public Move GetRandomMove()
    {
        var move = Moves[UnityEngine.Random.Range(0, Moves.Count)];

        return move;
    }

    public void CureStatus()
    {
        Status = null;
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }
    public bool OnBeforeMove()
    {
        bool canPerformMove = true;

        if (Status?.OnBeforeMove != null) 
        {
            if (!Status.OnBeforeMove.Invoke(this))
                canPerformMove = false;
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove.Invoke(this))
                canPerformMove = false;
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        ResetBoost();
        CureVolatileStatus();
    }

    public int Attack
    {
        get
        {
            return GetStat(Stat.Attack);
        }
    }
    public int Defense
    {
        get
        {
            return GetStat(Stat.Defense);
        }
    }

    public int SpAttack
    {
        get
        {
            return GetStat(Stat.SpAttack);
        }
    }
    public int SpDefense
    {
        get
        {
            return GetStat(Stat.SpDefense);
        }
    }

    public int Speed
    {
        get
        {
            return GetStat(Stat.Speed);
        }
    }

    public int MaxHP
    {
        get; private set;
    }

}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffective { get; set; }
}