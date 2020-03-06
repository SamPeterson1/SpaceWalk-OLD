using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainShape
{
    Noise noise = new Noise();
    int foo = 0;

    public NoiseSettings settings;
    public ComputeShader shader;
    public BiomeGenerator biomeGenerator = new BiomeGenerator();
    BiomeGenerator.BiomePoint[] biomePoints;

    public TerrainShape()
    {
        biomePoints = biomeGenerator.GenerateBiomes();
    }

    public float[] getDensities(Vector3 offset)
    {
        int kernelHandle = shader.FindKernel("CSMain");

        ComputeBuffer densitiesBuffer = new ComputeBuffer(40 * 40 * 40, sizeof(float));

        
        ComputeBuffer biomesBuffer = new ComputeBuffer(biomePoints.Length, sizeof(float) * 3 + sizeof(int));
        biomesBuffer.SetData(biomePoints);

        shader.SetBuffer(kernelHandle, "biomes", biomesBuffer);
        shader.SetBuffer(kernelHandle, "densities", densitiesBuffer);

        shader.SetFloat("xOff", offset.x);
        shader.SetFloat("yOff", offset.y);
        shader.SetFloat("zOff", offset.z);
        loadSettingsToGPU();
        shader.Dispatch(kernelHandle, 5, 5, 5);

        float[] densities = new float[40 * 40 * 40];
        densitiesBuffer.GetData(densities);
        densitiesBuffer.Dispose();

        return densities;
    }

    private void loadSettingsToGPU()
    {
        shader.SetFloat("roughness", settings.roughness);
        shader.SetFloat("persistence", settings.persistence);
        shader.SetFloat("baseRoughness", settings.baseRoughness);
        shader.SetFloat("amplitude", settings.amplitude);
        shader.SetFloat("numLayers", settings.numLayers);
        shader.SetFloat("minRadius", settings.minRadius);
    }

    public float getDensity(float x, float y, float z)
    {
        Vector3 toCenter = new Vector3(x, y, z);

        float radius = noise.Evaluate(toCenter.normalized * 1000f / 40f)*9f + 1000f;
        return toCenter.magnitude - radius;
    }
}
