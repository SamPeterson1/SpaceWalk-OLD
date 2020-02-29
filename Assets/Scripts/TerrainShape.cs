using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainShape
{
    Noise noise = new Noise();
    int foo = 0;

    public float getDensity(float x, float y, float z)
    {
        Vector3 toCenter = new Vector3(x, y, z);

        
        float dist = toCenter.magnitude + noise.Evaluate(toCenter / 10f) * 1.5f;
      
        return dist - 1000f;
    }
}
