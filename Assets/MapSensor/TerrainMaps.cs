using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.MapSensor
{

    public class TerrainMaps : MonoBehaviour
    {
        public Terrain terrain;
        //public float maxHeight = 3;
        [HideInInspector] public float[,] slopeMap;
        /// <summary>
        /// [x, y]{h, x, z}
        /// </summary>
        [HideInInspector] public float[,][] terrainMap;

        private float drawR = 0.03f;
        void Start()
        {
            init();
        }
        private void Update()
        {
            //drawMap();
            //drawNormals();
            //drawTerrainMap();
        }
        public void init()
        {
            if (slopeMap != null)
                return;
            fillMaps();
        }
        public float getHeight(Vector3 pos)
        {
            pos = terrain.transform.InverseTransformPoint(pos);
            return getHeightLocal(pos);
        }
        public float getHeightLocal(Vector3 pos)
        {
            Vector3 size = terrain.terrainData.size;
            float x = pos.x / size.x;
            float y = pos.z / size.z;
            float height = terrain.terrainData.GetInterpolatedHeight(x, y);
            return height;
        }
        public Vector3 drawNormal(Vector3 pos)
        {
            pos = terrain.transform.InverseTransformPoint(pos);
            pos.y = 0;
            Vector3 size = terrain.terrainData.size;
            float x = pos.x / size.x;
            float y = pos.z / size.z;
            Vector3 normal = terrain.terrainData.GetInterpolatedNormal(x, y);
            float angle = terrain.terrainData.GetSteepness(x, y);
            float height = terrain.terrainData.GetInterpolatedHeight(x, y);
            Vector3 startPos = terrain.transform.TransformPoint(pos);
            startPos.y += height;
            Vector3 endN = startPos;
            Vector3 endA = startPos;
            endN += normal;
            endA.y += angle / 18;
            Debug.DrawLine(startPos, endN, Color.black);
            Debug.DrawLine(startPos, endA, Color.white);
            return startPos;
        }
        private void fillMaps()
        {
            Vector3 size = terrain.terrainData.size;
            int res = terrain.terrainData.heightmapResolution;
            float[,] heights = terrain.terrainData.GetHeights(0, 0, res, res);
            slopeMap = new float[res - 1, res - 1];
            terrainMap = new float[res - 1, res - 1][];
            for (int x = 0; x < res - 1; x++)
                for (int y = 0; y < res - 1; y++)
                {
                    Vector3 normal = getNormal(x, y);
                    normal.y = 0;
                    float m = normal.magnitude * 0.0f + getCurvature(x, y) * 1.0f;
                    slopeMap[x, y] = Mathf.Clamp(m, 0, 1);

                    terrainMap[x, y] = new float[3];
                    terrainMap[x, y][0] = getHeight(x, y) / size.y;
                    terrainMap[x, y][1] = normal.x;
                    terrainMap[x, y][2] = normal.z;
                }

            foreach (TreeInstance tree in terrain.terrainData.treeInstances)
            {
                getXYNormalised(tree.position, out int x, out int y);
                slopeMap[x, y] = 1;

                terrainMap[x, y] = new float[3];
                terrainMap[x, y][0] = 1;
                terrainMap[x, y][1] = 1;
                terrainMap[x, y][2] = 1;
            }
        }
        private float getCurvature(int x, int y)
        {
            float maxX = slopeMap.GetLength(0) - 1;
            float maxY = slopeMap.GetLength(1) - 1;
            Vector3 n = getNormal(x, y);
            Vector3 n1, n2, n3, n4;
            if (x > 1)
                n1 = getNormal(x - 1, y);
            else
                n1 = n;
            if (x < maxX)
                n2 = getNormal(x + 1, y);
            else
                n2 = n;
            if (y > 1)
                n3 = getNormal(x, y - 1);
            else
                n3 = n;
            if (y < maxY)
                n4 = getNormal(x, y + 1);
            else
                n4 = n;
            float m1 = (n - n1).magnitude;
            float m2 = (n - n2).magnitude;
            float m3 = (n - n3).magnitude;
            float m4 = (n - n4).magnitude;
            return m1 + m2 + m3 + m4;
        }
        private Vector3 getNormal(int x, int y)
        {
            Vector3 pos = getPosNormalized(x, y);
            Vector3 normal = terrain.terrainData.GetInterpolatedNormal(pos.x, pos.z);
            return normal;
        }
        private float getHeight(int x, int y)
        {
            Vector3 pos = getPosNormalized(x, y);
            float height = terrain.terrainData.GetInterpolatedHeight(pos.x, pos.z);
            return height;
        }

        private void drawMap()
        {
            int i1 = (int)(terrainMap.GetLength(0) * (0.5f - drawR));
            int i2 = (int)(terrainMap.GetLength(0) * (0.5f + drawR));
            int j1 = (int)(terrainMap.GetLength(1) * (0.5f - drawR));
            int j2 = (int)(terrainMap.GetLength(1) * (0.5f + drawR));
            Vector3 size = terrain.terrainData.size;
            for (int i = i1; i < i2; i++)
                for (int j = j1; j < j2; j++)
                    if (slopeMap[i, j] != 0)
                    {
                        Vector3 pos = getPos(i, j);
                        Vector3 startPos = terrain.transform.TransformPoint(pos);
                        float x = pos.x / size.x;
                        float y = pos.z / size.z;
                        float height = terrain.terrainData.GetInterpolatedHeight(x, y);
                        startPos.y += height;
                        Vector3 endPos = startPos;
                        endPos.y += slopeMap[i, j] * 1;
                        Debug.DrawLine(startPos, endPos, Color.white);
                    }

        }
        private void drawNormals()
        {
            int i1 = (int)(terrainMap.GetLength(0) * (0.5f - drawR));
            int i2 = (int)(terrainMap.GetLength(0) * (0.5f + drawR));
            int j1 = (int)(terrainMap.GetLength(1) * (0.5f - drawR));
            int j2 = (int)(terrainMap.GetLength(1) * (0.5f + drawR));
            int res = terrain.terrainData.heightmapResolution;
            Vector3 size = terrain.terrainData.size;
            for (int i = i1; i < i2; i++)
                for (int j = j1; j < j2; j++)
                    if (slopeMap[i, j] != 0)
                    {
                        Vector3 pos = getPos(i, j);
                        float x = pos.x / size.x;
                        float y = pos.z / size.z;
                        Vector3 normal = terrain.terrainData.GetInterpolatedNormal(x, y);
                        float height = terrain.terrainData.GetInterpolatedHeight(x, y);
                        Vector3 startPos = terrain.transform.TransformPoint(pos);
                        startPos.y += height;
                        //Vector3 endPos = new Vector3(startPos.x, startPos.y, startPos.z);
                        Vector3 endPos = startPos;
                        endPos += normal;
                        Debug.DrawLine(startPos, endPos, Color.black);
                    }

        }
        private void drawTerrainMap()
        {
            Vector3 size = terrain.terrainData.size;
            int i1 = (int)(terrainMap.GetLength(0) * (0.5f - drawR));
            int i2 = (int)(terrainMap.GetLength(0) * (0.5f + drawR));
            int j1 = (int)(terrainMap.GetLength(1) * (0.5f - drawR));
            int j2 = (int)(terrainMap.GetLength(1) * (0.5f + drawR));
            for (int i = i1; i < i2; i++)
                for (int j = j1; j < j2; j++)
                {
                    float h = terrainMap[i, j][0];
                    float x = terrainMap[i, j][1];
                    float z = terrainMap[i, j][2];
                    if (h == 0)
                        continue;
                    float height = h * size.y;
                    Vector3 pos = getPos(i, j);
                    Vector3 startPos = terrain.transform.TransformPoint(pos);
                    startPos.y += height;
                    Vector3 endX = startPos;
                    Vector3 endZ = startPos;
                    endX.x += x;
                    endZ.z += z;
                    Debug.DrawLine(startPos, endX, Color.red);
                    Debug.DrawLine(startPos, endZ, Color.blue);
                }

        }

        private Vector3 getPosGlobal(int x, int y)
        {
            Vector3 pos = getPos(x, y);
            return terrain.transform.TransformPoint(pos);
        }
        private Vector3 getPos(int x, int y)
        {
            Vector3 size = terrain.terrainData.size;
            Vector3 posN = getPosNormalized(x, y);
            Vector3 pos = new Vector3(posN.x * size.x, 0, posN.z * size.z);
            return pos;
        }
        private Vector3 getPosNormalized(int x, int y)
        {
            int res = terrain.terrainData.heightmapResolution;
            Vector3 pos = new Vector3();
            pos.x = (y + 0.5f) / (res - 1);
            pos.z = (x + 0.5f) / (res - 1);
            return pos;
        }
        private void getXY(Vector3 pos, out int x, out int y)
        {
            Vector3 size = terrain.terrainData.size;
            pos.x /= size.x;
            pos.z /= size.z;
            getXYNormalised(pos, out x, out y);
        }
        private void getXYNormalised(Vector3 pos, out int x, out int y)
        {
            int res = terrain.terrainData.heightmapResolution;
            float X = pos.z * (res - 1) - 0.5f;
            float Y = pos.x * (res - 1) - 0.5f;
            x = Mathf.RoundToInt(X);
            y = Mathf.RoundToInt(Y);
        }
    }
}