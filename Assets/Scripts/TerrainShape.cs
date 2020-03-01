using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainShape
{
    Noise noise = new Noise();
    int foo = 0;

    public ComputeShader shader;

    public float[] getDensities(Vector3 offset)
    {
        int kernelHandle = shader.FindKernel("CSMain");

        ComputeBuffer densitiesBuffer = new ComputeBuffer(40 * 40 * 40, sizeof(float));

        shader.SetBuffer(kernelHandle, "densities", densitiesBuffer);

        shader.SetFloat("xOff", offset.x);
        shader.SetFloat("yOff", offset.y);
        shader.SetFloat("zOff", offset.z);
        shader.Dispatch(kernelHandle, 5, 5, 5);

        float[] densities = new float[40 * 40 * 40];
        densitiesBuffer.GetData(densities);
        densitiesBuffer.Dispose();

        return densities;
    }

    public float getDensity(float x, float y, float z)
    {
        Vector3 toCenter = new Vector3(x, y, z);

        
        float dist = toCenter.magnitude;
      
        return dist - 1000f;
    }
}
