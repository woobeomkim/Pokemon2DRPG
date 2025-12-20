using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
   public string Name { get; set; }
    public string Description { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAttack { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpAttack { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyDefense { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpDefense { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifySpeed { get; set; }
    public Func<float, Pokemon, Pokemon, Move, float> OnModifyAccuracy { get; set; }
}
