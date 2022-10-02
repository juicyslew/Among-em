using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public TargetPingScript[] pingScripts;
    float globalPingInterval = 10f;
    float globalPingTimer = -1f;
    private void Update()
    {
        if (globalPingTimer < 0f)
        {
            globalPingTimer = globalPingInterval;
            for (int i = 0; i < pingScripts.Length; i++)
            {
                pingScripts[i].InvokeInaccuratePing();
            }
        }
        globalPingTimer -= Time.deltaTime;
    }

    public void AnnounceLocation()
    {
        for (int i = 0; i < pingScripts.Length; i++)
        {
            pingScripts[i].InvokeAccuratePing();
        }
    }
}
