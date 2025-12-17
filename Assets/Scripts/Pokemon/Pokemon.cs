using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _pBase;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase,int pLevel)
    {
        _pBase = pBase;
        level = pLevel;

        Init();
    }

    public PokemonBase Base { get { return _pBase; } set { _pBase = value; } }
    public int Level { get { return level; } set { level = value; } }

    
    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }
    public Queue<StatusEvent> StatusChanges { get; private set; } = new Queue<string>();
    public Condition VolatileStatus { get; private set; }
    public int StatusTime { get; set; }
    public int VolatileStatusTime { get; set; }

    public event Action OnStatusChanged;
    public event Action OnHpChagnged;
    public void Init()
    {
        Moves = new List<Move>();
        StatusChanges = new Queue<StatusEvent>();

        foreach(var move in Base.LearnableMoves)
        {
            if(move.Level <= level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();

        HP = MaxHP;
        ResetBoost();

        Status = null;
        VolatileStatus = null;
    }

    public Pokemon(PokemonSaveData saveData)
    {
        Base = PokemonDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusID != null)
            Status = ConditionsDB.Conditions[saveData.statusID.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(m => new Move(m)).ToList();

        CalculateStats();
        StatusChanges = new Queue<StatusEvent>();
        ResetBoost();
        VolatileStatus = null;
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusID = Status?.ID,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        int oldMaxHP = MaxHP;
        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;

        if(oldMaxHP != 0)
            HP += MaxHP - oldMaxHP;
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

            {Stat.Accuracy,0 },
            {Stat.Evasion, 0 }
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
                AddStatusEvent(StatusEventType.StatBoost,$"{Base.Name}의 {stat}이 증가하였다!");
            else
                AddStatusEvent(StatusEventType.StatBoost,$"{Base.Name}의 {stat}이 감소하였다!");

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

        DecreaseHP(damage);

        return damageDetails;
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            return true;
        }
        
        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;

        Moves.Add(new Move(moveToLearn));
    }

    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
    }

    public Evolution CheckForEvolution()
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level);
    }

    public Evolution CheckForEvolution(ItemBase item)
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }

    public void evolve(Evolution evolution)
    {
        Base = evolution.EvolvesInto;
        CalculateStats();
    }

    public void Heal()
    {
        HP = MaxHP;
        OnHpChagnged?.Invoke();
        CureStatus();
    }

    public float GetNormalizedExp()
    {
        int currLevelExp = Base.GetExpForLevel(Level);
        int nextLevelExp = Base.GetExpForLevel(Level + 1);

        float normalizedExp = (float)(Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }
    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHP);
        OnHpChagnged?.Invoke();
    }
    public void DecreaseHP(int damage, bool callUpdateEvent = false)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        
        if(callUpdateEvent)
            OnHpChagnged?.Invoke();
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null) return;

        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        AddStatusEvent($"{Base.Name}(이)가 {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }
    public void SetVolatileStatus(ConditionID conditionID)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        AddStatusEvent($"{Base.Name}(이)가 {VolatileStatus.StartMessage}");
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        var move = movesWithPP[UnityEngine.Random.Range(0, movesWithPP.Count)];

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

    public void AddStatusEvent(StatusEventType type, string message)
    {
        StatusChanges.Enqueue(new StatusEvent(type, message));
    }

    public void AddStatusEvent(string message)
    {
        AddStatusEvent(StatusEventType.Text, message);
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

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusID;
    public List<MoveSaveData> moves;
}

public enum StatusEventType { Text,Damage,StatBoost}

public class StatusEvent
{
    public StatusEventType Type { get; private set; }
    public string Message { get; private set; }
    public StatusEvent(StatusEventType type,string message)
    {
        Type = type;
        Message = message;
    }
}