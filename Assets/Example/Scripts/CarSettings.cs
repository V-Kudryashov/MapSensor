using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSettings : MonoBehaviour
{
    public int targetFrameRate = -1;
    public float timeScale = 1;
    public bool resetCarOnTime = false;
    public bool resetTargetOnTime = true;
    public bool updateTerrain = false;
    public float tergetRange = 60;
    public int episodeDuration = 2000;
    public float veloWeight = 1.0f;
    public float steerSmooth = 0.9f;

    [Tooltip("max acceleration m/s2")]
    public float maxAccel = 8;
    [Tooltip("max brakeTorque")]
    public float maxBrake = 50;
    [Tooltip("max velocity m/s")]
    public float maxVelocity = 15;

    private void Start()
    {
        Application.targetFrameRate = targetFrameRate;
        Time.timeScale = timeScale;
    }
}
