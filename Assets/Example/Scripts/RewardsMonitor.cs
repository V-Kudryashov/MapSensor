using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rewards { Dir, Dist, Velo, Steer, Peak, Target, FailPeak, FailSteer, FailStop, FailOut };
public class RewardsMonitor : MonoBehaviour
{
    public float smooth = 0.999f;
    public float episodeLength;
    public int episodeN;
    public int steps;
    public float meanReward;
    public float maxReward = 40;
    public Dictionary<Rewards, RewardItem> rewardsDic;

    void Start()
    {
        init();  }
    public void init()
    {
        if (rewardsDic != null)
            return;
        rewardsDic = new Dictionary<Rewards, RewardItem>();
        Rewards[] arr = (Rewards[])Enum.GetValues(typeof(Rewards));
        foreach (Rewards r in arr)
            rewardsDic.Add(r, new RewardItem(r, true, 0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void addReward(Rewards id, float value)
    {
        rewardsDic[id].value = rewardsDic[id].value * smooth + value * (1 - smooth);
    }
    public void addSteps(int episodeLength, float totalReward)
    {
        this.episodeLength = this.episodeLength * smooth + episodeLength * (1 - smooth);
        steps += episodeLength;
        episodeN++;
        meanReward = meanReward * 0.995f + totalReward * 0.005f;

#if UNITY_EDITOR
        if (meanReward > maxReward)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
    }
    public class RewardItem
    {
        public RewardItem (Rewards id, bool active, float value)
        {
            this.id = id;
            this.active = active;
            this.value = value;
        }
        public Rewards id;
        public bool active;
        public float value;
    }
}
