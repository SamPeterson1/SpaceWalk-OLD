using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeGenerator
{
    public struct BiomePoint
    {
        public int biome;
        public Vector3 pos;
    }

    public BiomePoint[] GenerateBiomes()
    {
        BiomePoint[] biomes = new BiomePoint[600];
        for(int i = 0; i < 600; i ++)
        {
            BiomePoint biome;
            if (i % 3 == 0)
            {
                biome.biome = (int)Biome.MOUNTAINS;
            } else if(i % 3 == 1)
            {
                biome.biome = (int)Biome.PLAINS;
            } else
            {
                biome.biome = (int)Biome.ROCKY_HILLS;
            }

            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);

            Vector3 randPoint = new Vector3(x, y, z).normalized;
            biome.pos = randPoint * 1000.0f;
            biomes[i] = biome;
        }

        return biomes;
    }

}
