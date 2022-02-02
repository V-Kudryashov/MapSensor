using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarlyTermination : MonoBehaviour
{
    public bool active = true;
    public float maxError = 0.8f;
    public float duration = 0;
    public bool terminate(int step, float steerError, float veloError)
    {
        if (!active)
            return false;

        float s2 = steerError * steerError + veloError * veloError * 0.5f;
        float error = 1 - Mathf.Exp(-1.0f * s2);

        if (error > maxError)
        {
            setDuration(step);
            return true;
        }
        else
            return false;
    }
    private void setDuration(int step)
    {
        float d = step;
        float k = d / 1000000;
        k = Mathf.Clamp(k, 0, 1);
        duration = duration * (1 - k) + d * k;
        if (duration > 150 && maxError > 0.15f)
        {
            maxError *= 0.6f;
            duration = 0;
        }
        if (maxError < 0.15f)
        {
            active = false;
        }
    }
}
