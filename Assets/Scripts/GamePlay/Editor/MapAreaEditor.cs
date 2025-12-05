using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int totalChance = serializedObject.FindProperty("totalChance").intValue;
        int totalChanceInWater = serializedObject.FindProperty("totalChanceInWater").intValue;

        var style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;

        GUILayout.Label($"Total Cahnce = {totalChance}",style);
        GUILayout.Label($"Total CahnceInWater = {totalChanceInWater}",style);

        if(totalChance != 100 && totalChance != -1)
            EditorGUILayout.HelpBox($"The total chance percentage of pokemon in grass is {totalChance} and not 100", MessageType.Error);
        if (totalChanceInWater != 100 && totalChanceInWater != -1)
            EditorGUILayout.HelpBox($"The total chance percentage of pokemon in water is {totalChanceInWater} and not 100", MessageType.Error);
    }
}
