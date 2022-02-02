using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Replication))]
public class ReplicationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Replication replication = (Replication)target;
        if (GUILayout.Button("Replicate"))
            replication.replicate();
    }
}
