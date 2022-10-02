using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.SceneManagement;



public class SeekerController : MonoBehaviour
{
    StuckDetector stuckDetect;
    SusMeter susMeter;
    //ChaseDetector chaseDetect;

    public NPCSettings npcSettings;
    public SeekerSettings seekerSettings;
    public Vector2 player_intent = Vector2.zero;
    public Transform pingTarget;
    public Transform realTarget;
    PlayerController player;
    TargetPingScript pingScript;
    Mover mover;
    Vector2 intent;
    Vector2 wall_avoidance_acc;
    Vector2 separation_acc;
    Vector2 target_acc;
    //Vector2 exploration_acc;
    Vector2 bestDirSaved;
    Vector2[] otherNPCdirs;
    float targetingStrength;
    float targetingInAvoidanceStrength;
    int otherNPCnum = 0;
    bool isInsidePing = false;
    bool chasing = false;
    bool stalking = false;

    // Start is called before the first frame update
    void Start()
    {
        mover = GetComponent<Mover>();
        transform.Rotate(0f, 0f, Random.Range(0f, 360f));
        intent = transform.right;
        wall_avoidance_acc = transform.right;
        target_acc = transform.right;
        //exploration_acc = transform.right;
        bestDirSaved = transform.right;
        stuckDetect = new StuckDetector(npcSettings.stuckTime, npcSettings.unstuckTime);
        pingScript = pingTarget.GetComponent<TargetPingScript>();
        player = realTarget.GetComponent<PlayerController>();
        susMeter = new SusMeter(seekerSettings.susCapacity, seekerSettings.susResetInterval, seekerSettings.susPercentage);
    }

    // Update is called once per frame
    void Update()
    {
        mover.Move(((!chasing && isInsidePing) || stalking) ? intent*seekerSettings.searchSpeedMult : intent, false, seekerSettings.walkacc);
    }

    float EbbAndFlow(float max, float min, float period)
    {
        float amp = max - min;
        float avg = (max + min) / 2f;
        return amp / 2f * Mathf.Sin(2 * Mathf.PI * Time.fixedTime / period) + avg;
    }

    private void FixedUpdate()
    {
        float targStrengthDiff = (seekerSettings.targetingStrengthRange.y - seekerSettings.targetingStrengthRange.x) / 2f;
        float targStrengthAvg = (seekerSettings.targetingStrengthRange.y + seekerSettings.targetingStrengthRange.x) / 2f;

        CheckSus();
        isInsidePing = (pingTarget.position - transform.position).magnitude < pingScript.pingRadius;

        targetingStrength = EbbAndFlow(seekerSettings.targetingStrengthRange.y, seekerSettings.targetingStrengthRange.x, seekerSettings.targetingEbbFlowSpeed);
        targetingInAvoidanceStrength = EbbAndFlow(seekerSettings.targetingInAvoidanceStrengthRange.y, seekerSettings.targetingInAvoidanceStrengthRange.x, seekerSettings.targetingInAvoidanceEbbFlowSpeed);
        bestDirSaved = WallAvoidanceLogic();
        if (bestDirSaved != Vector2.zero)
        {
            wall_avoidance_acc = SteerTowards(bestDirSaved.normalized);
            wall_avoidance_acc *= npcSettings.wallAvoidanceStrength / (bestDirSaved.magnitude / npcSettings.collisionCheckDist);
        }
        else
        {
            wall_avoidance_acc = Vector2.zero;
        }

        OtherNPCForces();
        CheckStuck(bestDirSaved.magnitude <= npcSettings.stuckRadius && bestDirSaved != Vector2.zero);


        if (stuckDetect.stuckMode == StuckMode.Stuck)
        {
            separation_acc *= 0f;
            target_acc *= 0f;
        }
        else if (chasing || stalking)
        {
            separation_acc *= npcSettings.separationStrength;
            target_acc *= seekerSettings.chaseStrength;
        }
        else
        {
            float targetForceStrengthMult = (isInsidePing) ? seekerSettings.targetInsidePingStrengthMult : 1f;
            separation_acc *= npcSettings.separationStrength;
            target_acc *= targetingStrength * targetForceStrengthMult;
        }
        

        intent += (wall_avoidance_acc + separation_acc + target_acc) * Time.fixedDeltaTime;
        intent = Vector2.ClampMagnitude(intent, 1f);
        //transform.right = intent.normalized;
    }

