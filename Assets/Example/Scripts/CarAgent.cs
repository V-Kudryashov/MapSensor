using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using VK.MapSensor;

public class CarAgent : Agent
{
    public ACarController carController;
    public GameObject target;
    public Transform ground;
    public Terrain terrain;
    public CarSettings carSettings;
    public EarlyTermination earlyTermination;
    public TerrainDataGenerator generator;
    public RewardsMonitor rewardsMonitor;
    public TerrainCamera terrainCamera;
    public float velocityKM;
    public float smoothVelo;

    private Dictionary<Rewards, float> stepRewards;
    private Dictionary<Rewards, float> episodeRewards;

    private CarHelper car;
    private GameObject carGO;
    private Transform carTr;
    private Rigidbody carRB;
    private bool carResetRequare = true;
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 targetOriginalPos;
    public int maxStep;
    private WheelHit[] wheelHits;
    private Trails trails;

    private bool needEnd = false;
    private bool needTargetReset = false;
    public int step;
    private StatsRecorder statsRecorder;
    EnvironmentParameters parameters;

    public override void Initialize()
    {
        Academy.Instance.AgentPreStep += MakeRequests;
        statsRecorder = Academy.Instance.StatsRecorder;
        parameters = Academy.Instance.EnvironmentParameters;

        carController.Init();
        car = new CarHelper(carController);
        trails = GetComponent<Trails>();
        if (trails != null)
            trails.Init();

        carGO = carController.gameObject;
        carTr = carGO.transform;
        carRB = car.rb;

        stepRewards = new Dictionary<Rewards, float>();
        episodeRewards = new Dictionary<Rewards, float>();
        Rewards[] arr = (Rewards[])Enum.GetValues(typeof(Rewards));
        step = 1;
        foreach (Rewards r in arr)
        {
            stepRewards.Add(r, 0);
            episodeRewards.Add(r, 0);
        }

        originalPos = carTr.position;
        originalRot = carTr.rotation;
        targetOriginalPos = target.transform.localPosition;

        needTargetReset = true;
    }
    void MakeRequests(int academyStepCount)
    {
        RequestDecision();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dir;
        Vector3 toTarget = target.transform.position - carTr.position;

        dir = carTr.InverseTransformVector(toTarget);
        dir.y = 0;
        dir = dir.normalized;

        float dist = toTarget.magnitude;
        float veloZ = carTr.InverseTransformVector(carRB.velocity / 14 / 1.5f).z;
        Vector3 steerDir = car.getSteerDir();
        Vector3 up = carTr.InverseTransformVector(Vector3.up);

        Vector3 normal = getNormal(ground.InverseTransformPoint(carTr.position));
        normal = ground.TransformVector(normal);
        normal.y = 0;
        Vector3 forward = carTr.forward;
        forward.y = 0;
        forward = forward.normalized;
        Quaternion saveRot = carTr.rotation;
        carTr.rotation = Quaternion.LookRotation(forward);
        normal = carTr.InverseTransformVector(normal);
        carTr.rotation = saveRot;

        float[] wheelPos = car.getWheelPos();
        wheelHits = car.GetWheelHits();

        addObs(dir.x, sensor);
        addObs(dir.z, sensor);
        addObs(dist / 1000, sensor);

        addObs(veloZ, sensor);
        addObs(steerDir.x, sensor);
        addObs(steerDir.z, sensor);
        addObs(up.x, sensor);
        addObs(up.z, sensor);
        addObs(normal.x, sensor);
        addObs(normal.z, sensor);
        for (int i = 0; i < 4; i++)
        {
            addObs(wheelPos[i], sensor);

            addObs(Mathf.Clamp(wheelHits[i].forwardSlip, -1, 1), sensor);
            addObs(Mathf.Clamp(wheelHits[i].sidewaysSlip, -1, 1), sensor);
        }
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        step = StepCount;
        float steer = actionBuffers.ContinuousActions[0];
        steer *= 2;
        steer = Mathf.Clamp(steer, -1, 1);
        float velo = actionBuffers.ContinuousActions[1];
        velo = velo * carSettings.veloWeight + (1 - carSettings.veloWeight);
        if (!car.ManualControl())
        {
            car.setSteer(steer, carSettings.steerSmooth); //0.8
            car.setVelocity(velo);
            smoothVelo = car.smoothVelo;
        }

        Vector3 dir = (target.transform.position - carTr.position).normalized;
        float dist2 = (target.transform.position - carTr.position).sqrMagnitude;
        float veloZ = carTr.InverseTransformVector(carRB.velocity / carSettings.maxVelocity).z;
        float targetSteer = car.targetSteer(target.transform.position) * Mathf.Sign(veloZ);
        float currentSteer = car.getSteer();
        float steerError = Mathf.Abs(targetSteer - currentSteer);
        float steerReward = Mathf.Exp(-steerError);
        float peakReward = 0;

        stepRewards[Rewards.Dir] = 0.01f * (Vector3.Dot(dir, carTr.forward) + 1);
        stepRewards[Rewards.Dist] = 0.01f * Mathf.Exp(-dist2 / 1000);
        stepRewards[Rewards.Velo] = 0.03f * (veloZ + 1);
        stepRewards[Rewards.Steer] = 0.01f * steerReward;
        stepRewards[Rewards.Peak] = 0.01f * peakReward;

        Vector3 pos = ground.InverseTransformPoint(carTr.position);
        Vector3 size = terrain.terrainData.size;
        if (pos.x < 0 || pos.z < 0 || pos.x > size.x || pos.z > size.z)
            carFail(Rewards.FailOut, -4);

        if (StepCount > 20)
        {
            float veloError = 1 - actionBuffers.ContinuousActions[1];
            if (StepCount > 100)
                veloError = 0;

            if (earlyTermination.terminate(StepCount, steerError, veloError))
                carFail(Rewards.FailPeak, -4);
        }
        float newV = carRB.velocity.magnitude * 3.6f;
        velocityKM = velocityKM * 0.95f + newV * 0.05f;
        if (velocityKM < 1.0f)
            carFail(Rewards.FailStop, -4);

        if (StepCount > maxStep)
        {
            maxStep = carSettings.episodeDuration;
            needEnd = true;
            if (carSettings.resetCarOnTime)
                carReset();
            if (carSettings.resetTargetOnTime)
                randomTarget();
        }

        addRewards();
        if (needEnd)
        {
            EndEpisode();
            needEnd = false;
        }
        if (needTargetReset)
        {
            randomTarget();
            needTargetReset = false;
        }
    }
    private void addRewards()
    {
        float total = 0;
        Rewards[] arr = (Rewards[])Enum.GetValues(typeof(Rewards));
        foreach (Rewards r in arr)
        {
            if (!rewardsMonitor.rewardsDic[r].active)
                stepRewards[r] = 0;
            episodeRewards[r] += stepRewards[r];
            AddReward(stepRewards[r]);
            if (!r.ToString().StartsWith("Fail"))
                total += stepRewards[r];
            stepRewards[r] = 0;
        }
    }
    private void addObs(float value, VectorSensor sensor)
    {
        if (Mathf.Abs(value) > 1)
        {
            if (Mathf.Abs(value) > 1.2f)
                Debug.Log("value=" + value);
            value = value / Mathf.Abs(value);
        }
        sensor.AddObservation(value);
    }
    public override void OnEpisodeBegin()
    {
        if (carResetRequare)
            carReset();
        carResetRequare = false;

        float total = 0;
        Rewards[] arr = (Rewards[])Enum.GetValues(typeof(Rewards));
        foreach (Rewards r in arr)
        {
            float reward = episodeRewards[r];
            total += reward;
            rewardsMonitor.addReward(r, reward);
            if (r.ToString().StartsWith("Fail"))
                statsRecorder.Add("FailRewards/" + r.ToString(), reward);
            else
            {
                if (r == Rewards.Target)
                    statsRecorder.Add("Rewards/" + r.ToString(), reward);
                else
                    statsRecorder.Add("Rewards/" + r.ToString(), reward / step);
            }
            episodeRewards[r] = 0;
        }
        //statsRecorder.Add("EnvParams/" + "HillsCount", generator.heightsParams.hillsCount);
        //statsRecorder.Add("EnvParams/" + "PeaksCount", generator.heightsParams.peaksCount);
        //statsRecorder.Add("EnvParams/" + "TreesCount", generator.heightsParams.treesCount);
        //statsRecorder.Add("EnvParams/" + "maxVelocity", carParams.maxVelocity);
        //statsRecorder.Add("EnvParams/" + "smoothT", rewardsMonitor.veloControl.smoothT);
        statsRecorder.Add("EnvParams/" + "maxError", earlyTermination.maxError);
        statsRecorder.Add("EnvParams/" + "duration", earlyTermination.duration);
        statsRecorder.Add("EnvParams/" + "frame[0, 0, 0]", terrainCamera.frame[0, 0, 0]);
        rewardsMonitor.addSteps(step, total);

        if (trails != null)
            trails.startAllTrails();
    }

