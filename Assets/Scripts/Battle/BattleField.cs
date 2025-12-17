using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleField
{
    public WeatherCondition Weather { get; private set; }
    public int? WeatherDuration { get; set; }
    public void SetWeather(WeatherConditonID weatherID, int? weatherDuration = null)
    {
        if (weatherID == WeatherConditonID.none)
            Weather = null;
        else
            Weather = WeatherCondtionsDB.Conditions[weatherID];

        WeatherDuration = weatherDuration;
    }
}
