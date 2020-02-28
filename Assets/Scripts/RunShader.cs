using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunShader
{
    public ComputeShader shader;
    private static int FLOAT_BYTES = 4;
    
    struct Triangle
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public Vector3 pointC;
    };

    public RunShader(ComputeShader shader)
    {
        this.shader = shader;

        
    }

    public Vector3[] run(float[] densities)
    {

        int kernelHandle = shader.FindKernel("testing");
        ComputeBuffer densitiesBuffer = new ComputeBuffer(densities.Length, 4);
        densitiesBuffer.SetData(densities);
        
        ComputeBuffer trianglesBuffer = new ComputeBuffer(40 * 40 * 40 * 5, FLOAT_BYTES * 9, ComputeBufferType.Append);
        trianglesBuffer.SetCounterValue(0);

        shader.SetBuffer(kernelHandle, "densities", densitiesBuffer);
        shader.SetBuffer(kernelHandle, "triangles", trianglesBuffer);
        shader.Dispatch(kernelHandle, 5, 5, 5);
        ComputeBuffer triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        int[] triCountArray = { 0 };

        triCountBuffer.SetData(triCountArray);
        ComputeBuffer.CopyCount(trianglesBuffer, triCountBuffer, 0);
        triCountBuffer.GetData(triCountArray);

        Triangle[] triangles = new Triangle[triCountArray[0]];
        trianglesBuffer.GetData(triangles);
        Vector3[] outVerts = new Vector3[triangles.Length * 3];
        for(int i = 0; i < triangles.Length; i ++)
        {
            outVerts[i*3] = triangles[i].pointA;
            outVerts[i*3 + 1] = triangles[i].pointB;
            outVerts[i*3 + 2] = triangles[i].pointC;
        }

        return outVerts;
    }

}