    public void targetContact()
    {
        //maxStep += carParams.episodeDuration;
        stepRewards[Rewards.Target] = 20;
        needTargetReset = true;
    }
    public void terrainContact(bool reset, float reward)
    {
        if (reset)
            carFail(Rewards.FailPeak, reward);
        else
            stepRewards[Rewards.FailPeak] = reward;
    }

    private void carFail(Rewards id, float reward)
    {
        stepRewards[id] = reward;
        needEnd = true;
        carReset();
        randomTarget();
    }
    private void carReset()
    {
        if (car.ManualControl())
            return;
        if (trails != null)
            trails.stopAllTrails();
        if (carSettings.updateTerrain)
            generateTerrain();
        maxStep = carSettings.episodeDuration;
        Vector3 pos = originalPos;
        pos.y = getHeight(originalPos) + terrain.transform.position.y + 1.0f;
        carTr.position = pos;
        carTr.rotation = originalRot;
        carRB.velocity = Vector3.zero;
        carRB.angularVelocity = Vector3.zero;
        velocityKM = 10;

        float targetSteer = car.targetSteer(target.transform.position);
        car.setSteer(targetSteer);

        carResetRequare = false;
    }
    private void generateTerrain()
    {
        float rnd = UnityEngine.Random.Range(0f, 1f);
        if (rnd > 0.1f)
            return;
        //Debug.Log(terrain.name + " rnd = " + rnd);
        generator.terrain = terrain;
        generator.generateData();
    }
    private void randomTarget()
    {
        if (car.ManualControl())
            return;
        if (target.tag == "Car")
            return;
        float r = carSettings.tergetRange;
        Vector3 c = targetOriginalPos;
        Vector3 pos = target.transform.localPosition;
        float x = UnityEngine.Random.Range(c.x - r, c.x + r);
        float z = UnityEngine.Random.Range(c.z - r, c.z + r);
        pos.x = x;
        pos.z = z;
        pos.y = getHeightLocal(pos) + 1.0f;
        target.transform.localPosition = pos;
    }

