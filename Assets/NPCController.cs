using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    /**
     * Inspired heavily by Sebastian Lague's Boids project:
     * https://github.com/SebLague/Boids
     */
    StuckDetector stuckDetect;
    ShyDetector shyDetect;

    public NPCSettings settings;
    public bool hijacked = false;
    public Vector2 player_intent = Vector2.zero;
    Mover mover;
    Vector2 intent;
    Vector2 wall_avoidance_acc;
    Vector2 separation_acc;
    Vector2 alignment_acc;
    Vector2 cohesion_acc;
    Vector2 bestDirSaved;
    Vector2[] otherNPCdirs;
    int otherNPCnum = 0;

    

    // Start is called before the first frame update
    void Start()
    {
        mover = GetComponent<Mover>();
        transform.Rotate(0f, 0f, Random.Range(0f, 360f));
        intent = transform.right;
        wall_avoidance_acc = transform.right;
        alignment_acc = transform.right;
        cohesion_acc = transform.right;
        bestDirSaved = transform.right;
        stuckDetect = new StuckDetector(settings.stuckTime, settings.unstuckTime);
        float shyTime = Random.Range(settings.shyTimeRange.x, settings.shyTimeRange.y);
        int shyThreshold = Random.Range((int)settings.shyBodiesThresholdRange.x, (int)settings.shyBodiesThresholdRange.y);
        shyDetect = new ShyDetector(shyTime, settings.shyRunAwayTime, shyThreshold);
    }

    // Update is called once per frame
    void Update()
    {
        HijackableUpdate(false, Vector2.zero);
    }

    public void HijackableUpdate(bool useThisController, Vector2 lastControlIntent)
    {
        if (!hijacked)
        {
            mover.Move(intent, shyDetect.shyMode == ShyMode.Normal, settings.walkacc);
        }
        else
        {
            if (useThisController)
            {
                mover.Move(intent, true, settings.walkacc);
            }
            else
            {
                if (lastControlIntent != Vector2.zero) {
                    intent = lastControlIntent;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        bestDirSaved = WallAvoidanceLogic();
        if (bestDirSaved != Vector2.zero) {
            wall_avoidance_acc = SteerTowards(bestDirSaved.normalized);
            wall_avoidance_acc *= settings.wallAvoidanceStrength / (bestDirSaved.magnitude / settings.collisionCheckDist);
        }
        else
        {
            wall_avoidance_acc = Vector2.zero;
        }
        

        OtherNPCForces();
        CheckStuck(bestDirSaved.magnitude <= settings.stuckRadius && bestDirSaved != Vector2.zero);

        if (shyDetect.shyMode == ShyMode.Normal)
        {
            separation_acc *= settings.separationStrength;
            alignment_acc *= settings.alignStregnth;
            cohesion_acc *= settings.cohesionStrength;
        }
        else
        {
            separation_acc *= settings.separationStrength * settings.shynessStrength;
            alignment_acc = Vector2.zero;
            cohesion_acc = Vector2.zero;
        }
        intent += (wall_avoidance_acc + separation_acc + alignment_acc + cohesion_acc) * Time.fixedDeltaTime;
        intent = Vector2.ClampMagnitude(intent, 1f);
        //transform.right = intent.normalized;
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

    void CheckShy(bool currentlyOverwhelmed)
    {
        if (hijacked)
        {
            return;
        }
        if (shyDetect.shyMode == ShyMode.Normal)
        {
            if (currentlyOverwhelmed)
            {
                shyDetect.amIShyTimer -= Time.fixedDeltaTime;
                if (shyDetect.amIShyTimer < 0f)
                {
                    shyDetect.shyMode = ShyMode.RunAway;
                    shyDetect.amIShyTimer = shyDetect.amIShyInterval;
                }
            }
            else
            {
                shyDetect.amIShyTimer = shyDetect.amIShyInterval;
            }

        }
        else
        {
            if (shyDetect.getAwayTimer < 0f)
            {
                shyDetect.shyMode = ShyMode.Normal;
                shyDetect.getAwayTimer = shyDetect.getAwayInterval;
            }
            else
            {
                shyDetect.getAwayTimer -= Time.fixedDeltaTime;
            }

        }
    }

    void OtherNPCForces()
    {
        // Get All Other People in search radius
        // TODO - Add view angle.

        float NPCCheckRadius = shyDetect.shyMode == ShyMode.Normal ? settings.NPCCheckRadius : settings.shyCheckRadius;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, NPCCheckRadius, settings.PersonMask);

        RaycastHit2D[] hits = new RaycastHit2D[2];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(settings.PersonMask + settings.obstacleMask);

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
            
            if (len == 2 && settings.PersonMask == (settings.PersonMask | (1 << hits[1].collider.gameObject.layer)))
            {
                allbodies[numBodiesTotal] = hits[1].rigidbody;
                numBodiesTotal++;
                if (Vector2.Angle(transform.right, dir) < (180f * settings.NPCfieldOfView))
                {
                    visibleBodies[numBodiesVisible] = hits[1].rigidbody;
                    otherNPCdirs[numBodiesVisible] = dir;
                    numBodiesVisible++;
                    otherNPCnum = numBodiesVisible;
                }
            }
        }


        Vector2 separationIntent = Vector2.zero;
        Vector2 alignmentIntent = Vector2.zero;
        Vector2 cohesionLocation = mover.body.position;  // Include self in calculation
        bool shy = shyDetect.shyMode == ShyMode.RunAway;
        Rigidbody2D[] rigidBodies = shy ? allbodies : visibleBodies;
        int numBodies = shy ? numBodiesTotal : numBodiesVisible;
        for (int i = 0; i < numBodies; i++)
        {
            
            Vector2 dir = rigidBodies[i].transform.position - transform.position;
            
            float avoidSkin = shy ? settings.shyAvoidSkin : settings.avoidSkin;
            float dist_from_skin = Mathf.Max(dir.magnitude - avoidSkin, .1f);
            float sqrDst = dist_from_skin * dist_from_skin;

            // Separation - Don't crash into each other
            separationIntent -= dir / (shy ? dist_from_skin : sqrDst);

            // Alignment - Vaguely try to travel the same direction
            alignmentIntent += rigidBodies[i].velocity;

            // Cohesion - Attempt to stay near other NPCs
            cohesionLocation += rigidBodies[i].position;
        }

        // Extras:
        // Shyness - Run away if it gets too crowded.  (if too many people, slowly turn down all parameters other than Separation and Wall Avoidance)
        CheckShy(numBodiesTotal > shyDetect.shyThreshold); // Greater Than because this gameobject is included in numBodiesTotal

        Vector2 averageAlignment = numBodies > 0 ? (alignmentIntent / numBodies) : Vector2.zero;
        Vector2 cohesionIntent = (cohesionLocation / (numBodies + 1)) - (Vector2) transform.position;
        cohesion_acc = GroomAccel(cohesionIntent);
        separation_acc = GroomAccel(separationIntent);
        alignment_acc = GroomAccel(averageAlignment);
    }


    Vector2 WallAvoidanceLogic()
    {
        bool flagSmallRadius = false;
        bool flagHeadingForCollision = false;
        RaycastHit2D[] results = new RaycastHit2D[1];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(settings.obstacleMask);
        int len = Physics2D.CircleCast(transform.position, settings.bigBoundsRadius, intent, filter, results, settings.collisionCheckDist);
        for (int i = 0; i < len; i++)
        {
            RaycastHit2D hit = results[i];
            if (hit.collider.gameObject != gameObject)
            {
                flagHeadingForCollision = true;
                if(hit.distance < settings.bigBoundsRadius)
                {
                    flagSmallRadius = true;
                    break;
                }
            }
        }

        if (flagHeadingForCollision)
        {
            return FindUnobstructedDirection(flagSmallRadius ? settings.smallBoundRadius : settings.bigBoundsRadius);
        }
        return Vector2.zero;
    }

    Vector2 FindUnobstructedDirection(float castRadius)
    {
        Vector2 bestDir = transform.right;
        float furthestUnobstructedDst = 0;
        RaycastHit2D[] results = new RaycastHit2D[1];
        RaycastHit2D hit;
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(settings.obstacleMask);
        int dir_num = Mathf.RoundToInt(NPCHelper.directions.Length * ((stuckDetect.stuckMode == StuckMode.Stuck) ? 1f : settings.ObstaclefieldOfView));

        for (int i = 0; i < dir_num; i++)
        {
            Vector2 dir = transform.TransformDirection(NPCHelper.directions[i]);
            int len = Physics2D.CircleCast(transform.position, castRadius, dir, filter, results, settings.collisionCheckDist);
            if (len == 1) {
                hit = results[0];
                if (furthestUnobstructedDst < hit.distance)
                {
                    bestDir = dir;
                    furthestUnobstructedDst = hit.distance;
                }
            }
            else
            {
                return dir * settings.collisionCheckDist;
            }
        }
        return bestDir * furthestUnobstructedDst;
    }

    Vector2 GroomAccel(Vector2 intentDiff)
    {
        return (intentDiff).normalized * Mathf.Min(settings.intentSteeringStrength, intentDiff.magnitude);
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
        Gizmos.DrawRay(transform.position, alignment_acc);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, cohesion_acc);
        if (shyDetect.shyMode == ShyMode.RunAway)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position + new Vector3(.3f, .3f, 0), .1f);
        }
        /*Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, bestDirSaved);*/
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.grey;
        for (int i=0; i < otherNPCnum; i++)
        {
            Gizmos.DrawRay(transform.position, otherNPCdirs[i]);
        }
        
    }
}
