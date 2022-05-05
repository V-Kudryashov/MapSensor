using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainDataGenerator))]
public class TerrainDataGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainDataGenerator generator = (TerrainDataGenerator)target;
        if (GUILayout.Button("Generate data"))
            generator.generateData(null);
    }
}
