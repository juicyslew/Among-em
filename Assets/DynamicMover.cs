using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class DynamicMover : Mover
{
    SpriteRenderer sprend;
    Color crouchColor = new Color(0.5188679f, 0.5188679f, 0.5188679f);
    public Color runColor = new Color(0.213f, 0.3584906f, 0.695f);
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        sprend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    public override void Move(Vector2 intent, bool crouching, float walkaccOverride)
    {
        float walkacc = walkaccOverride;
        Vector2 movement = intent * walkacc * Time.deltaTime;
        movement *= crouching ? settings.crouchMult : 1f;
        body.AddForce(movement, ForceMode2D.Impulse);
        sprend.color = (movement != Vector2.zero && !crouching) ? runColor : crouchColor;
    }

    public void FixedUpdate()
    {
        if (body.velocity != Vector2.zero)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, body.velocity)));
        }
    }
}
