using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(SceneDetails))]
public class SceneDetailsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Open Scene"))
            {
                foreach (var t in targets)
                {
                    var sceneDetails = t as SceneDetails;
                    if (sceneDetails != null)
                        sceneDetails.OpenSceneInEditor();
                }
            }
            if(GUILayout.Button("Close Scene"))
            {
                foreach (var t in targets)
                {
                    var sceneDetails = t as SceneDetails;
                    if (sceneDetails != null)
                        sceneDetails.CloseSceneInEditor();
                }
            }
        }
        base.OnInspectorGUI();
    }
}
