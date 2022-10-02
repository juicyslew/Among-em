using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Mover mover;
    public NPCController hijackedController;
    public PlayerMode pMode;
    public NPCSettings settings;
    // Start is called before the first frame update
    void Start()
    {
        mover = GetComponent<Mover>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 intent = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);

        if (hijackedController)
        {
            bool useController = Input.GetKey(KeyCode.LeftControl) && intent == Vector2.zero;
            hijackedController.HijackableUpdate(useController, intent);
            if (useController)
            {
                pMode = PlayerMode.NPC;
            }
            else
            {
                NormalMove(intent);
            }
        }
        else
        {
            NormalMove(intent);
        }
    }

    void NormalMove(Vector2 intent)
    {
        bool crouch = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl);
        mover.Move(intent, crouch, settings.walkacc);
        pMode = crouch ? PlayerMode.Crouch : PlayerMode.Run;

        if (intent == Vector2.zero)
        {
            pMode = PlayerMode.Still;
        }
    }
}