    void CheckSus()
    {
        // Can we see player?
        bool playerSeeable = false;
        chasing = false;
        stalking = false;

        Vector2 dir = realTarget.position - transform.position;
        RaycastHit2D[] hits = new RaycastHit2D[4];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(npcSettings.PersonMask + npcSettings.obstacleMask);
        int len = Physics2D.Raycast(transform.position, dir, filter, hits, dir.magnitude);
        if (len >= 2)
        {
            for (int i = 1; i < len; i++)
            {
                if (npcSettings.obstacleMask == (npcSettings.obstacleMask | (1 << hits[i].collider.gameObject.layer))) break;
                if (hits[i].transform == realTarget.transform) {

                    if (Vector2.Angle(transform.right, dir) < (180f * seekerSettings.searchFieldOfView) && (hits[i].point - (Vector2)transform.position).magnitude < seekerSettings.searchDist)
                    {
                        playerSeeable = true;
                        break;
                    }
                }
            }
            
        }

        if (playerSeeable)
        {
            if (susMeter.getMode() == SusMode.Chase)
            {
                Debug.Log(gameObject.name + "is Chasing you");
                pingScript.InvokeAccuratePingAll();
                chasing = true;
                return;
            }

            // Add to Sussiness
            susMeter.currentSus += seekerSettings.getSusRates()[player.pMode] / (Mathf.Log(dir.magnitude - npcSettings.avoidSkin + 1f) + 1f) * Time.fixedDeltaTime; 


            if (susMeter.getMode() == SusMode.Sus)
            {
                Debug.Log(gameObject.name + "is Sus of you");
                stalking = true;
                pingScript.InvokeAccuratePingNoCooldown();
                return;
            }
        }
        else
        {
            if (susMeter.susResetTimer < 0f)
            {
                susMeter.currentSus = 0f;
                susMeter.susResetTimer = susMeter.susResetInterval;
            }
            else
            {
                susMeter.susResetTimer -= Time.fixedDeltaTime;
            }
        }
        
    }

    void CheckStuck(bool currentlyStuck)
    {
        if (stuckDetect.stuckMode == StuckMode.Normal)
        {
            if (currentlyStuck)
            {
                stuckDetect.amIStuckTimer -= Time.fixedDeltaTime;
                if (stuckDetect.amIStuckTimer < 0f)
                {
                    stuckDetect.stuckMode = StuckMode.Stuck;
                    stuckDetect.amIStuckTimer = stuckDetect.amIStuckInterval;
                }
            }
            else
            {
                stuckDetect.amIStuckTimer = stuckDetect.amIStuckInterval;
            }

        }
        else
        {
            if (stuckDetect.getUnstuckTimer < 0f)
            {
                stuckDetect.stuckMode = StuckMode.Normal;
                stuckDetect.getUnstuckTimer = stuckDetect.getUnstuckInterval;
            }
            else
            {
                stuckDetect.getUnstuckTimer -= Time.fixedDeltaTime;
            }

        }
    }

    void OtherNPCForces()
    {
        // Get All Other People in search radius

        float NPCCheckRadius = seekerSettings.avoidFieldOfView;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, NPCCheckRadius, npcSettings.PersonMask);

        RaycastHit2D[] hits = new RaycastHit2D[2];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(npcSettings.PersonMask + npcSettings.obstacleMask);

        Rigidbody2D[] visibleBodies = new Rigidbody2D[colliders.Length];
        int numBodiesVisible = 0;

        Rigidbody2D[] allbodies = new Rigidbody2D[colliders.Length];
        otherNPCdirs = new Vector2[colliders.Length];
        int numBodiesTotal = 0;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D col = colliders[i];

            Vector2 dir = col.transform.position - transform.position;

            int len = Physics2D.Raycast(transform.position, dir.normalized, filter, hits, NPCCheckRadius);

