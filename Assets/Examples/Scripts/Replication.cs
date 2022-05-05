using UnityEditor;
using UnityEngine;

public class Replication : MonoBehaviour
{
    public GameObject terrainGO;
    public TerrainDataGenerator generator;
    public bool generate;
    public bool usePlane;
    public bool addColumns = false;
    public int semplesN = 20;
    public int startPeaksN = 3;
    public float step = -10;

    public void replicate()
    {
        Terrain terrain = terrainGO.GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;
//#if UNITY_EDITOR
//        terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/Example/Terrains/Terrain0.asset");
//#endif
//        terrain.terrainData = terrainData;
        terrainGO.GetComponent<TerrainCollider>().terrainData = terrainData;
        
        Material terrainMaterial = terrain.materialTemplate;

        for (int i = 0; i < semplesN; i++)
        {
            Vector3 pos = terrainGO.transform.position;
            GameObject clone;
            if (i == 0)
            {
                clone = terrainGO;
            }
            else
            {
                pos.y = pos.y + step * i;
                clone = Instantiate(terrainGO, pos, terrainGO.transform.rotation);
                clone.name = terrainGO.name + i;
                Terrain t = clone.GetComponent<Terrain>();
                string path = "Assets/Examples/Terrains/Terrain" + i + ".asset";
#if UNITY_EDITOR
                terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
#endif
                t.terrainData = terrainData;
                t.materialTemplate = terrainMaterial;

                TerrainCollider collider = clone.GetComponent<TerrainCollider>();
                collider.terrainData = terrainData;
                if (generate)
                {
                    generateData(t);
                }
            }
        }
    }
    private void generateData(Terrain terrain)
    {
        generator.generateData(terrain);
    }
}
