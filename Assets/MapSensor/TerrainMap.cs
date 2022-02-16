using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace VK.MapSensor
{
    public enum Channel
    {
        Height = 0,
        NormalX = 1,
        NormalY = 2,
        NormalMagnitude = 3,
        CurvatureX = 4,
        CurvatureY = 5,
        CurvatureMagnitude = 6,
        Objects = 7
    }
    public enum Aggregation
    {
        Override = 0,
        Average = 1,
        Add = 2
    }

    [AddComponentMenu("ML Agents/Map/Tarran Map")]
    [RequireComponent(typeof(Terrain))]
    public class TerrainMap : MonoBehaviour
    {
        public int MapResolution = 513;
        public Channel[] channels;
        public bool placeObjectsOnMap;
        public Channel objectsChannel;
        [TagField]
        public string[] tags;
        public GameObject[] objects;
        public float[,,] Map { get { return map; } }

        private float[,,] map;
        private float[,,] originalMap;
        private float[,] heights;
        private Vector3[,] normals;
        private Vector2[,] curvatures;
        private float[,] objectsMap;
        private Terrain terrain;
        private TerrainData terrainData;
        private Vector3 size;
        
        private float kx, kz;
        private List<Point> points;
        private int pointsCount = 0;

        void Start()
        {
            //tags = UnityEditorInternal.InternalEditorUtility.tags;
            Init();
        }
        private void FixedUpdate()
        {
            Profiler.BeginSample("UpdateMap");
            UpdateMap();
            Profiler.EndSample();
        }
        public void Init()
        {
            if (map != null)
                return;
            terrain = GetComponent<Terrain>();
            terrainData = terrain.terrainData;
            size = terrain.terrainData.size;

            fillHeights(); // 13 ms
            fillNormals(); // 287 ms
            fillCurvatures(); // 115 ms
            objectsMap = new float[MapResolution, MapResolution];

            kx = (MapResolution - 1) / terrain.terrainData.size.x;
            kz = (MapResolution - 1) / terrain.terrainData.size.z;
            points = new List<Point>(200);
            for (int i = 0; i < 200; i++)
                points.Add(new Point(0, 0));
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();
            fillMap(); // ch7 718
            //stopWatch.Stop();
            //Double ms = stopWatch.Elapsed.TotalMilliseconds;
        }
        public Terrain GetTarrain()
        {
            return GetComponent<Terrain>();
        }
        public void UpdateMap()
        {
            restoreMap();
            pointsCount = 0;
            foreach (GameObject go in objects)
                drawObject(go);
            foreach (string tag in tags)
                if (!String.IsNullOrEmpty(tag))
                    foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
                        drawObject(go);
        }
        private void fillHeights()
        {
            float interval = 1f / (MapResolution - 1);
            heights = terrainData.GetInterpolatedHeights(0, 0, MapResolution, MapResolution, interval, interval);
        }
        private void fillNormals()
        {
            float interval = 1f / (MapResolution - 1);
            normals = new Vector3[MapResolution, MapResolution];
            for (int x = 0; x < MapResolution; x++)
                for (int y = 0; y < MapResolution; y++)
                {
                    float X = x * interval;
                    float Y = y * interval;
                    normals[x, y] = terrainData.GetInterpolatedNormal(X, Y);
                    normals[x, y].y = 0;
                }
        }
        private void fillCurvatures()
        {
            curvatures = new Vector2[MapResolution, MapResolution];
            for (int x = 0; x < MapResolution; x++)
                for (int y = 0; y < MapResolution; y++)
                {
                    int x1 = x - 1;
                    int x2 = x + 1;
                    int y1 = y - 1;
                    int y2 = y + 1;
                    x1 = Mathf.Max(x1, 0);
                    y1 = Mathf.Max(y1, 0);
                    x2 = Mathf.Min(x2, MapResolution - 1);
                    y2 = Mathf.Min(y2, MapResolution - 1);
                    float dx = normals[x2, y].x - normals[x1, y].x;
                    float dy = normals[x, y2].z - normals[x, y1].z;
                    Vector2 curvature = new Vector2(dx, dy);
                    if (curvature.magnitude > 1)
                        curvature.Normalize();
                    curvatures[x, y] = curvature;
                }
        }
        private void restoreMap()
        {
            for (int i = 0; i < pointsCount; i++)
                for (int ch = 0; ch < channels.Length; ch++)
                    map[points[i].y, points[i].x, ch] = originalMap[points[i].y, points[i].x, ch];

        }
        private void drawObject(GameObject go)
        {
            
            foreach (Collider collider in go.GetComponentsInChildren<Collider>())
            {
                // terrain rotation = 0;
                float minX = collider.bounds.min.x - terrain.transform.position.x;
                float minZ = collider.bounds.min.z - terrain.transform.position.z;
                float maxX = collider.bounds.max.x - terrain.transform.position.x;
                float maxZ = collider.bounds.max.z - terrain.transform.position.z;

                int x1 = Mathf.FloorToInt(minX * kx) + 1;
                int x2 = Mathf.FloorToInt(maxX * kx);
                int y1 = Mathf.FloorToInt(minZ * kz) + 1;
                int y2 = Mathf.FloorToInt(maxZ * kz);
                for (int x = x1; x <= x2; x++)
                    for (int y = y1; y <= y2; y++)
                    {
                        points[pointsCount].x = x;
                        points[pointsCount].y = y;
                        for (int i = 0; i < channels.Length; i++)
                            map[y, x, i] = 1;
                        pointsCount++;
                    }

            }
            
        }
        private void fillMap()
        {
            map = new float[MapResolution, MapResolution, channels.Length];
            originalMap = new float[MapResolution, MapResolution, channels.Length];

            for (int x = 0; x < MapResolution; x++)
                for (int y = 0; y < MapResolution; y++)
                    for (int i = 0; i < channels.Length; i++)
                    {
                        map[y, x, i] = getValue(channels[i], x, y);
                        originalMap[y, x, i] = map[y, x, i];
                    }
        }
        private float getValue(Channel channel, int x, int y)
        {
            float value = 0;
            switch (channel)
            {
                case Channel.Height:
                    value = heights[y, x] / size.y;
                    break;
                case Channel.NormalX:
                    value = normals[x, y].x;
                    break;
                case Channel.NormalY:
                    value = normals[x, y].z;
                    break;
                case Channel.NormalMagnitude:
                    value = normals[x, y].magnitude;
                    break;
                case Channel.CurvatureX:
                    value = curvatures[x, y].x;
                    break;
                case Channel.CurvatureY:
                    value = curvatures[x, y].y;
                    break;
                case Channel.CurvatureMagnitude:
                    value = curvatures[x, y].magnitude;
                    break;
                case Channel.Objects:
                    value = objectsMap[x, y];
                    break;
            }
            return value;
        }

        public class Point
        {
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public int x;
            public int y;
        }
    }
}