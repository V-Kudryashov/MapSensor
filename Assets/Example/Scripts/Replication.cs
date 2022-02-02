using UnityEditor;
using UnityEngine;

public class Replication : MonoBehaviour
{
    public GameObject terrainGO;
    public TerrainDataGenerator generator;
    public bool generate;
    public TerrainData plane;
    public bool usePlane;
    public bool addColumns = false;
    public int semplesN = 20;
    public int startPeaksN = 3;
    public float step = -10;

    public void replicate()
    {
        TerrainData terrainData = plane;
#if UNITY_EDITOR
        terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/OffRoad/Terrains/Terrain0.asset");
#endif
        if (usePlane)
            terrainData = plane;
        Terrain terrain = terrainGO.GetComponent<Terrain>();
        terrain.terrainData = terrainData;
        terrainGO.GetComponent<TerrainCollider>().terrainData = terrainData;
        
        Material terrainMaterial = terrain.materialTemplate;

        for (int i = 0; i < semplesN; i++)
        {
            Vector3 pos = terrainGO.transform.position;
            if (i == 0)
            {
            }
            else
            {
                pos.y = pos.y + step * i;
                GameObject clone = Instantiate(terrainGO, pos, terrainGO.transform.rotation);
                clone.name = terrainGO.name + i;
                Terrain t = clone.GetComponent<Terrain>();
                string path = "Assets/OffRoad/Terrains/Terrain" + i + ".asset";
#if UNITY_EDITOR
                if (usePlane)
                    terrainData = plane;
                else
                    terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
#endif
                t.terrainData = terrainData;
                t.materialType = Terrain.MaterialType.Custom;
                t.materialTemplate = terrainMaterial;

                TerrainCollider collider = clone.GetComponent<TerrainCollider>();
                collider.terrainData = terrainData;
                if (generate)
                {
                    generateData(t);
                }
            }
        }
        if (generate)
        {
            generateData(terrain);
        }
    }
    private void generateData(Terrain terrain)
    {
        Vector3 size = terrain.terrainData.size;
        terrain.terrainData.size = new Vector3(size.x, generator.heightsParams.terrainHeight, size.z);
        generator.terrain = terrain;
        generator.generateData();
    }
}
