using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
        public List<Channel> channels;
        public bool drawObjectsOnMap;
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
        //private TerrainData terrainData;
        private Vector3 size;

        /// <summary>
        /// world to map
        /// </summary>
        private float kx, kz;
        private float intervalX, intervalZ;
        private List<mapObject> mapObjects;
        private Transform mapTr;

        void Start()
        {
            //tags = UnityEditorInternal.InternalEditorUtility.tags;
            Init();
        }
        private void FixedUpdate()
        {
            Profiler.BeginSample("UpdateMap");
            DrawMapObjects();
            Profiler.EndSample();
        }
        public void Init()
        {
            if (map != null)
                return;
            terrain = GetComponent<Terrain>();
            size = terrain.terrainData.size;

            mapTr = new GameObject("mapTr").transform;
            mapTr.parent = terrain.transform;
            mapTr.position = terrain.transform.position;
            mapTr.rotation = terrain.transform.rotation;
            int res = (MapResolution - 1);
            float x = size.x / res / terrain.transform.lossyScale.x;
            float y = 1;
            float z = size.z / res / terrain.transform.lossyScale.z;
            mapTr.localScale = new Vector3(x, y, z);

            fillHeights(); // 13 ms
            fillNormals(); // 287 ms
            fillCurvatures(); // 115 ms
            objectsMap = new float[MapResolution, MapResolution];

            kx = (MapResolution - 1) / size.x;
            kz = (MapResolution - 1) / size.z;
            intervalX = 1f / kx;
            intervalZ = 1f / kz;
            fillMap(); // ch7 718
            fillMapObjects(); // 8 ms
            /*
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            stopWatch.Stop();
            Double ms = stopWatch.Elapsed.TotalMilliseconds;
            */
        }
        public Terrain GetTarrain()
        {
            return GetComponent<Terrain>();
        }
        private void fillHeights()
        {
            float interval = 1f / (MapResolution - 1);
            heights = terrain.terrainData.GetInterpolatedHeights(0, 0, MapResolution, MapResolution, interval, interval);
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
                    normals[x, y] = terrain.terrainData.GetInterpolatedNormal(X, Y);
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
        private void fillMap()
        {
            map = new float[MapResolution, MapResolution, channels.Count];
            originalMap = new float[MapResolution, MapResolution, channels.Count];

            for (int x = 0; x < MapResolution; x++)
                for (int y = 0; y < MapResolution; y++)
                    for (int i = 0; i < channels.Count; i++)
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
        private void fillMapObjects()
        {
            mapObjects = new List<mapObject>();
            if (!drawObjectsOnMap)
                return;
            if (!channels.Contains(objectsChannel))
                return;
            foreach (GameObject go in objects)
                mapObjects.Add(new mapObject(go, this));
            foreach (string tag in tags)
                if (!String.IsNullOrEmpty(tag))
                    foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
                        mapObjects.Add(new mapObject(go, this));
        }
        private void DrawMapObjects()
        {
            if (!drawObjectsOnMap)
                return;
            if (!channels.Contains(objectsChannel))
                return;
            foreach (mapObject mapObject in mapObjects)
            {
                mapObject.Draw();
            }
        }
        
        public class mapObject
        {
            public GameObject Object;
            private TerrainMap map;
            private List<ColliderBounds> prevBounds;
            private Collider[] colliders;
            public mapObject(GameObject Object, TerrainMap map)
            {
                this.Object = Object;
                this.map = map;
                
                colliders = Object.GetComponentsInChildren<Collider>();
                prevBounds = new List<ColliderBounds>();
                foreach (Collider collider in colliders)
                    prevBounds.Add(new ColliderBounds(collider));
            }
            public void Draw()
            {
                Profiler.BeginSample("dyamicDraw"); // 0.31 ms
                Profiler.BeginSample("restorePoints");// 0.04 - 0.08 ms
                restorePoints();
                Profiler.EndSample();
                saveBounds();
                int channelIndex = map.channels.IndexOf(map.objectsChannel);
                Matrix4x4 localToWorldMatrix = map.mapTr.localToWorldMatrix;
                Vector3 pos = Vector3.zero;
                Ray ray = new Ray(Object.transform.position, Vector3.down);
                foreach (Collider collider in colliders)
                {
                    Vector3 min = map.mapTr.InverseTransformPoint(collider.bounds.min);
                    Vector3 max = map.mapTr.InverseTransformPoint(collider.bounds.max);
                    getRange(min, max, out int x1, out int x2, out int y1, out int y2);
                    for (int x = x1; x <= x2; x++)
                        for (int y = y1; y <= y2; y++)
                        {
                            pos.x = x;
                            pos.z = y;
                            Vector3 world = localToWorldMatrix.MultiplyPoint(pos); // -0.07ms
                            world.y = collider.bounds.max.y + 1;
                            ray.origin = world;
                            if (collider.Raycast(ray, out RaycastHit hit, 10)) // 0.15
                                map.map[y, x, channelIndex] = 1;
                        }
                }
                Profiler.EndSample();
            }
            private void saveBounds()
            {
                foreach (ColliderBounds b in prevBounds)
                    b.save();
            }
            private void restorePoints()
            {
                int channelIndex = map.channels.IndexOf(map.objectsChannel);

                foreach (ColliderBounds b in prevBounds)
                {
                    Vector3 min = map.mapTr.InverseTransformPoint(b.min);
                    Vector3 max = map.mapTr.InverseTransformPoint(b.max);
                    getRange(min, max, out int x1, out int x2, out int y1, out int y2);
                    for (int x = x1; x <= x2; x++)
                        for (int y = y1; y <= y2; y++)
                            map.map[y, x, channelIndex] = map.originalMap[y, x, channelIndex];
                }
            }
            private void getRange(Vector3 min, Vector3 max, out int x1, out int x2, out int y1, out int y2)
            {
                x1 = Mathf.FloorToInt(min.x);
                x2 = Mathf.FloorToInt(max.x) + 1;
                y1 = Mathf.FloorToInt(min.z);
                y2 = Mathf.FloorToInt(max.z) + 1;
                x1 = Mathf.Max(x1, 0);
                y1 = Mathf.Max(y1, 0);
                x2 = Mathf.Min(x2, map.map.GetLength(1) - 1);
                y2 = Mathf.Min(y2, map.map.GetLength(0) - 1);
            }
        }
        public class ColliderBounds
        {
            private Collider collider;
            public Vector3 min;
            public Vector3 max;
            public ColliderBounds(Collider collider)
            {
                this.collider = collider;
                save();
            }
            public void save()
            {
                min = collider.bounds.min;
                max = collider.bounds.max;
            }
        }
    }
}