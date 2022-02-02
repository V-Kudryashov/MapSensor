using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trails : MonoBehaviour
{
    public Transform[] wTrails;
    public ACarController carController;

    private WheelData[] wheels;
    private bool stopTrails;
    void Start()
    {
        Init();
    }
    void Update()
    {
        if (!stopTrails)
            foreach (WheelData w in wheels)
                w.updateTrails();
    }
    public void Init()
    {
        if (wheels != null)
            return;
        carController.Init();

        WheelCollider[] colliders = carController.GetWheelColliders();
        wheels = new WheelData[4];
        for (int i = 0; i < 4; i++)
            wheels[i] = new WheelData(colliders[i], wTrails[i]);
    }
    public void stopAllTrails()
    {
        stopTrails = true;
        foreach (WheelData w in wheels)
            w.stopTrails();
    }
    public void startAllTrails()
    {
        stopTrails = false;
    }


    public class WheelData
    {
        public WheelCollider collider;
        public Transform trails;
        public TrailRenderer trail;
        public TrailRenderer trailSlip;
        public WheelData(WheelCollider collider, Transform trails)
        {
            this.collider = collider;
            this.trails = trails;
            TrailRenderer[] trailRenderers = trails.GetComponentsInChildren<TrailRenderer>();
            trail = trailRenderers[0];
            trailSlip = trailRenderers[1];
        }
        public void updateTrails()
        {
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            pos.y -= collider.radius;
            trails.position = pos;

            bool tach = collider.GetGroundHit(out WheelHit hit);
            if (tach)
            {
                if (Mathf.Abs(hit.forwardSlip) > 0.5f || Mathf.Abs(hit.sidewaysSlip) > 0.2f)
                {
                    trailSlip.emitting = true;
                    trail.emitting = false;
                }
                else
                {
                    trailSlip.emitting = false;
                    trail.emitting = true;
                }
            }
            else
            {
                trail.emitting = false;
                trailSlip.emitting = false;
            }
        }
        public void stopTrails()
        {
            trail.emitting = false;
            trailSlip.emitting = false;
        }
    }
}