            if (len == 2 && npcSettings.PersonMask == (npcSettings.PersonMask | (1 << hits[1].collider.gameObject.layer)))
            {
                allbodies[numBodiesTotal] = hits[1].rigidbody;
                numBodiesTotal++;
                if (Vector2.Angle(transform.right, dir) < (180f * seekerSettings.avoidFieldOfView))
                {
                    visibleBodies[numBodiesVisible] = hits[1].rigidbody;
                    otherNPCdirs[numBodiesVisible] = dir;
                    numBodiesVisible++;
                    otherNPCnum = numBodiesVisible;
                }
            }
        }


        Vector2 separationIntent = Vector2.zero;
        for (int i = 0; i < numBodiesVisible; i++)
        {
            float avoidSkin = npcSettings.avoidSkin;
            float dist_from_skin = Mathf.Max(otherNPCdirs[i].magnitude - avoidSkin, .1f);
            float sqrDst = dist_from_skin * dist_from_skin;
            // Separation - Don't crash into NPCs
            separationIntent -= otherNPCdirs[i] / sqrDst;

        }

        // Target - Move towards target area
        Vector2 targetIntent = pingTarget.position - transform.position;

        // Sus Meter - Staring at you for too long will fill up the chaser's sus meter and let them realize it's you.
        // Acting like an NPC fills it slowly, crouch walking fills it faster, running fills it very fast!
        // Chasers don't do anything until sus meter is filled
        // Then they cautiously check you out.  This would be a new movement mode essentially.  They lose sussiness after a bit of time.

        // TODO: Exploration - Aim to go to nearby unexplored Areas


        separation_acc = GroomAccel(separationIntent);
        target_acc = GroomAccel(targetIntent.normalized);
    }


    Vector2 WallAvoidanceLogic()
    {
        bool flagSmallRadius = false;
        bool flagHeadingForCollision = false;
        RaycastHit2D[] results = new RaycastHit2D[1];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(npcSettings.obstacleMask);
        int len = Physics2D.CircleCast(transform.position, npcSettings.bigBoundsRadius, intent, filter, results, npcSettings.collisionCheckDist);
        for (int i = 0; i < len; i++)
        {
            RaycastHit2D hit = results[i];
            if (hit.collider.gameObject != gameObject)
            {
                flagHeadingForCollision = true;
                if (hit.distance < npcSettings.bigBoundsRadius)
                {
                    flagSmallRadius = true;
                    break;
                }
            }
        }

        if (flagHeadingForCollision)
        {
            return FindUnobstructedDirection(flagSmallRadius ? npcSettings.smallBoundRadius : npcSettings.bigBoundsRadius);
        }
        return Vector2.zero;
    }

    Vector2 FindUnobstructedDirection(float castRadius)
    {
        // Towards Player
        Vector2 targetDir = ((Vector2)transform.right + (target_acc).normalized * targetingInAvoidanceStrength).normalized;
        Vector2 bestDir = targetDir;
        float furthestUnobstructedDst = 0;
        RaycastHit2D[] results = new RaycastHit2D[1];
        RaycastHit2D hit;
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(npcSettings.obstacleMask);
        int dir_num = Mathf.RoundToInt(NPCHelper.directions.Length * ((stuckDetect.stuckMode == StuckMode.Stuck) ? 1f : npcSettings.ObstaclefieldOfView));

        float ang = Vector2.SignedAngle(Vector2.right, targetDir) * Mathf.Deg2Rad;

        for (int i = 0; i < dir_num; i++)
        {
            Vector2 dir = NPCHelper.directions[i].Rotated(ang);
            int len = Physics2D.CircleCast(transform.position, castRadius, dir, filter, results, npcSettings.collisionCheckDist);
            if (len == 1)
            {
                hit = results[0];
                if (furthestUnobstructedDst < hit.distance)
                {
                    bestDir = dir;
                    furthestUnobstructedDst = hit.distance;
                }
            }
            else
            {
                return dir * npcSettings.collisionCheckDist;
            }
        }
        return bestDir * furthestUnobstructedDst;
    }

    Vector2 GroomAccel(Vector2 intentDiff)
    {
        return (intentDiff).normalized * Mathf.Min(seekerSettings.intentSteeringStrength, intentDiff.magnitude);
    }

    Vector2 SteerTowards(Vector2 goalIntent)
    {
        Vector2 intentDiff = goalIntent - intent;
        return GroomAccel(intentDiff.normalized);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, intent);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, wall_avoidance_acc);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, separation_acc);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, target_acc);
        Gizmos.color = Color.green;

        Gizmos.color = GetComponent<SpriteRenderer>().color;
        Gizmos.DrawWireSphere(transform.position, seekerSettings.searchDist);
        /*Gizmos.DrawRay(transform.position, exploration_acc);
        
        for (int i = 0 
        */
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.grey;
        for (int i = 0; i < otherNPCnum; i++)
        {
            Gizmos.DrawRay(transform.position, otherNPCdirs[i]);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform == realTarget)
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}
