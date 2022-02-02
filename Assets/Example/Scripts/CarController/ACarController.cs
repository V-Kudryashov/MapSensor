using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ACarController : MonoBehaviour
{
    /// <summary>
    /// Can be called multiple times.
    /// </summary>
    public abstract void Init();
    /// <summary>
    /// 
    /// </summary>
    /// <returns>false for NN control</returns>
    public abstract bool ManualControl();
    public abstract Transform GetRoot();
    public abstract Rigidbody GetRigidbody();
    /// <summary>
    /// FL, FR, RL, RR
    /// </summary>
    /// <returns></returns>
    public abstract WheelCollider[] GetWheelColliders();
    public abstract float GetMaxSteerAngle();
    /// <summary>
    /// m/s
    /// </summary>
    /// <returns></returns>
    public abstract float GetMaxVelocity();
    /// <summary>
    /// [-1, 1]
    /// </summary>
    /// <param name="value"></param>
    public abstract void SetSteer(float value);
    /// <summary>
    /// [-1, 1]
    /// </summary>
    /// <param name="value"></param>
    public abstract void SetAcceleration(float value);
}
