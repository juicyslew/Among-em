using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NPCHelper
{

    const int numViewDirections = 50;
    public static readonly Vector2[] directions;

    static NPCHelper()
    {
        directions = new Vector2[NPCHelper.numViewDirections];
        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections * Mathf.PI * ((i % 2 == 0) ? 1f : -1f);

            float x = Mathf.Cos(t);
            float y = Mathf.Sin(t);
            directions[i] = new Vector2(x, y);
        }
    }

}