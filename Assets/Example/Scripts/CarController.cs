using UnityEngine;
using System.Collections;
using System;
using Unity.Mathematics;

public class CarController : ACarController
{
	public float timeScale = 1;

	public Rigidbody carRB;

	public WheelCollider colliderFL;
	public WheelCollider colliderFR;
	public WheelCollider colliderRL;
	public WheelCollider colliderRR;

	public Transform wheelFL;
	public Transform wheelFR;
	public Transform wheelRL;
	public Transform wheelRR;

	public Transform axleFL;
	public Transform axleFR;
	public Transform axleRL;
	public Transform axleRR;

	public float maxSteer = 30;

	public bool manualControl;

	private CarSettings carSettings;
	private float smoothTargetSteer;

	public Transform cylinder;

	public class WheelData
	{
		public Transform wheelTransform;
		public Transform axle;
		public WheelCollider collider;
		public float normalizedPos;
	}

	private WheelData[] wheels;
	private WheelHit[] wheelHits;

	[HideInInspector] public float smoothSteer;
	[HideInInspector] public float smoothVelo;
	private float targetV;

	void Start()
	{
		Init();
	}
	public override void Init()
	{
		if (wheels != null)
			return;

		Time.timeScale = timeScale;

		wheels = new WheelData[4];
		wheels[0] = SetupWheels(wheelFL, axleFL, colliderFL);
		wheels[1] = SetupWheels(wheelFR, axleFR, colliderFR);
		wheels[2] = SetupWheels(wheelRL, axleRL, colliderRL);
		wheels[3] = SetupWheels(wheelRR, axleRR, colliderRR);

		wheelHits = new WheelHit[4];

		carSettings = carRB.GetComponent<CarAgent>().carSettings;

		smoothTargetSteer = 0;
	}

	private WheelData SetupWheels(Transform wheel, Transform axle, WheelCollider col)
	{
		WheelData data = new WheelData();
		data.wheelTransform = wheel;
		data.axle = axle;
		data.collider = col;
		return data;
	}

	void FixedUpdate()
	{
		if (manualControl)
		{
			float v = Input.GetAxis("Vertical");
			float h = Input.GetAxis("Horizontal");

			//setSteer(h);
			//setVelocity(v);
			
			//setAcc(v);
		}
	}
	private void Update()
	{
		updateVisual();
	}
	private void updateVisual()
	{
		foreach (WheelData w in wheels)
		{
			updateWheelPose();

			Vector3 pos = w.wheelTransform.position;
			pos.y -= w.collider.radius;
		}
	}
	private void updateWheelPose()
	{
		foreach (WheelData w in wheels)
		{
			w.collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
			w.axle.position = pos;
			w.wheelTransform.rotation = rot;

			w.normalizedPos = w.collider.transform.InverseTransformPoint(pos).y;
			w.normalizedPos = w.normalizedPos / w.collider.suspensionDistance + 1;
		}
	}

	public Vector3 getSteerDir()
	{
		float a = (colliderFL.steerAngle + colliderFR.steerAngle) / 2 * Mathf.Deg2Rad;
		Vector3 dir = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
		return dir;
	}
	//***************************************** ICarController
	/// <inheritdoc/>
	public override bool ManualControl()
	{
		return manualControl;
	}
	/// <inheritdoc/>
	public override Transform GetRoot()
    {
		return carRB.transform;
    }
	/// <inheritdoc/>
	public override Rigidbody GetRigidbody()
	{
		return carRB;
	}
	public override WheelCollider[] GetWheelColliders()
    {
		WheelCollider[] colliders = new WheelCollider[4];
		colliders[0] = colliderFL;
		colliders[1] = colliderFR;
		colliders[2] = colliderRL;
		colliders[3] = colliderRR;
		return colliders;
	}
	/// <inheritdoc/>
	public override float GetMaxSteerAngle()
    {
        return maxSteer;
    }
	/// <inheritdoc/>
	public override float GetMaxVelocity()
    {
		return carSettings.maxVelocity;
    }
	/// <inheritdoc/>
	public override void SetSteer(float value)
    {
		colliderFL.steerAngle = value * maxSteer;
		colliderFR.steerAngle = value * maxSteer;
	}
	/// <inheritdoc/>
	public override void SetAcceleration(float value)
    {
		float force = value * carSettings.maxAccel * carRB.mass / 4;
		colliderFL.motorTorque = force * colliderFL.radius * 0.7f;
		colliderFR.motorTorque = force * colliderFR.radius * 0.7f;
		colliderRL.motorTorque = force * colliderRL.radius * 1.3f;
		colliderRR.motorTorque = force * colliderRR.radius * 1.3f;
	}

}