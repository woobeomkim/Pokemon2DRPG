using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherCondtionsDB 
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionID = kvp.Key;
            var condition = kvp.Value;

            condition.ID = conditionID;
        }
    }

    public static Dictionary<WeatherConditonID, WeatherCondition> Conditions = new Dictionary<WeatherConditonID, WeatherCondition>()
    {
        {
            WeatherConditonID.sandstorm,
            new WeatherCondition()
            {
                Name = "Sandstorm",
                StartMessage = "¸ð·¡ÆøÇ³ÀÌ ºÒ±â ½ÃÀÛÇß´Ù!",
                EffeectMessage = "¸ð·¡ÆøÇ³ÀÌ ¼¼Â÷°Ô ºÐ´Ù!",
                EndMessage = "¸ð·¡ÆøÇ³ÀÌ ¸ØÃè´Ù!",
                StartByMoveMessage = "¸ð·¡ÆøÇ³ÀÌ ºÒ¾î¿Â´Ù...",
                OnWeatherEffect = (Pokemon pokemon) =>
                {
                    if(pokemon.IsOfType(PokemonType.Ground) || pokemon.IsOfType(PokemonType.Rock)) return;

                    pokemon.DecreaseHP(Mathf.CeilToInt(pokemon.MaxHP /16f));
                    pokemon.AddStatusEvent(StatusEventType.Damage,$"¸ð·¡ÆøÇ³ÀÌ {pokemon.Base.Name}¸¦ µ¤ÃÆ´Ù.");
                },
            }
        },
         {
            WeatherConditonID.hail,
            new WeatherCondition()
            {
                Name = "Hail",
                StartMessage = "¿ì¹ÚÀÌ ³»¸®°íÀÖ´Ù!",
                EffeectMessage = "¿ì¹ÚÀÌ °è¼ÓÇØ¼­ ³»¸°´Ù!",
                EndMessage = "¿ì¹ÚÀÌ ¸ØÃè´Ù!",
                StartByMoveMessage = "¿ì¹ÚÀÌ ³»¸®±â ½ÃÀÛÇÑ´Ù",
                OnWeatherEffect = (Pokemon pokemon) =>
                {
                    if(pokemon.IsOfType(PokemonType.Ice)) return;

                    pokemon.DecreaseHP(Mathf.CeilToInt(pokemon.MaxHP /16f));
                    pokemon.AddStatusEvent(StatusEventType.Damage,$"¿ì¹ÚÀÌ {pokemon.Base.Name}¸¦ µ¤ÃÆ´Ù.");
                },
            }
        },

          {
            WeatherConditonID.rain,
            new WeatherCondition()
            {
                Name = "Rain",
                StartMessage = "ºñ°¡ ³»¸®°íÀÖ´Ù!",
                EffeectMessage = "ºñ°¡ °è¼ÓÇØ¼­ ³»¸°´Ù!",
                EndMessage = "ºñ°¡ ¸ØÃè´Ù!",
                StartByMoveMessage = "ºñ°¡ ³»¸®±â ½ÃÀÛÇÑ´Ù...",
                OnDamageModify = (Move move) =>
                {
                    if(move.Base.Type == PokemonType.Water)
                        return 1.5f;
                    else if (move.Base.Type == PokemonType.Fire)
                        return 0.5f;

                    return 1f;
                }
            }
        },
          {
            WeatherConditonID.harshsunlight,
            new WeatherCondition()
            {
                Name = "Harsh Sunlight",
                StartMessage = "ÇØ°¡ Â¸Â¸ÇÏ´Ù!",
                EffeectMessage = "ÇØ°¡ Â¸Â¸ÇÏ´Ù!",
                EndMessage = "ÇØ°¡ Á®¹ö·È´Ù!",
                StartByMoveMessage = "ÇØ°¡ Â¸Â¸ÇÏ´Ù!",
                OnDamageModify = (Move move) =>
                {
                    if(move.Base.Type == PokemonType.Fire)
                        return 1.5f;
                    else if (move.Base.Type == PokemonType.Water)
                        return 0.5f;

                    return 1f;
                }
            }
        },
    };
}

public class WeatherCondition
{
    public WeatherConditonID ID { get; set; }
    public string Name { get; set; }
    public string StartMessage { get; set; }
    public string EffeectMessage { get; set; }
    public string EndMessage { get; set; }
    public string StartByMoveMessage { get; set; }

    public Action<Pokemon> OnWeatherEffect { get; set; }
    public Func<Move, float> OnDamageModify { get; set; }
}

public enum WeatherConditonID
{
    none, sandstorm,hail,rain,harshsunlight
}
