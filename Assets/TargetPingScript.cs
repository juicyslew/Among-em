using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class TargetPingScript : MonoBehaviour
{

    public SeekerSettings settings;
    [HideInInspector]
    public float pingRadius = 0f;
    public Transform follow;
    GameController gcont;

    // Start is called before the first frame update
    void Start()
    {
        gcont = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
        pingRadius += settings.targetPingGrowthRate * Time.deltaTime;
    }

    public void InvokeAccuratePingAll()
    {
        gcont.AnnounceLocation();
    }

    public void InvokeAccuratePing()
    {
        pingRadius = 0f;
        transform.position = follow.position;
    }

    public void InvokeAccuratePingNoCooldown()
    {
        pingRadius = 0f;
        transform.position = follow.position;
    }
    
    public void InvokeInaccuratePing()
    {
        pingRadius = 0f;
        transform.position = (Vector2)follow.position + Vector2.zero.UnitCircleRandomGaussian() * settings.targetPingRandomOffset;  // Could also make this based on seeker distance
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pingRadius);
    }
}
