using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RewardsMonitor))]
public class RewardsMonitorEditor : Editor
{
    RewardsMonitor monitor;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        monitor = (RewardsMonitor)target;

        if (monitor.rewardsDic == null)
            return;
        Rewards[] arr = (Rewards[])Enum.GetValues(typeof(Rewards));
        
        GUILayout.BeginVertical();
        float total = 0;
        foreach (Rewards r in arr)
        {
            GUILayout.BeginHorizontal();
            monitor.rewardsDic[r].active = GUILayout.Toggle(monitor.rewardsDic[r].active, "");
            EditorGUILayout.Slider(r.ToString(), monitor.rewardsDic[r].value, -5, 5);
            GUILayout.EndHorizontal();
            total += monitor.rewardsDic[r].value;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Toggle(true, "");
        EditorGUILayout.Slider("Total", total, -20, 20);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
    private void drawLine(Rewards r, string name, float value)
    {

    }
}
