using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NPCSettings : ScriptableObject
{
    public float bigBoundsRadius;
    public float smallBoundRadius;
    public float collisionCheckDist;
    public float NPCCheckRadius;
    public float avoidSkin;
    public float wallAvoidanceStrength;
    public float separationStrength;
    public float alignStregnth;
    public float cohesionStrength;

    public float shynessStrength;
    public Vector2 shyTimeRange;
    public float shyRunAwayTime;
    public float shyAvoidSkin;
    public float shyCheckRadius;
    public Vector2 shyBodiesThresholdRange;

    public LayerMask PersonMask;
    public LayerMask obstacleMask;
    public float walkacc;
    public float crouchMult;
    public float intentSteeringStrength;

    public float ObstaclefieldOfView;
    public float NPCfieldOfView;

    public float stuckRadius;
    public float stuckTime;
    public float unstuckTime;
}
