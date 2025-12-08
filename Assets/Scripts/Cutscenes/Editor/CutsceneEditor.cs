using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cutscene))]
public class CutsceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var cutScene = target as Cutscene;

        using (var scope = new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Dialogue"))
            {
                cutScene.AddAction(new DialogueAction());
            }
            else if (GUILayout.Button("Move Actor"))
            {
                cutScene.AddAction(new MoveActorAction());
            }
            else if (GUILayout.Button("Turn Actor"))
            {
                cutScene.AddAction(new TurnActorAction());
            }
        }

        using (var scope = new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Teleport Object"))
            {
                cutScene.AddAction(new TeleportObjectAction());
            }
            else if (GUILayout.Button("Enable Object"))
            {
                cutScene.AddAction(new EnableObjectAction());
            }
            else if (GUILayout.Button("Disable Object"))
            {
                cutScene.AddAction(new DisableObjectAction());
            }
        }

        using (var scope = new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("NPC Interact"))
            {
                cutScene.AddAction(new NPCInteractAction());
            }
            else if (GUILayout.Button("FadeIn"))
            {
                cutScene.AddAction(new FadeInAction());
            }
            else if (GUILayout.Button("FadeOut"))
            {
                cutScene.AddAction(new FadeOutAction());
            }
        }
        base.OnInspectorGUI();
    }
}
