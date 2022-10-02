using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mover : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D body;
    public NPCSettings settings;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public abstract void Move(Vector2 intent, bool crouching, float walkaccOverridde);
}
