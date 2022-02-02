using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSettings : MonoBehaviour
{
    public bool resetCarOnTime;
    public bool resetTargetOnTime;
    public bool updateTerrain;
    public float tergetRange = 60;
    public int episodeDuration = 1000;
    public float veloWeight = 0.6f;
    public float steerSmooth = 0.8f;

    public float maxAccel = 500;
    public float maxBrake = 50;
    public float maxVelocity = 50;
}
