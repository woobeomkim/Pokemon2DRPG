using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cutscen))]
public class CutsceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var cutScene = target as Cutscen;

        if(GUILayout.Button("Add Dialogue Action"))
        {
            cutScene.AddAction(new DialogueAction());
        }
        else if (GUILayout.Button("Add Move Actor Action"))
        {
            cutScene.AddAction(new MoveActorAction());
        }
        else if(GUILayout.Button("Add Turn Actor Action"))
        {
            cutScene.AddAction(new TurnActorAction());
        }
        else if (GUILayout.Button("Add Teleport Object Action"))
        {
            cutScene.AddAction(new TeleportObjectAction());
        }
        base.OnInspectorGUI();
    }
}
