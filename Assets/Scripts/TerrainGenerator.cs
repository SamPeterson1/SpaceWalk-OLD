using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    TerrainChunk chunk;
    TerrainShape shape;
    public ComputeShader compute;
    public RunShader shader;
    private List<TerrainChunk> chunks;
    void Start()
    {
        float foo = Time.time;
        shader = new RunShader(compute);
        shape = new TerrainShape();
        chunks = new List<TerrainChunk>();

        for(int x = -2; x <= 2; x ++)
        {
            for(int y = -2; y <= 2; y ++)
            {
                for(int z = -2; z <= 2; z ++)
                {
                    genChunk(new Vector3(x*39, y*39, z*39));
                }
            }
        }

        Debug.Log(Time.realtimeSinceStartup - foo + " TIME");

    }

        void genChunk(Vector3 offset)
    {
        TerrainChunk chunk = new TerrainChunk(new Vector3Int(40, 40, 40), offset, shape, shader);
        chunks.Add(chunk);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(TerrainChunk chunk in chunks)
        {
            chunk.update();
        }
    }
}
