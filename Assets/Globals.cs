using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum StuckMode { Normal, Stuck };
enum ShyMode { Normal, RunAway };

public enum SusMode { Normal, Sus, Chase };

public enum PlayerMode { NPC, Crouch, Run, Still};



struct StuckDetector
{
    public float amIStuckInterval, amIStuckTimer;
    public float getUnstuckInterval, getUnstuckTimer;

    public StuckMode stuckMode;

    public StuckDetector(float amIStuckInterval, float getUnstuckInterval)
    {
        this.amIStuckInterval = amIStuckInterval;
        this.getUnstuckInterval = getUnstuckInterval;
        this.amIStuckTimer = amIStuckInterval;
        this.getUnstuckTimer = getUnstuckInterval;
        this.stuckMode = StuckMode.Normal;
    }
}

struct ShyDetector
{
    public float amIShyInterval, amIShyTimer;
    public float getAwayInterval, getAwayTimer;
    public int shyThreshold;

    public ShyMode shyMode;

    public ShyDetector(float amIShyInterval, float getAwayInterval, int shyThreshold = 3)
    {
        this.shyThreshold = shyThreshold;
        this.amIShyInterval = amIShyInterval;
        this.getAwayInterval = getAwayInterval;
        this.amIShyTimer = amIShyInterval;
        this.getAwayTimer = getAwayInterval;
        this.shyMode = ShyMode.Normal;
    }
}

struct SusMeter
{
    public float susCapacity, currentSus;
    public float susResetInterval, susResetTimer;
    public float susPercentage;

    public SusMeter(float susCapacity, float susResetInterval, float susPercentage)
    {
        this.susCapacity = susCapacity;
        this.currentSus = 0f;
        this.susResetInterval = susResetInterval;
        this.susResetTimer = susResetInterval;
        this.susPercentage = susPercentage;
    }

    public SusMode getMode()
    {
        float currPercentage = currentSus / susCapacity;
        if (currPercentage >= 1f)
        {
            return SusMode.Chase;
        }
        else if (currPercentage >= susPercentage)
        {
            return SusMode.Sus;
        }
        else
        {
            return SusMode.Normal;
        }
    }
}

/*struct ChaseDetector
{
    public float chaseDecayInterval, chaseDecayTimer;
    public bool chasing;

    public ChaseDetector(float chaseDecayInterval)
    {
        this.chaseDecayInterval = chaseDecayInterval;
        this.chaseDecayTimer = chaseDecayInterval;
        this.chasing = false;
    }
}*/
