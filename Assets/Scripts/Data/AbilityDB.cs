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
                OnModifyAttack = (float atk, Pokemon attaker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Fire && attaker.HP <= attaker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAttack = (float atk, Pokemon attaker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Fire && attaker.HP <= attaker.MaxHP / 3)
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
                OnModifyAttack = (float atk, Pokemon attaker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Grass && attaker.HP <= attaker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAttack = (float atk, Pokemon attaker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Grass && attaker.HP <= attaker.MaxHP / 3)
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
                OnModifyAttack = (float atk, Pokemon attaker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Water && attaker.HP <= attaker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAttack = (float atk, Pokemon attaker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Water && attaker.HP <= attaker.MaxHP / 3)
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
                OnModifyAttack = (float atk, Pokemon attaker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Bug && attaker.HP <= attaker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                },
                OnModifySpAttack = (float atk, Pokemon attaker, Pokemon defender, Move move) =>
                {
                    if(move.Base.Type == PokemonType.Bug && attaker.HP <= attaker.MaxHP / 3)
                    {
                        atk = atk * 1.5f;
                    }

                    return atk;
                }
            }
        },
    };
}

public enum AbilityID
{
    none, blaze , overgrow,torrent,swarm
}