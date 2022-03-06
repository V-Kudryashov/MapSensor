using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VK.MapSensor
{
    [CustomEditor(typeof(TerrainMap))]
    [CanEditMultipleObjects]
    public class TerrainMapEditor : Editor
    {
        int i = 0;
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            
            serializedObject.Update();
            TerrainMap tm = (TerrainMap)target;
            //EditorGUIUtility.LookLikeInspector();
            SerializedProperty MapResolution = serializedObject.FindProperty("MapResolution");
            SerializedProperty mapTr = serializedObject.FindProperty("mapTr");
            SerializedProperty channels = serializedObject.FindProperty("channels");
            SerializedProperty drawObjectsOnMap = serializedObject.FindProperty("drawObjectsOnMap");
            SerializedProperty tags = serializedObject.FindProperty("tags");
            SerializedProperty objects = serializedObject.FindProperty("objects");
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(MapResolution, true);
            EditorGUILayout.PropertyField(mapTr, true);
            EditorGUILayout.PropertyField(channels, true);
            EditorGUILayout.PropertyField(drawObjectsOnMap, true);
            if (tm.drawObjectsOnMap)
            {
                string[] opts = options(tm.channels);

                if (!tm.channels.Contains( tm.objectsChannel))
                    EditorGUILayout.HelpBox("The objects channel must be in the channel list.", MessageType.Error);
                i = EditorGUILayout.Popup("Objects Channel", i, opts);
                if (i < tm.channels.Count)
                    tm.objectsChannel = tm.channels[i];
                EditorGUILayout.PropertyField(tags, true);
                EditorGUILayout.PropertyField(objects, true);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            
        }
        private string[] options(List<Channel> channels)
        {
            string[] ops = new string[channels.Count];
            for (int i = 0; i < ops.Length; i++)
                ops[i] = channels[i].ToString();
            return ops;
        }
    }
}