using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VK.MapSensor;

[CustomEditor(typeof(TerrainMapDisplay))]
[CanEditMultipleObjects]
public class TerrainMapDisplayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TerrainMapDisplay md = (TerrainMapDisplay)target;
        md.updateUI();
        if (md.mapCamera == null)
            return;
        List<Channel> channels = md.mapCamera.terrainMap.channels;
        int ch = md.mapCamera.GetShape()[2];

        GUIStyle bold = new GUIStyle(GUI.skin.label);
        bold.fontStyle = FontStyle.Bold;

        string name = md.mapCamera.GetType().Name;
        GUIContent header = new GUIContent(name + " channels (" + ch + ")", "Select camera channel for each RGB channel");

        GUIStyle width130 = new GUIStyle(GUI.skin.label);
        width130.fixedWidth = 130;

        GUILayout.BeginVertical();
        GUILayout.Label(header, bold);

        foreach (TerrainMapDisplay.CameraChannel  c in md.channels)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(c.channel.ToString(), width130);
            c.r = GUILayout.Toggle(c.r, "R");
            c.g = GUILayout.Toggle(c.g, "G");
            c.b = GUILayout.Toggle(c.b, "B");
            GUILayout.EndHorizontal();

        }

        GUILayout.EndVertical();
    }

}