    public Vector3 getNormal(Vector3 pos)
    {
        Vector3 size = terrain.terrainData.size;
        float x = pos.x / size.x;
        float y = pos.z / size.z;
        Vector3 normal = terrain.terrainData.GetInterpolatedNormal(x, y);
        return normal;
    }
    public float getHeight(Vector3 pos)
    {
        pos = terrain.transform.InverseTransformPoint(pos);
        return getHeightLocal(pos);
    }
    public float getHeightLocal(Vector3 pos)
    {
        Vector3 size = terrain.terrainData.size;
        float x = pos.x / size.x;
        float y = pos.z / size.z;
        float height = terrain.terrainData.GetInterpolatedHeight(x, y);
        return height;
    }
}
public class CarHelper
    {
        private ACarController carController;

        private WheelCollider colliderFL;
        private WheelCollider colliderFR;
        private WheelCollider colliderRL;
        private WheelCollider colliderRR;

        private float maxSteer;
        public float smoothSteer;
        public float smoothVelo;
        public Rigidbody rb;
        private float targetV;

        public CarHelper(ACarController carController)
        {
            carController.Init();
            this.carController = carController;

            WheelCollider[] colliders = carController.GetWheelColliders();
            colliderFL = colliders[0];
            colliderFR = colliders[1];
            colliderRL = colliders[2];
            colliderRR = colliders[3];

            maxSteer = carController.GetMaxSteerAngle();
            rb = carController.GetRigidbody();
        }
        public  bool ManualControl()
        {
            return carController.ManualControl();
        }
        /// <summary>
        ///  [-1, 1]
        /// </summary>
        /// <param name="steer"></param>
        /// <param name="smooth"></param>
        public void setSteer(float steer, float smooth = 0)
        {
            smoothSteer = smoothSteer * smooth + steer * (1 - smooth);
            carController.SetSteer(smoothSteer);
        }
    /// <summary>
    ///  [-1, 1]
    /// </summary>
    /// <param name="velo"></param>
    public void setVelocity(float velo)
    {
        smoothVelo = smoothVelo * 0.9f + velo * 0.1f;

        targetV = smoothVelo * carController.GetMaxVelocity();
        Vector3 velocity = carController.GetRigidbody().velocity;
        float zVelo = carController.GetRoot().InverseTransformVector(velocity).z;
        float diffVelo = targetV - zVelo;
        if (Mathf.Abs(diffVelo) < 0.1f)
            diffVelo = 0;

        float diffNorm = 1 / carController.GetMaxVelocity();
        if (Mathf.Abs(smoothVelo) > 0.0000001f)
            diffNorm /= Mathf.Abs(smoothVelo);
        diffVelo *= diffNorm;

        diffVelo = Mathf.Clamp(diffVelo, -1, 1);
        //CarAgent.LogValue(diffVelo);
        carController.SetAcceleration(diffVelo);
    }
        /// <summary>
        /// [-1, 1]
        /// </summary>
        /// <param name="acc"></param>
        public void setAcc(float acc)
        {
            carController.SetAcceleration(acc);
        }
        public float targetSteer(Vector3 targetPosition)
        {
            Transform root = carController.GetRoot();
            Vector3 toTarget = targetPosition - root.position;
            toTarget.y = 0;
            Vector3 dir = root.InverseTransformVector(toTarget).normalized;
            dir.y = 0;

            float a = Vector3.Angle(Vector3.forward, dir) * Mathf.Sign(dir.x);
            float tSteer = Mathf.Clamp(a / maxSteer, -1, 1);
            return tSteer;
        }
        public float peakSteer(Vector3 peakPosition)
        {
            Transform root = carController.GetRoot();
            Vector3 toTarget = peakPosition - root.position;
            toTarget.y = 0;
            Vector3 dir = root.InverseTransformVector(toTarget).normalized;
            dir.y = 0;

            float a = Vector3.Angle(Vector3.forward, dir) * Mathf.Sign(dir.x);
            return a / maxSteer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>[-1, 0]</returns>
        public float getSteer()
        {
            float steer = (colliderFL.steerAngle + colliderFR.steerAngle) / 2 / maxSteer;
            return steer;
        }
        public Vector3 getSteerDir()
        {
            float a = (colliderFL.steerAngle + colliderFR.steerAngle) / 2 * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a));
            return dir;
        }
        public float[] getWheelPos()
        {
            float[] pos = new float[4];
            pos[0] = getWheelPos(colliderFL);
            pos[1] = getWheelPos(colliderFR);
            pos[2] = getWheelPos(colliderRL);
            pos[3] = getWheelPos(colliderRR);
            return pos;
        }
        private float getWheelPos(WheelCollider collider)
        {
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);

            float normalizedPos = collider.transform.InverseTransformPoint(pos).y;
            normalizedPos = normalizedPos / collider.suspensionDistance + 1;

            return normalizedPos;
        }
        public WheelHit[] GetWheelHits()
        {
            WheelHit[] wheelHits = new WheelHit[4];
            wheelHits[0] = GetWheelHit(colliderFL);
            wheelHits[1] = GetWheelHit(colliderFR);
            wheelHits[2] = GetWheelHit(colliderRL);
            wheelHits[3] = GetWheelHit(colliderRR);
            return wheelHits;
        }
        private WheelHit GetWheelHit(WheelCollider collider)
        {
            collider.GetGroundHit(out WheelHit hit);
            return hit;
        }
    }

