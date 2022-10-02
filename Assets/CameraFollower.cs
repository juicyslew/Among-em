using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform target;
    public float snapiness = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos_diff = target.position - transform.position;
        Vector2 move_intent = pos_diff * Mathf.Min(Time.deltaTime * snapiness, 1f);
        transform.position = transform.position + (Vector3)move_intent;
    }
}
