using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VK.MapSensor
{
    [CustomEditor(typeof(MapDisplay))]
    [CanEditMultipleObjects]
    public class MapDisplayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapDisplay md = (MapDisplay)target;
            md.updateUI();
            int ch = md.mapCamera.GetShape()[2];
            string[] options = new string[ch];
            for (int i = 0; i < options.Length; i++)
                if (md.mapCamera is TerrainCamera)
                    options[i] = ((TerrainCamera)(md.mapCamera)).terrainMap.channels[i].ToString();
                else
                    options[i] = i.ToString();
            DrawDefaultInspector();
            
            GUIStyle bold = new GUIStyle(GUI.skin.label);
            bold.fontStyle = FontStyle.Bold;

            string name = md.mapCamera.GetType().Name;
            GUIContent header = new GUIContent(name + " channels (" + ch + ")", "Select camera channel for each RGB channel");

            GUIStyle width20 = new GUIStyle(GUI.skin.label);
            width20.fixedWidth = 20;
            width20.alignment = TextAnchor.MiddleRight;
            GUIStyle width50 = new GUIStyle(GUI.skin.label);
            width50.fixedWidth = 50;

            GUILayout.BeginVertical();
            GUILayout.Label(header, bold);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Red", width50);
            md.red = EditorGUILayout.Popup(md.red, options);
            md.useRed = GUILayout.Toggle(md.useRed, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Green", width50);
            md.green = EditorGUILayout.Popup(md.green, options);
            md.useGreen = GUILayout.Toggle(md.useGreen, "");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Blue", width50);
            md.blue = EditorGUILayout.Popup(md.blue, options);
            md.useBlue = GUILayout.Toggle(md.useBlue, "");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

    }
}
