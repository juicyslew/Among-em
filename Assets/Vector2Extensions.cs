using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class Vector2Extension
    {
        public static Vector2 Rotated(this Vector2 v0, float ang)
        {
            return new Vector2(Mathf.Cos(ang) * v0.x - Mathf.Sin(ang) * v0.y, Mathf.Sin(ang) * v0.x + Mathf.Cos(ang) * v0.y);
        }

        public static Vector2 UnitCircleRandomGaussian(this Vector2 v0)
        {
            float[] values = new float[2];
                
            for (int i = 0; i < 2; i++)
            {
                float v1, v2, s;
                do
                {
                    v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
                    v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1.0f || s == 0f);
                s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

                values[i] = v1 * s;
            }
            
            return new Vector2(values[0], values[1]);
        }
    }
}
