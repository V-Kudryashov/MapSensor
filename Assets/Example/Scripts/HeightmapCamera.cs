using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VK.MapSensor;

public class HeightmapCamera : MapCamera
{
    public int width = 84;
    public int height = 84;
    public Vector2 size = new Vector2(20, 20);
    public Terrain terrain;
    public Transform agent;

    private float[,,] frame;
    private int[] shape = new int[3]; // h,w,ch
    private Texture2D texture;
    private Color[] colors;
    private float[,] H;

    private float posXtoMap;
    private float posZtoMap;
    private float frameToMapX;
    private float frameToMapY;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        if (frame != null)
            return;
        shape = new int[] { height, width, 1 };
        frame = new float[height, width, 1];

        int mapW = shape[1];
        int mapH = shape[0];
        texture = new Texture2D(mapW, mapH);
        colors = new Color[mapW * mapH];

        int res = terrain.terrainData.heightmapResolution;
        Vector3 terrainSize = terrain.terrainData.size;
        posXtoMap = (res - 1) / terrainSize.x;
        posZtoMap = (res - 1) / terrainSize.z;
        frameToMapX = (res - 1) / terrainSize.x / (width / size.x);
        frameToMapY = (res - 1) / terrainSize.z / (height / size.y);

        H = terrain.terrainData.GetHeights(0, 0, res, res);
    }

    public override float[,,] GetFrame()
    {
        Vector3 pos = terrain.transform.InverseTransformPoint(agent.position);
        float cX = pos.x * posXtoMap;
        float cY = pos.z * posZtoMap;

        Vector3 forward = terrain.transform.InverseTransformVector(agent.forward);
        forward.y = 0;
        float sin = forward.normalized.x;
        float cos = forward.normalized.z;

        int d = (int)(height * 1.5f * frameToMapX);
        int mapH = H.GetLength(0);
        int mapW = H.GetLength(1);
        cX = Mathf.Max(cX, d);
        cY = Mathf.Max(cY, d);
        cX = Mathf.Min(cX, mapH - d);
        cY = Mathf.Min(cY, mapW - d);

        int dw = -shape[1] / 2;
        int dh = -shape[0];
        for (int h = 0; h < height; h++) // 0.04ms
            for (int w = 0; w < width; w++)
            {
                int h1 = h + dh;
                int w1 = w + dw;
                float x = cX + (w1 * cos - h1 * sin) * frameToMapX;
                float y = cY - (w1 * sin + h1 * cos) * frameToMapY;

                int x1 = Mathf.FloorToInt(x);
                int y1 = Mathf.FloorToInt(y);
                int x2 = x1 + 1;
                int y2 = y1 + 1;
                x -= x1;
                y -= y1;
                // bilinear interpolation
                float k11 = (1 - x) * (1 - y);
                float k12 = (1 - y) * x;
                float k21 = y * (1 - x);
                float k22 = x * y;

                frame[h, w, 0] = H[y1, x1] * k11 + H[y1, x2] * k12 + H[y2, x1] * k21 + H[y2, x2] * k22;
            }


        return frame;
    }

    public override int[] GetShape()
    {
        return shape;
    }
    public override Texture2D GetTexture()
    {
        return texture;
    }
    public override void UpdateTexture()
    {
        for (int h = width - 1; h >= 0; h--)
            for (int w = 0; w < width; w++)
            {
                float r = frame[h, w, 0];
                colors[(height - h - 1) * width + w] = new Color(r, r, r);
            }
        texture.SetPixels(colors);
        texture.Apply();
    }

}
