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
            SerializedProperty channels = serializedObject.FindProperty("channels");
            SerializedProperty placeObjectsOnMap = serializedObject.FindProperty("placeObjectsOnMap");
            SerializedProperty tags = serializedObject.FindProperty("tags");
            SerializedProperty objects = serializedObject.FindProperty("objects");
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(MapResolution, true);
            EditorGUILayout.PropertyField(channels, true);
            EditorGUILayout.PropertyField(placeObjectsOnMap, true);
            if (tm.placeObjectsOnMap)
            {
                string[] opts = options(tm.channels);

                if (!inChannels(tm.objectsChannel, tm.channels))
                    EditorGUILayout.HelpBox("The objects channel must be in the channel list.", MessageType.Error);
                i = EditorGUILayout.Popup("Objects Channel", i, opts);
                if (i < tm.channels.Length)
                    tm.objectsChannel = tm.channels[i];
                EditorGUILayout.PropertyField(tags, true);
                EditorGUILayout.PropertyField(objects, true);
            }
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            
        }
        private bool inChannels(Channel channel, Channel[] channels)
        {
            foreach (Channel c in channels)
                if (c == channel)
                    return true;
            return false;
        }
        private string[] options(Channel[] channels)
        {
            string[] ops = new string[channels.Length];
            for (int i = 0; i < ops.Length; i++)
                ops[i] = channels[i].ToString();
            return ops;
        }
    }
}