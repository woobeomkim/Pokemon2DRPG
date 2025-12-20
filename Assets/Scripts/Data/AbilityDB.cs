using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDB
{
    public static Dictionary<AbilityID, Ability> Ablities = new Dictionary<AbilityID, Ability>()
    {
        {
            AbilityID.blaze,
            new Ability()
            {
                Name = "Blaze",
                Description = "HP가 낮아지면 불꽃타입의 기술이 강해진다.",
                OnModifyAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Fire && attacker.HP <= attacker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Fire && attacker.HP <= attacker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.overgrow,
            new Ability()
            {
                Name = "Overgrow",
                Description = "HP가 낮아지면 풀타입의 기술이 강해진다.",
                OnModifyAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Grass && attacker.HP <= attacker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Grass && attacker.HP <= attacker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.torrent,
            new Ability()
            {
                Name = "Torrent",
                Description = "HP가 낮아지면 물타입의 기술이 강해진다.",
                OnModifyAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Water && attacker.HP <= attacker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Water && attacker.HP <= attacker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },
        {
            AbilityID.swarm,
            new Ability()
            {
                Name = "Swarm",
                Description = "HP가 낮아지면 벌레타입의 기술이 강해진다.",
                OnModifyAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Bug && attacker.HP <= attacker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Bug && attacker.HP <= attacker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },

         {
            AbilityID.guts,
            new Ability()
            {
                Name = "Guts",
                Description = "상태이상에 걸리면 공격이 강해진다.",
                OnModifyAttack = (float atk, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(attacker.Status != null)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
            }
        },
           {
            AbilityID.marvelscale,
            new Ability()
            {
                Name = "Marvel Scale",
                Description = "상태이상에 걸리면 방어력이 강해진다.",
                OnModifyDefense = (float def, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(defender.Status != null)
                    {
                        def = def * 1.5f;
                    }

                    return def;
                },
            }
        },
  {
            AbilityID.quickfeet,
            new Ability()
            {
                Name = "Quick feet",
                Description = "상태이상에 걸리면 스피드가 빨라진다.",
                OnModifySpeed = (float speed, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    if(attacker.Status != null)
                    {
                        speed = speed * 1.5f;
                    }

                    return speed;
                },
            }
        },
   {
            AbilityID.compoundeyes,
            new Ability()
            {
                Name = "Compound Eyes",
                Description = "포켓몬의 복안이 명중률을 향상시킨다.",
                OnModifySpeed = (float acc, Pokemon attacker, Pokemon defender, Move move) =>
                {
                    return acc * 1.3f;
                },
            }
        },
    };
};


public enum AbilityID
{
    none, blaze , overgrow,torrent,swarm,guts,marvelscale,quickfeet,compoundeyes
}