using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

[System.Serializable]
public class CutsceneAction
{
    [SerializeField] string name;

    public string Name
    {
        get => name;
        set => name = value;
    }
}
