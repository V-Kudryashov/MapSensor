using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VK.MapSensor
{
    [AddComponentMenu("ML Agents/Map/Tarran Camera")]
    public class TerrainCamera : MapCamera
    {
        public int width = 84;
        public int height = 84;
        //public Vector2 size = new Vector2(20, 20);
        public TerrainMap terrainMap;
        //public Transform agent;

        private int ch;
        private float[,,] frame;
        private int[] shape = new int[3]; // h,w,ch
        private float[,,] M;

        private Terrain terrain;
        private float posXtoMap;
        private float posZtoMap;

        void Start()
        {
            Init();
        }

        public override void Init()
        {
            if (frame != null)
                return;
            terrainMap.Init();
            ch = terrainMap.channels.Length;
            shape = new int[] { height, width, ch };
            frame = new float[height, width, ch];

            terrain = terrainMap.GetTarrain();
            int res = terrainMap.MapResolution;
            Vector3 terrainSize = terrain.terrainData.size;
            posXtoMap = (res - 1) / terrainSize.x;
            posZtoMap = (res - 1) / terrainSize.z;

            M = terrainMap.Map;
        }

        public override float[,,] UpdateFrame()
        {
            Vector3 pos = terrain.transform.InverseTransformPoint(transform.position);
            float cX = pos.x * posXtoMap;
            float cY = pos.z * posZtoMap;

            Vector3 forward = terrain.transform.InverseTransformVector(transform.forward);
            forward.y = 0;
            float sin = forward.normalized.x;
            float cos = forward.normalized.z;

            int mapH = M.GetLength(0);
            int mapW = M.GetLength(1);

            int dw = -shape[1] / 2;
            int dh = -shape[0];
            for (int h = 0; h < height; h++) // 0.04ms
                for (int w = 0; w < width; w++)
                {
                    int h1 = h + dh;
                    int w1 = w + dw;
                    float x = cX + (w1 * cos - h1 * sin);
                    float y = cY - (w1 * sin + h1 * cos);

                    int x1 = Mathf.FloorToInt(x);
                    int y1 = Mathf.FloorToInt(y);
                    int x2 = x1 + 1;
                    int y2 = y1 + 1;
                    if (x1 < 0 || y1 < 0 || x2 >= mapW || y2 >= mapH)
                    {
                        for (int i = 0; i < ch; i++)
                            frame[h, w, i] = 1;
                        continue;
                    }
                    x -= x1;
                    y -= y1;
                    // bilinear interpolation
                    float k11 = (1 - x) * (1 - y);
                    float k12 = (1 - y) * x;
                    float k21 = y * (1 - x);
                    float k22 = x * y;
                    for (int i = 0; i < ch; i++)
                        frame[h, w, i] = M[y1, x1, i] * k11 + M[y1, x2, i] * k12 + M[y2, x1, i] * k21 + M[y2, x2, i] * k22;
                }


            return frame;
        }
        public override float[,,] Frame { get { return frame; } }
        public override int Width { get { return width; } }
        public override int Height { get { return height; } }

        public override int[] GetShape()
        {
            if (shape == null || shape.Length != 3)
                shape = new int[3];
            ch = terrainMap.channels.Length;
            shape[0] = height;
            shape[1] = width;
            shape[2] = ch;
            return shape;
        }
    }
}