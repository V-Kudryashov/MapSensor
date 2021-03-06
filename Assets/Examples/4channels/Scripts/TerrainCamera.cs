using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using VK.MapSensor;

[AddComponentMenu("ML Agents/Map/Tarran Camera")]
    public class TerrainCamera : MapCamera
    {
        public int width = 84;
        public int height = 84;
        //public Vector2 size = new Vector2(20, 20);
        public TerrainMap terrainMap;
        public bool useGPU;
        //public Transform agent;

        private int ch;
        public float[,,] frame;
        private int[] shape = new int[3]; // h,w,ch
        private float[,,] M;

        private Terrain terrain;
        private float posXtoMap;
        private float posZtoMap;

        static public int kiCam; // kernel index
        static public ComputeBuffer mapBuffer;
        static public ComputeBuffer frameBuffer;
        static public ComputeBuffer transformBuffer;
        static public ComputeBuffer offsetBuffer;
        static public ComputeShader _shader;
        private float2[] arrTrahsform = new float2[2];
        private int[] arrOffset = new int[7];

        void Start()
        {
            Init();
        }

        public override void Init()
        {
            if (frame != null)
                return;
            terrainMap.Init();
            ch = terrainMap.channels.Count;
            shape = new int[] { height, width, ch };
            frame = new float[height, width, ch];

            terrain = terrainMap.GetTarrain();
            int res = terrainMap.MapResolution;
            Vector3 terrainSize = terrain.terrainData.size;
            posXtoMap = (res - 1) / terrainSize.x;
            posZtoMap = (res - 1) / terrainSize.z;

            M = terrainMap.Map;

            if (useGPU)
            {
                mapBuffer = new ComputeBuffer(terrainMap.Map.Length, sizeof(float));
                frameBuffer = new ComputeBuffer(frame.Length, sizeof(float));
                transformBuffer = new ComputeBuffer(arrTrahsform.Length, sizeof(float) * 2);
                offsetBuffer = new ComputeBuffer(arrOffset.Length, sizeof(int));

                mapBuffer.SetData(terrainMap.Map);
                frameBuffer.SetData(frame);
               
                _shader = Resources.Load<ComputeShader>("CsTerrainCamera");    // here we link computer shader code file to the shader class
                kiCam = _shader.FindKernel("terrainCamera");                   // we retrieve kernel index by name from the code
                                                                               // folowwing three lines allocate video memory and write there our data, kernel will then be able to use the data in calculations
                _shader.SetBuffer(kiCam, "map", mapBuffer);
                _shader.SetBuffer(kiCam, "frame", frameBuffer);
                _shader.SetBuffer(kiCam, "transform", transformBuffer);
                _shader.SetBuffer(kiCam, "offset", offsetBuffer);
            }
        }


        public override float[,,] UpdateFrame()
        {
            if (useGPU)
            {
                return updateFrameGPU();
            }
            else
            {
                return updateFrameCPU();
            }
        }
        private float[,,] updateFrameCPU() // 2.76 ms 60x60 1.3 0.22 0.88
        {
            Vector3 pos = terrain.transform.InverseTransformPoint(transform.position);
            float cX = pos.x * posXtoMap;
            float cY = pos.z * posZtoMap;

            Vector3 forward = terrain.transform.InverseTransformVector(transform.forward);
            forward.y = 0;
            float sin = forward.normalized.x;
            float cos = forward.normalized.z;

            M = terrainMap.Map;
            int mapH = M.GetLength(0);
            int mapW = M.GetLength(1);

            int dw = -shape[1] / 2;
            int dh = -shape[0];
            dh -= 2; // Car size
            for (int h = 0; h < height; h++) // 0.04ms
                for (int w = 0; w < width; w++)
                {
                    int h1 = h + dh;
                    int w1 = w + dw;
                    float x = cX + (w1 * cos - h1 * sin); // 0.22
                    float y = cY - (w1 * sin + h1 * cos);

                    int x1 = Mathf.FloorToInt(x);
                    int y1 = Mathf.FloorToInt(y);

                    //transform.TransformPoint(pos); //0.66
                    //for (int i = 0; i < ch; i++) // 1.1 ms 7ch
                      //  frame[h, w, i] = M[y1, x1, i];
                    
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
        private float[,,] updateFrameGPU()
        {
            Vector3 pos = terrain.transform.InverseTransformPoint(transform.position);
            float cX = pos.x * posXtoMap;
            float cY = pos.z * posZtoMap;

            Vector3 forward = terrain.transform.InverseTransformVector(transform.forward);
            forward.y = 0;
            float sin = forward.normalized.x;
            float cos = forward.normalized.z;

            M = terrainMap.Map;
            int mapH = M.GetLength(0);
            int mapW = M.GetLength(1);

            int dw = -shape[1] / 2;
            int dh = -shape[0];

            arrTrahsform[0].x = cX;
            arrTrahsform[0].y = cY;
            arrTrahsform[1].x = sin;
            arrTrahsform[1].y = cos;
            transformBuffer.SetData(arrTrahsform);
            arrOffset[0] = dw;
            arrOffset[1] = dh;
            arrOffset[2] = mapW;
            arrOffset[3] = mapH;
            arrOffset[4] = width;
            arrOffset[5] = height;
            arrOffset[6] = ch;
            offsetBuffer.SetData(arrOffset);

            int threadX = width / 8;
            int threadY = height / 8;
            if (threadX * 8 < width)
                threadX++;
            if (threadY * 8 < height)
                threadY++;


            Profiler.BeginSample("Dispatch");
            _shader.Dispatch(kiCam, threadX, threadY, ch);
            Profiler.EndSample();

            Profiler.BeginSample("GetData");
            frameBuffer.GetData(frame);
            Profiler.EndSample();

            return frame;
        }
        public override float[,,] Frame { get { return frame; } }
        public override int Width { get { return width; } }
        public override int Height { get { return height; } }

        public override int[] GetShape()
        {
            if (shape == null || shape.Length != 3)
                shape = new int[3];
            ch = terrainMap.channels.Count;
            shape[0] = height;
            shape[1] = width;
            shape[2] = ch;
            return shape;
        }

        void OnDestroy()
        {                   // we need to explicitly release the buffers, otherwise Unity will not be satisfied
            if (mapBuffer != null)
                mapBuffer.Release();
            if (frameBuffer != null)
                frameBuffer.Release();
            if (transformBuffer != null)
                transformBuffer.Release();
            if (offsetBuffer != null)
                offsetBuffer.Release();
        }

    }
