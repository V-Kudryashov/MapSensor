using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HeightMapOld : MonoBehaviour
{
    public RenderTexture renderTexture;
    public RawImage rawImage2D;
    public GameObject heightMapPlane;
    public Transform heightMapCamera;
    public Material heightMapMaterial;
    public GameObject columns;
    public bool AddHills = false;
    public int hillsCount = 30;
    public bool AddPeaks = false;
    public bool AddTrees = false;
    public Transform car;
    public Transform target;

    private int mapW;
    private int mapH;
    private float sempleSize;
    private float maxHeight;
    private float[,] heights;
    private Color[] colors;
    private Color[] colorsSave;

    private int saveX;
    private int saveY;
    private float saveH;
    private Color saveColor;

    private Terrain terrain;
    private Texture2D texture2D;
    void Start()
    {
        init();
    }
    public void init()
    {
        if (terrain != null)
            return;
        TerrainCollider collider = GetComponent<TerrainCollider>();
        terrain = GetComponent<Terrain>();
        TerrainData terrainData = Instantiate<TerrainData>(terrain.terrainData);
        terrain.terrainData = terrainData;
        collider.terrainData = terrainData;

        mapW = terrain.terrainData.heightmapResolution;
        mapH = terrain.terrainData.heightmapResolution;
        colors = new Color[mapW * mapH];
        colorsSave = new Color[mapW * mapH];
        heights = terrain.terrainData.GetHeights(0, 0, mapW, mapH);

        //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        //stopWatch.Start();
        clearHeights();
        addHills();
        addPeaks();
        //terrain.terrainData.SetTreeInstances(terrain.terrainData.treeInstances, true);
        //addTrees();
        drawMap();
        saveColors();
        //stopWatch.Stop();
        //TimeSpan ts = stopWatch.Elapsed;
        //Debug.Log("t=" + ts.Milliseconds + "ms");
    }
    private void clearHeights()
    {
        for (int i = 0; i < mapW; i++)
            for (int j = 0; j < mapH; j++)
                heights[i, j] = 0;
        if (!AddTrees)
        {
            terrain.terrainData.SetTreeInstances(new TreeInstance[0], false);
            GetComponent<TerrainCollider>().terrainData = terrain.terrainData;
        }
    }
    public void addHills()
    {
        if (!AddHills)
            return;
        float maxH = 3; //3
        float maxW = 30;//30
        float r = 100;//100
        for (int i = 0; i < hillsCount; i++)
        {
            float h = UnityEngine.Random.Range(0.1f, maxH);
            float w = UnityEngine.Random.Range(0.0f, maxW);
            float posX = UnityEngine.Random.Range(-r, r);
            float posZ = UnityEngine.Random.Range(-r, r);
            Vector3 pos = new Vector3(posX, 0, posZ);
            addHill(pos, h, w);
        }
        /*
        for (int i = 0; i < 5; i++)
            for (int j = 0; j < 10; j++)
            {
                float h = i;
                float w = j * 2;
                float posX = i * 20;
                float posZ = j * 20;
                Vector3 pos = new Vector3(posX, 0, posZ);
                addHill(pos, h, w);
            }
        */
        terrain.terrainData.SetHeights(0, 0, heights);
    }
    private void addHill(Vector3 pos, float height, float width)
    {
        int r = Mathf.RoundToInt(width / 2);
        getXYheights(pos, out int cX, out int cY);
        int x1 = cX - r;
        int x2 = cX + r;
        int y1 = cY - r;
        int y2 = cY + r;
        float m = 1f / terrain.terrainData.size.y;
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
                heights[x, y] += h * m;
            }
    }
    public void addPeaks()
    {
        int r = 200;
        int count = 500; // 2000;
        Vector3 c = target.position;
        float m = 1f / terrain.terrainData.size.y;

        if (AddPeaks)
            for (int i = 0; i < count; i++)
            {
                float posX = UnityEngine.Random.Range(c.x - r, c.x + r);
                float posZ = UnityEngine.Random.Range(c.z - r, c.z + r);
                Vector3 pos = car.position;
                pos.x = posX;
                pos.z = posZ;
                float d = (pos - car.position).magnitude;
                if (d < 2)
                    pos.z += 4;
                getXY(pos, out int x, out int y);
                float h = UnityEngine.Random.Range(0.0f, 2f);
                heights[x, y] += h * m;
            }

        terrain.terrainData.SetHeights(0, 0, heights);
    }
    public void addTrees()
    {
        int r = 200;
        int count = 2000;
        TreeInstance[] trees = new TreeInstance[count];
        Vector3 c = target.position;
        float m = 1f / terrain.terrainData.size.y;
        float mX = 1f / terrain.terrainData.size.x;
        float mZ = 1f / terrain.terrainData.size.z;

        if (AddTrees)
        {
            for (int i = 0; i < count; i++)
            {
                float posX = UnityEngine.Random.Range(c.x - r, c.x + r);
                float posZ = UnityEngine.Random.Range(c.z - r, c.z + r);
                Vector3 pos = car.position;
                pos.x = posX;
                pos.z = posZ;
                float d = (pos - car.position).magnitude;
                if (d < 2)
                    pos.z += 4;
                getXYheights(pos, out int x, out int y);
                pos.x = pos.x * mX + 0.5f;
                pos.z = pos.z * mZ + 0.5f;
                pos.y = 0.0f;// heights[x, y];

                TreeInstance tree = new TreeInstance();
                tree.prototypeIndex = 0;
                tree.color = Color.white;
                tree.lightmapColor = Color.white;
                tree.heightScale = 1;
                tree.widthScale = 1;
                tree.position = pos;
                tree.rotation = UnityEngine.Random.Range(0, Mathf.PI * 2);

                trees[i] = tree;
            }
            terrain.terrainData.SetTreeInstances(trees, true);
        }
    }
    private void drawMap()
    {
        texture2D = new Texture2D(mapH, mapW);
        if (rawImage2D != null)
            if (rawImage2D.texture == null)
                rawImage2D.texture = texture2D;

        heights = terrain.terrainData.GetHeights(0, 0, mapW, mapH);
        float maxH = 0;
        for (int i = 0; i < texture2D.width; i++)
            for (int j = 0; j < texture2D.height; j++)
            {
                //float h = heights[j, i] * terrain.terrainData.heightmapHeight / 4f;
                float h = gradient4(j, i) * terrain.terrainData.heightmapResolution / 3;
                Color color = new Color(h, h, h);
                int x = texture2D.width - i - 1;
                int y = texture2D.height - j - 1;
                colors[x + y * mapH] = color;
                //texture2D.SetPixel(j, i, color);
                if (h > maxH)
                    maxH = h;
            }
        drawTrees();
        drawColumns(colors);
        updateTexture();
    }
    private float gradient(int x, int y)
    {
        float dx = 1;
        if (x < texture2D.height - 1 && x > 0)
            dx = heights[x, y] - heights[x + 1, y];
        float dy = 1;
        if (y < texture2D.width - 1 && y > 0)
            dy = heights[x, y] - heights[x, y + 1];
        Vector2 gradient = new Vector2(dx, dy);
        float magnitude = gradient.magnitude;
        if (magnitude <  1 &&  magnitude > 0)
        {

        }
        if (heights[x, y] > 0)
        {

        }
        return gradient.magnitude;
    }
    private float gradient4(int x, int y)
    {
        float m = 1f / terrain.terrainData.size.y;
        float d1 = 2 * m;
        float d2 = 2 * m;
        if (x < texture2D.height - 1 && x > 0 && y < texture2D.width - 1 && y > 0)
        {
            d1 = Mathf.Abs(heights[x, y] - heights[x + 1, y + 1]);
            d2 = Mathf.Abs(heights[x + 1, y] - heights[x, y + 1]);
        }
        float max = Mathf.Max(d1, d2);
        return max;
    }
    private void drawTrees()
    {
        Color white = Color.white;
        foreach (TreeInstance t in terrain.terrainData.treeInstances)
        {
            Vector3 pos = t.position;
            int x = (int)(texture2D.width * (1 - pos.x)) + 0;
            int y = (int)(texture2D.height * (1 - pos.z)) + 1;
            colors[x + y * mapH] = white;
        }
    }
    private void drawColumns(Color[] colors)
    {
        Color white = Color.white;
        foreach (Transform c in columns.transform)
        {
            drawPoint(c.position, white);
        }
    }
    public void drawPoint(Vector3 pos, Color color)
    {
        getXY(pos, out int x, out int y);
        colors[x + y * mapH] = color;
    }
    public Color getColor(Vector3 pos)
    {
        getXY(pos, out int x, out int y);
        return colors[x + y * mapH];
    }
    public float getHeight(Vector3 pos)
    {
        getXYheights(pos, out int x, out int y);
        if (x < texture2D.height - 1 && x > 0 && y < texture2D.width - 1 && y > 0)
            return heights[x, y] * terrain.terrainData.size.y;
        else
            return -1;
    }
    public Color getColor(int x, int y)
    {
        int index = x + y * mapH;
        if (index >= colors.Length || index < 0)
            return Color.white;
        return colors[index];
    }
    public Vector3 getPos(int x, int y)
    {
        Vector3 pos = new Vector3();
        pos.x = 1 - (float)x / texture2D.width;
        pos.z = 1 - (float)y / texture2D.height;
        pos.x *= terrain.terrainData.size.x;
        pos.z *= terrain.terrainData.size.z;
        pos.x -= terrain.terrainData.size.x / 2;
        pos.z -= terrain.terrainData.size.z / 2;
        return pos;
    }
    public void getXY(Vector3 pos, out int x, out int y)
    {
        pos.x += terrain.terrainData.size.x / 2;
        pos.z += terrain.terrainData.size.z / 2;
        pos.x /= terrain.terrainData.size.x;
        pos.z /= terrain.terrainData.size.z;
        x = Mathf.RoundToInt(mapW * (1 - pos.x));
        y = Mathf.RoundToInt(mapH * (1 - pos.z));
    }
    public void getXYheights(Vector3 pos, out int x, out int y)
    {
        Vector3 iPos = pos;
        iPos.x = -pos.z;
        iPos.z = -pos.x;
        getXY(iPos, out x, out y);
    }
    public void updateTexture()
    {
        texture2D.SetPixels(colors);
        texture2D.Apply();
    }
    public Texture2D getTexture()
    {
        return texture2D;
    }
    private void saveColors()
    {
        for (int i = 0; i < colors.Length; i++)
            colorsSave[i] = colors[i];
    }
    public void restoreColors()
    {
        for (int i = 0; i < colors.Length; i++)
            colors[i] = colorsSave[i];
        updateTexture();
    }
}
