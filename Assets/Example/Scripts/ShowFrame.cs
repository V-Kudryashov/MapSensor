using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VK.MapSensor;

public class ShowFrame : MonoBehaviour
{
    public Camera ortCamera;
    public MapCamera mapCamera;
    public Material heightMapMaterial;
    public GameObject plane;

    void Start()
    {
        mapCamera.Init();
        Texture2D texture = mapCamera.GetTexture();
        texture.filterMode = FilterMode.Point;

        MeshRenderer meshRenderer = plane.GetComponentInChildren<MeshRenderer>();
        //Material material = new Material(heightMapMaterial);
        Material material = meshRenderer.sharedMaterial;
        material.SetTexture("_MainTex", texture);
        material.SetTexture("_EmissionMap", texture);
        meshRenderer.sharedMaterial = material;
    }

    void Update()
    {
        mapCamera.GetFrame();
        mapCamera.UpdateTexture();
    }
}
