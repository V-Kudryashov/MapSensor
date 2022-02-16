using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Editor;
using UnityEditor;
using UnityEngine;

namespace VK.MapSensor
{
    [CustomEditor(typeof(MapSensorComponent))]
    [CanEditMultipleObjects]
    internal class MapSensorComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;
            so.Update();

            // Drawing the RenderTextureComponent
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(!EditorUtilities.CanUpdateModelProperties());
            {
                EditorGUILayout.PropertyField(so.FindProperty("m_MapCamera"), true);
                EditorGUILayout.PropertyField(so.FindProperty("m_SensorName"), true);
                EditorGUILayout.PropertyField(so.FindProperty("m_ObservationStacks"), true);
            }
            EditorGUI.EndDisabledGroup();

            var requireSensorUpdate = EditorGUI.EndChangeCheck();
            so.ApplyModifiedProperties();

            if (requireSensorUpdate)
            {
                UpdateSensor();
            }
        }

        void UpdateSensor()
        {
        }
    }
}