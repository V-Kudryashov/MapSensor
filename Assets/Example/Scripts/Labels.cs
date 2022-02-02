using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Labels : MonoBehaviour
{
    public Text textFPS;
    public CarAgent carAgent;

    private float time;
    public int fps;
    public int fpsF;
    void Start()
    {
        time = Time.realtimeSinceStartup;
        fps = 0;
    }

    void Update()
    {
        fps++;
        if (Time.realtimeSinceStartup - time >= 1)
        {
            textFPS.text = "FPS=" + fps + " " + fpsF + " V=" + carAgent.velocityKM.ToString("00");
            time = Time.realtimeSinceStartup;
            fps = 0;
            fpsF = 0;
        }

    }
    private void FixedUpdate()
    {
        fpsF++;
    }
}
