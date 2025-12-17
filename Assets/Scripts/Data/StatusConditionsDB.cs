using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class StatusConditionsDB
{
    public static void Init()
    {
        foreach(var kvp in Conditions)
        {
            var conditionID = kvp.Key;
            var condition = kvp.Value;

            condition.ID = conditionID;
        }
    }

    public static Dictionary<StatusConditionID, StatusCondition> Conditions { get; set; } = new Dictionary<StatusConditionID, StatusCondition>()
    { 
        {
            StatusConditionID.psn,
            new StatusCondition()
            {
                Name = "Posion",
                StartMessage = "독에 감염되었다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHP / 8);
                    pokemon.AddStatusEvent(StatusEventType.Damage,$"{pokemon.Base.Name}(이)가 독에 감염되어 데미지를 입었다!");
                }
            }
        },
        {
            StatusConditionID.brn,
            new StatusCondition()
            {
                Name = "Burn",
                StartMessage = "화상을 입었다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHP / 16);
                    pokemon.AddStatusEvent(StatusEventType.Damage,$"{pokemon.Base.Name}(이)가 화상을입어 데미지를 입었다!");
                }
            }
        },
        {
            StatusConditionID.par,
            new StatusCondition()
            {
                Name = "Paralyzed",
                StartMessage = "마비되었다!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(UnityEngine.Random.Range(1,5) == 1)
                    {
                        pokemon.AddStatusEvent($"{pokemon.Base.Name}(이)가 마비되어 움직일수없다!");
                        return false;
                    }
                    return true;
                }
            }
        },
         {
            StatusConditionID.frz,
            new StatusCondition()
            {
                Name = "Freeze",
                StartMessage = "얼어붙었다!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(UnityEngine.Random.Range(1,5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.AddStatusEvent($"{pokemon.Base.Name}(이)가 깨어났다");
                        return true;
                    }
                    return false;
                }
            }
        },
          {
            StatusConditionID.slp,
            new StatusCondition()
            {
                Name = "Sleep",
                StartMessage = "잠들었다!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.StatusTime = UnityEngine.Random.Range(1,4);
                    Debug.Log($"{pokemon.StatusTime} == StatusTime");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.AddStatusEvent($"{pokemon.Base.Name}(이)가 깨어났다!");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.AddStatusEvent($"{pokemon.Base.Name}(이)가 자고있는중이다!");

                    return false;
                }
            }
        },
          {
            StatusConditionID.confused,
            new StatusCondition()
            {
                Name = "Confused",
                StartMessage = "혼란에 빠졌다!",
                OnStart = (Pokemon pokemon) =>
                {
                    // 1~4턴동안 혼란에 빠진다
                    pokemon.VolatileStatusTime = UnityEngine.Random.Range(1,5);
                    Debug.Log($"{pokemon.VolatileStatusTime} == VolatileStatusTime");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.AddStatusEvent($"{pokemon.Base.Name}(이)가 혼란에서 깨어났다!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;
                    pokemon.AddStatusEvent($"{pokemon.Base.Name}(이)가 혼란에 빠져있다!");
                    if(UnityEngine.Random.Range(1,3) == 1)
                        return true;
                    pokemon.DecreaseHP(pokemon.MaxHP / 8);
                    pokemon.AddStatusEvent(StatusEventType.Damage,$"{pokemon.Base.Name}(이)가 혼란에빠져 영문도모른체 자신을 공격했다!");
                    return false;
                }
            }

        }
    };

    public static float GetStatusBounus(StatusCondition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.ID == StatusConditionID.slp || condition.ID == StatusConditionID.frz)
            return 2f;
        else if (condition.ID == StatusConditionID.par || condition.ID == StatusConditionID.psn || condition.ID == StatusConditionID.brn)
            return 1.5f;

        return 1f;
    }
}

public enum StatusConditionID
{
    none,psn,brn,slp,par,frz,
    //VolatileStatus
    confused,
}