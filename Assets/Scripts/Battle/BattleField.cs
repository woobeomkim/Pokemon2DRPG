using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleField
{
   public WeatherCondition Weather { get; private set; }

    public void SetWeather(WeatherConditonID weatherID)
    {
        if (weatherID == WeatherConditonID.none)
            Weather = null;
        else
            Weather = WeatherCondtionsDB.Conditions[weatherID];
    }
}
