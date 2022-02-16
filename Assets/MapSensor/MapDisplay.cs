using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using VK.MapSensor;

namespace VK.MapSensor
{
    public class MapDisplay : MonoBehaviour
    {
        public MapSensorComponent mapSensor;
        public RawImage rawImage;
        [HideInInspector][SerializeField] public int red;
        [HideInInspector][SerializeField] public int green;
        [HideInInspector][SerializeField] public int blue;
        [HideInInspector][SerializeField] public bool useRed = true;
        [HideInInspector][SerializeField] public bool useGreen = true;
        [HideInInspector][SerializeField] public bool useBlue = true;

        [HideInInspector]public MapCamera mapCamera;
        private int width, height;
        private Texture2D texture2d;
        private Color[] colors;
        private RenderTexture renderTexture;
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        void Start()
        {
            updateUI();
            texture2d = new Texture2D(width, height);
            colors = new Color[width * height];

            renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.Create();
            rawImage.texture = renderTexture;
            rawImage.enabled = true;
        }
        public void updateUI()
        {
            mapCamera = mapSensor.MapCamera;
            mapCamera.Init();
            width = mapCamera.Width;
            height = mapCamera.Height;
        }
        void Update()
        {
            //mapCamera.UpdateFrame();

            updateTexture2d(); // 0.12ms
            updateRenderTexture();
        }
        private void updateTexture2d()
        {
            float r, g, b;
            float[,,] frame = mapCamera.Frame;
            for (int h = height - 1; h >= 0; h--)
                for (int w = 0; w < width; w++)
                {
                    if (useRed)
                    {
                        r = frame[h, w, red];
                        if (red == 1 || red == 2 || red == 4 || red == 5)
                            r = r / 2 + 0.5f;
                    }
                    else
                        r = 0;
                    if (useGreen)
                    {
                        g = frame[h, w, green];
                        if (green == 1 || green == 2 || green == 4 || green == 5)
                            g = g / 2 + 0.5f;
                    }
                    else
                        g = 0;
                    if (useBlue)
                    {
                        b = frame[h, w, blue];
                        if (blue == 1 || blue == 2 || blue == 4 || blue == 5)
                            b = b / 2 + 0.5f;
                    }
                    else
                        b = 0;


                    colors[(height - h - 1) * width + w] = new Color(r, g, b);
                }
            texture2d.SetPixels(colors);
            texture2d.Apply();

        }
        private void updateRenderTexture()
        {
            //Profiler.BeginSample("updateRenderTexture");
            RenderTexture active = RenderTexture.active;
            Graphics.Blit(texture2d, renderTexture);
            RenderTexture.active = active;
            //Profiler.EndSample();
        }
    }
}