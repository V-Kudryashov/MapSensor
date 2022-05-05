using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class TerrainDataGenerator : MonoBehaviour
{
    public Terrain terrain;
    public float zeroLevel;
    public Bump hils;
    public Bump peaks;
    public HeightsParams heightsParams;
    public string assetPath = "Assets/Example/Terrains/Terrain0.asset";
    /// <summary>
    /// [x,y] -> (y, 0, x)
    /// </summary>
    private float[,] heights;
    private Terrain currentTerrain;

    void Start()
    {
        
    }
    private void Update()
    {
        //drawHeights();
    }
    public void generateData(Terrain t)
    {
        if (t == null)
            currentTerrain = terrain;
        else
            currentTerrain = t;
        //TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/OffRoad/Terrains/Terrain0.asset");
        TerrainCollider collider = currentTerrain.gameObject.GetComponent<TerrainCollider>();
        TerrainData terrainData = collider.terrainData;
       
        Vector3 size = terrainData.size;
        terrainData.size = new Vector3(size.x, heightsParams.terrainHeight, size.z);

        int res = terrainData.heightmapResolution;
        heights = terrainData.GetHeights(0, 0, res, res);

        clearData(terrainData);
        addBumps(terrainData, hils);
        addBumps(terrainData, peaks);
        if (heightsParams.CentralPeak)
            addCentralPeak(terrainData);
        if (heightsParams.AddTrees)
            addTrees(terrainData);

        terrainData.SetHeights(0, 0, heights);
        currentTerrain.terrainData = terrainData;
    }
    private void clearData(TerrainData terrainData)
    {
        int res = terrainData.heightmapResolution;
        heights = terrainData.GetHeights(0, 0, res, res);
        for (int i = 0; i < heights.GetLength(0); i++)
            for (int j = 0; j < heights.GetLength(1); j++)
                heights[i, j] = zeroLevel / terrainData.size.y;

        terrainData.SetHeights(0, 0, heights);
        terrainData.SetTreeInstances(new TreeInstance[0], false);
    }
    private void addBumps(TerrainData terrainData, Bump bump)
    {
        if (!bump.Add)
            return;
        float maxH = bump.maxH / terrainData.size.y;
        float minH = bump.minH / terrainData.size.y;
        int maxW = bump.w;
        int r = bump.r;
        for (int i = 0; i < bump.n; i++)
        {
            float h = UnityEngine.Random.Range(minH, maxH);
            int w = UnityEngine.Random.Range(0, maxW);
            int cX = UnityEngine.Random.Range(-r, r);
            int cY = UnityEngine.Random.Range(-r, r);
            if (cX * cX + cY * cY < 100)
                continue;
            cX += heights.GetLength(0) / 2;
            cY += heights.GetLength(1) / 2;
            addBump(cX, cY, h, w);
        }
        /*
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 10; j++)
            {
                float h = 0.001f * i;
                int w = j * 2;
                int posX = i * 20 + heights.GetLength(0) / 2;
                int posY = j * 20 + heights.GetLength(1) / 2;
                addHill(posX, posY, h, w);
            }
        */
        terrainData.SetHeights(0, 0, heights);
    }
    private void addBump(int cX, int cY, float height, int width)
    {
        int r = Mathf.RoundToInt(width / 2);
        int x1 = cX - r;
        int x2 = cX + r;
        int y1 = cY - r;
        int y2 = cY + r;
        for (int x = x1; x <= x2; x++)
            for (int y = y1; y <= y2; y++)
            {
                float rx = cX - x;
                float ry = cY - y;
                float r2 = rx * rx + ry * ry;
                float k = 1;
                if (r > 0)
                    k = 1f / r / r;
                float h = Mathf.Exp(-r2 * k * 4) * height;
                heights[x, y] += h;
            }
    }
    private void addCentralPeak(TerrainData terrainData)
    {
        int x = heights.GetLength(0) / 2 + 2;
        int y = heights.GetLength(1) / 2;

        float m = 1f / terrainData.size.y;
        float h = 2;
        heights[x, y] += h * m;
    }
    private void addTrees(TerrainData terrainData)
    {
        float r = 0.4f;
        int count = heightsParams.treesCount; //2000
        TreeInstance[] trees = new TreeInstance[count];
        Vector3 c = new Vector3(0.5f, 0, 0.5f); //target.position;

        for (int i = 0; i < count; i++)
        {
            float posX = UnityEngine.Random.Range(c.x - r, c.x + r);
            float posZ = UnityEngine.Random.Range(c.z - r, c.z + r);
            Vector3 pos = new Vector3(posX, 0, posZ);
            pos.y = 0.0f;// heights[x, y];
            float d = (pos - new Vector3(0.5f,0,0.5f)).magnitude;
            if (d < 0.01f) //5m
                pos.x += 0.02f; //10m

            TreeInstance tree = new TreeInstance();
            tree.prototypeIndex = heightsParams.prototypeIndex;
            tree.color = Color.white;
            tree.lightmapColor = Color.white;
            tree.heightScale = 1;
            tree.widthScale = 1;
            tree.position = pos;
            tree.rotation = UnityEngine.Random.Range(0, Mathf.PI * 2);

            trees[i] = tree;
        }
        terrainData.SetTreeInstances(trees, true);
    }
    private void drawHeights()
    {
        int res = currentTerrain.terrainData.heightmapResolution;
        float[,] heights = currentTerrain.terrainData.GetHeights(0, 0, res, res);
        for (int i = 0; i < heights.GetLength(0); i++)
            for (int j = 0; j < heights.GetLength(1); j++)
                if (heights[i, j] != 0)
                {
                    Vector3 pos = getPos(i, j);
                    Vector3 startPos = currentTerrain.transform.TransformPoint(pos);
                    Vector3 endPos = new Vector3(startPos.x, startPos.y, startPos.z);
                    endPos.y += heights[i, j] * 2 * currentTerrain.terrainData.size.y;
                    UnityEngine.Debug.DrawLine(startPos, endPos, Color.red);
                }

    }
    private Vector3 getPos(int x, int y)
    {
        int res = currentTerrain.terrainData.heightmapResolution;
        Vector3 size = currentTerrain.terrainData.size;
        Vector3 pos = new Vector3();
        pos.x = y * size.x / (res - 1);
        pos.z = x * size.z / (res - 1);
        return pos;
    }
    [System.Serializable]
    public class HeightsParams
    {
        public float terrainHeight = 600;
        public bool CentralPeak = false;
        public bool AddTrees = false;
        [Tooltip("default value 2000")]
        public int treesCount = 2000;
        public int prototypeIndex = 0;
    }
    [System.Serializable]
    public class Bump
    {
        public bool Add = false;
        [Tooltip("hills - 100, peaks - 200")]
        public int r = 100;
        [Tooltip("hills - 200, peaks - 2000")]
        public int n = 200;
        public float minH = 0;
        [Tooltip("hills - 3, peaks - 2")]
        public float maxH = 3;
        [Tooltip("hills - 50, peaks - 0")]
        public int w = 50;
    }
}
