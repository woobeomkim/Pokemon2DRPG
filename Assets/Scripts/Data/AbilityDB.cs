using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDB
{
    public static Dictionary<AbilityID, Ability> Ablities = new Dictionary<AbilityID, Ability>()
    {
        // 1. Abilities that increase stats

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
            // 복안
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

            // 2. Abilites that prevent stats reduction
        {
            AbilityID.keeneye,
            new Ability()
            {
                Name = "Keen Eye",
                Description = "다른 포켓몬으로부터 명중률이 낮아지는걸 막아준다.",
                OnBoost = (Dictionary<Stat,int> boosts, Pokemon source, Pokemon target) =>
                {
                    // if self boost, then return
                    if (source != null && source == target) return;

                    if(boosts.ContainsKey(Stat.Accuracy) && boosts[Stat.Accuracy] < 0)
                    {
                        boosts.Remove(Stat.Accuracy);
                        target.AddStatusEvent(StatusEventType.Text,$"{target.Base.Name}의 명중률이 예리한 눈때문에 낮아질수없다.");
                    }
                },
            }
        },
        {
            AbilityID.hypercutter,
            new Ability()
            {
                Name = "Hyper Cutter",
                Description = "다른 포켓몬으로부터 공격력이 낮아지는걸 막아준다.",
                OnBoost = (Dictionary<Stat,int> boosts, Pokemon source, Pokemon target) =>
                {
                    // if self boost, then return
                    if (source != null && source == target) return;

                    if(boosts.ContainsKey(Stat.Attack) && boosts[Stat.Attack] < 0)
                    {
                        boosts.Remove(Stat.Attack);
                        target.AddStatusEvent(StatusEventType.Text,$"{target.Base.Name}의 공격력이 낮아질수없다.");
                    }
                },
            }
        },
        {
            AbilityID.bigpecks,
            new Ability()
            {
                Name = "Big Pecks",
                Description = "다른 포켓몬으로부터 방어력이 낮아지는걸 막아준다.",
                OnBoost = (Dictionary<Stat,int> boosts, Pokemon source, Pokemon target) =>
                {
                    // if self boost, then return
                    if (source != null && source == target) return;

                    if(boosts.ContainsKey(Stat.Defense) && boosts[Stat.Defense] < 0)
                    {
                        boosts.Remove(Stat.Defense);
                        target.AddStatusEvent(StatusEventType.Text,$"{target.Base.Name}의 방어력이 낮아질수없다.");
                    }
                },
            }
        },
        {
            AbilityID.clearbody,
            new Ability()
            {
                Name = "Clear Body",
                Description = "다른 포켓몬으로부터 스탯이 낮아지는걸 막아준다.",
                OnBoost = (Dictionary<Stat,int> boosts, Pokemon source, Pokemon target) =>
                {
                    // if self boost, then return
                    if (source != null && source == target) return;

                    bool boostRemoved = false;
                    foreach (var stat in boosts.Keys.ToList())
                    {
                        if(boosts[stat] < 0)
                        {
                            boosts.Remove(stat);
                            boostRemoved = true;
                        }
                    }

                    if(boostRemoved)
                         target.AddStatusEvent(StatusEventType.Text,$"{target.Base.Name}의 클리어바디가 스탯이 낮아지는걸 막아주었다.");
                },
            }
        },
        {
            AbilityID.whitesmoke,
            new Ability()
            {
                Name = "White Smoke",
                Description = "다른 포켓몬으로부터 스탯이 낮아지는걸 막아준다.",
                OnBoost = (Dictionary<Stat,int> boosts, Pokemon source, Pokemon target) =>
                {
                    // if self boost, then return
                    if (source != null && source == target) return;

                    bool boostRemoved = false;
                    foreach (var stat in boosts.Keys)
                    {
                        if(boosts[stat] < 0)
                        {
                            boosts.Remove(stat);
                            boostRemoved = true;
                        }
                    }

                    if(boostRemoved)
                         target.AddStatusEvent(StatusEventType.Text,$"{target.Base.Name}의 하얀연기가 스탯이 낮아지는걸 막아주었다.");
                },
            }
        },

        // 3. Abilites that prevent status conditions
        {
            AbilityID.insomnia,
            new Ability()
            {
                Name = "Insomina",
                Description = "포켓몬이 잠드는것을 막아준다",
                OnTrySetStatus = (StatusConditionID statusID, Pokemon pokemon, EffectSource effectSource) =>
                {
                    if(statusID == StatusConditionID.slp)
                    {
                        if(effectSource == EffectSource.Move)
                            pokemon.AddStatusEvent(StatusEventType.Text, $"{pokemon.Base.Name}의 불면때문에 잠에들수없다!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            AbilityID.immunity,
            new Ability()
            {
                Name = "Immunity",
                Description = "포켓몬이 독에 감여되는것을 막아준다",
                OnTrySetStatus = (StatusConditionID statusID, Pokemon pokemon, EffectSource effectSource) =>
                {
                    if(statusID == StatusConditionID.psn)
                    {
                        if(effectSource == EffectSource.Move)
                            pokemon.AddStatusEvent(StatusEventType.Text, $"{pokemon.Base.Name}의 면역때문에 독에 감염될수없다!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            AbilityID.limber,
            new Ability()
            {
                Name = "Limber",
                Description = "포켓몬이 마비되는것을 막아준다",
                OnTrySetStatus = (StatusConditionID statusID, Pokemon pokemon, EffectSource effectSource) =>
                {
                    if(statusID == StatusConditionID.par)
                    {
                        if(effectSource == EffectSource.Move)
                            pokemon.AddStatusEvent(StatusEventType.Text, $"{pokemon.Base.Name}의 유연때문에 마비될수없다!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            AbilityID.waterveil,
            new Ability()
            {
                Name = "Water Veil",
                Description = "포켓몬이 화상을 입는것을 막아준다",
                OnTrySetStatus = (StatusConditionID statusID, Pokemon pokemon, EffectSource effectSource) =>
                {
                    if(statusID == StatusConditionID.brn)
                    {
                        if(effectSource == EffectSource.Move)
                            pokemon.AddStatusEvent(StatusEventType.Text, $"{pokemon.Base.Name}의 물막때문에 화상을 입을수 없다!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            AbilityID.vitalspirit,
            new Ability()
            {
                Name = "Vital Spirit",
                Description = "포켓몬이 잠드는것을 막아준다",
                OnTrySetStatus = (StatusConditionID statusID, Pokemon pokemon, EffectSource effectSource) =>
                {
                    if(statusID == StatusConditionID.slp)
                    {
                        if(effectSource == EffectSource.Move)
                            pokemon.AddStatusEvent(StatusEventType.Text, $"{pokemon.Base.Name}의 생명력때문에 잠에들수없다!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            AbilityID.owntempo,
            new Ability()
            {
                Name = "Own Tempo",
                Description = "포켓몬이 혼란에드는것을 막아준다",
                OnTrySetVolatileStatus = (StatusConditionID statusID, Pokemon pokemon, EffectSource effectSource) =>
                {
                    if(statusID == StatusConditionID.confused)
                    {
                        if(effectSource == EffectSource.Move)
                            pokemon.AddStatusEvent(StatusEventType.Text, $"{pokemon.Base.Name}의 마이페이스때문에 혼란에빠질수없다!");
                        return false;
                    }

                    return true;
                }
            }
        },

        // 4. Abilites that inflict status conditions on contact move

        {
            AbilityID.Static,
            new Ability()
            {
                Name = "Static",
                Description = "접촉했을때 포켓몬을 마비상태로 만든다",
                OnDamagingHit = (float damage, Pokemon attacker, Pokemon defender,Move move) =>
                {
                    if(move.Base.HasFlag(MoveFlag.Contact) && Random.Range(1,101)<=30)
                    {
                        attacker.SetStatus(StatusConditionID.par,EffectSource.Ability);
                    }
                }
            }
        },
        {
            AbilityID.posionpoint,
            new Ability()
            {
                Name = "Poision Point",
                Description = "접촉했을때 포켓몬을 독감염상태로 만든다",
                OnDamagingHit = (float damage, Pokemon attacker, Pokemon defender,Move move) =>
                {
                    if(move.Base.HasFlag(MoveFlag.Contact) && Random.Range(1,101)<=30)
                    {
                        attacker.SetStatus(StatusConditionID.psn,EffectSource.Ability);
                    }
                }
            }
        },
        {
            AbilityID.flamebody,
            new Ability()
            {
                Name = "Flame Body",
                Description = "접촉했을때 포켓몬을 화상상태로 만든다",
                OnDamagingHit = (float damage, Pokemon attacker, Pokemon defender,Move move) =>
                {
                    if(move.Base.HasFlag(MoveFlag.Contact) && Random.Range(1,101)<=30)
                    {
                        attacker.SetStatus(StatusConditionID.brn,EffectSource.Ability);
                    }
                }
            }
        },
    };
};


public enum AbilityID
{
    none, blaze , overgrow,torrent,swarm,guts,marvelscale,quickfeet,compoundeyes,
    keeneye, hypercutter, bigpecks,clearbody,whitesmoke,
    insomnia, immunity,limber,waterveil,vitalspirit,owntempo,
    Static,posionpoint,flamebody
}