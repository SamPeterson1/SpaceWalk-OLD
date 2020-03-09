using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator
{
    public Dictionary<BiomeType, Biome> biomes;
    public struct BiomePoint
    {
        public int biome;
        public float roughness;
        public float amplitude;
        public float persistence;
        public float baseRoughness;
        public float numLayers;
        public float minRadius;
        public Vector3 center;
        public Vector3 pos;
    }

    public BiomeGenerator(List<Biome> biomes)
    {
        this.biomes = new Dictionary<BiomeType, Biome>();
        foreach(Biome biome in biomes)
        {
            this.biomes.Add(biome.type, biome);
        }
    }

    public BiomePoint[] GenerateBiomes()
    {
        BiomePoint[] biomes = new BiomePoint[600];
        for(int i = 0; i < 600; i ++)
        {
            BiomePoint biomePoint;
            Biome biome;
            if (i % 3 == 0)
            {
                this.biomes.TryGetValue(BiomeType.MOUNTAINS, out biome);
                
            } else if(i % 3 == 1)
            {
                this.biomes.TryGetValue(BiomeType.PLAINS, out biome);
            } else
            {
                this.biomes.TryGetValue(BiomeType.ROCKY_HILLS, out biome);
            }

            if (biome == null) Debug.LogError("Biome not found!");
            biomePoint.biome = (int) biome.type;
            NoiseSettings settings = biome.settings;
            biomePoint.amplitude = settings.amplitude;
            biomePoint.baseRoughness = settings.baseRoughness;
            biomePoint.center = settings.center;
            biomePoint.minRadius = settings.minRadius;
            biomePoint.numLayers = settings.numLayers;
            biomePoint.persistence = settings.persistence;
            biomePoint.roughness = settings.roughness;

            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);

            Vector3 randPoint = new Vector3(x, y, z).normalized;
            biomePoint.pos = randPoint * 1000.0f;
            biomes[i] = biomePoint;
        }

        return biomes;
    }

}
