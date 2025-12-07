using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscen : MonoBehaviour
{
    [SerializeReference]
    [SerializeField] List<CutsceneAction> actions;

    public void AddAction(CutsceneAction action)
    {
        action.Name = action.GetType().ToString();
        actions.Add(action);
    }
}
