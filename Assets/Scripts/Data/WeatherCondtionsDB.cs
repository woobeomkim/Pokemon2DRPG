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
                OnWeatherEffect = (Pokemon pokemon) =>
                {
                    if(pokemon.IsOfType(PokemonType.Ground) || pokemon.IsOfType(PokemonType.Rock)) return;

                    pokemon.DecreaseHP(pokemon.MaxHP /16);
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
                OnWeatherEffect = (Pokemon pokemon) =>
                {
                    if(pokemon.IsOfType(PokemonType.Ice)) return;

                    pokemon.DecreaseHP(pokemon.MaxHP /16);
                    pokemon.AddStatusEvent(StatusEventType.Damage,$"¿ì¹ÚÀÌ {pokemon.Base.Name}¸¦ µ¤ÃÆ´Ù.");
                },
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

    public Action<Pokemon> OnWeatherEffect { get; set; }
    public Func<Move, float> OnDamageModify { get; set; }
}

public enum WeatherConditonID
{
    none, sandstorm,hail,rain,harshsunlight
}
