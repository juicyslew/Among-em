using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SeekerSettings : ScriptableObject
{
    public float walkacc;
    public float searchFieldOfView;
    public float searchDist;
    public float avoidFieldOfView;
    public Vector2 targetingStrengthRange;
    public float targetingEbbFlowSpeed;
    public Vector2 targetingInAvoidanceStrengthRange;
    public float targetingInAvoidanceEbbFlowSpeed;
    public float intentSteeringStrength;
    public float targetingInAvoidanceWeight;
    public float targetPingRandomOffset;
    public float targetPingGrowthRate;
    public float chaseStrength;
    public float targetInsidePingStrengthMult;
    public float searchSpeedMult;

    public float susCapacity;
    public float susResetInterval;
    public float susPercentage;

    public float stillSusRate;
    public float NPCSusRate;
    public float CrouchSusRate;
    public float RunSusRate;


    public Dictionary<PlayerMode, float> susRates;

    public Dictionary<PlayerMode, float> getSusRates()
    {
        return new Dictionary<PlayerMode, float>()
        {
            {PlayerMode.Still, stillSusRate},
            {PlayerMode.NPC, NPCSusRate},
            {PlayerMode.Crouch, CrouchSusRate},
            {PlayerMode.Run, RunSusRate}
        };
    }
}
