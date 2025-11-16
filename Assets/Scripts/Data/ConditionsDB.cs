using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class ConditionsDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    { 
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Posion",
                StartMessage = "독에 감염되었다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHP / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}(이)가 독에 감염되어 데미지를 입었다!");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "화상을 입었다!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHP / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}(이)가 화상을입어 데미지를 입었다!");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "마비되었다!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(UnityEngine.Random.Range(1,5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}(이)가 마비되어 움직일수없다!");
                        return false;
                    }
                    return true;
                }
            }
        },
         {
            ConditionID.frz,
            new Condition()
            {
                Name = "Freeze",
                StartMessage = "얼어붙었다!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(UnityEngine.Random.Range(1,5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}(이)가 깨어났다");
                        return true;
                    }
                    return false;
                }
            }
        },
          {
            ConditionID.slp,
            new Condition()
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
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}(이)가 깨어났다!");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}(이)가 자고있는중이다!");

                    return false;
                }
            }
        }
    };
}

public enum ConditionID
{
    none,psn,brn,slp,par,frz
}