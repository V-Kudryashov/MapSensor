using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using VK.MapSensor;

public class TerrainMapDisplay : MonoBehaviour
{
    public MapSensorComponent mapSensor;
    public RawImage rawImage;

    [HideInInspector] public CameraChannel[] channels;

    [HideInInspector] public TerrainCamera mapCamera;
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
        if (mapSensor == null)
            return;
        mapCamera = (TerrainCamera)mapSensor.MapCamera;
        mapCamera.Init();
        width = mapCamera.Width;
        height = mapCamera.Height;

        if (channels == null || channels.Length != mapCamera.GetShape()[2])
        {
            channels = new CameraChannel[mapCamera.GetShape()[2]];
            for (int i = 0; i < channels.Length; i++)
            {
                channels[i] = new CameraChannel(mapCamera.terrainMap.channels[i]);
            }
        }
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
                r = 0;
                g = 0;
                b = 0;
                for (int i = 0; i < channels.Length; i++)
                {
                    if (channels[i].r)
                        r += channels[i].normalize(frame[h, w, i]);
                    if (channels[i].g)
                        g += channels[i].normalize(frame[h, w, i]);
                    if (channels[i].b)
                        b += channels[i].normalize(frame[h, w, i]);
                }

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

    [Serializable]
    public class CameraChannel
    {
        public Channel channel;
        public bool r;
        public bool g;
        public bool b;
        public bool positive;
        public CameraChannel(Channel channel)
        {
            this.channel = channel;
            positive = (
                channel == Channel.Height ||
                channel == Channel.NormalMagnitude ||
                channel == Channel.CurvatureMagnitude ||
                channel == Channel.Objects);
            r = true;
            g = true;
            b = true;
        }
        public float normalize(float value)
        {
            if (positive)
                return value;
            else
                return value / 2 + 0.5f;
        }
    }
}
