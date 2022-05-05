using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VK.MapSensor;

public class HeightmapDisplay : MonoBehaviour
{
    public MapSensorComponent mapSensor;
    public RawImage rawImage;

    private HeightmapCamera mapCamera;
    private int width, height;
    private Texture2D texture2d;
    private Color[] colors;
    private RenderTexture renderTexture;
    void Start()
    {
        mapCamera = (HeightmapCamera)mapSensor.MapCamera;
        mapCamera.Init();
        width = mapCamera.Width;
        height = mapCamera.Height;

        texture2d = new Texture2D(width, height);
        colors = new Color[width * height];

        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
        rawImage.texture = renderTexture;
        rawImage.enabled = true;
    }

    void Update()
    {
        updateTexture2d();
        updateRenderTexture();
    }
    private void updateTexture2d()
    {
        for (int h = height - 1; h >= 0; h--)
            for (int w = 0; w < width; w++)
            {
                float v = mapCamera.Frame[h, w, 0];
                colors[(height - h - 1) * width + w] = new Color(v, v, v);
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
