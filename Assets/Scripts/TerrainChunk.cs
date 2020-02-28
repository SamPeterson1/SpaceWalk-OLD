using static System.Buffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    float[] densities;
    TerrainShape shape;
    Vector3Int size;
    Vector3 offset;
    Vector3 lastOffset;
    Vector3 staticOffset;
    GameObject chunkObject;
    RunShader generator;
    Player player;
    Vector3Int lastPlayerPos;

    public TerrainChunk(Vector3Int size, Vector3 offset, TerrainShape shape, RunShader generator)
    {
        this.size = size;
        this.generator = generator;
        this.shape = shape;
        this.offset = offset;
        staticOffset = offset;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        genDensities();
        chunkObject = new GameObject();
        chunkObject.AddComponent<MeshFilter>();
        chunkObject.AddComponent<MeshRenderer>();
        chunkObject.AddComponent<MeshCollider>();
        chunkObject.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Terrain", typeof(Material)) as Material;
        computeMesh();
        chunkObject.transform.Translate(offset);
    }

    private void genDensities()
    {
        densities = new float[size.x * size.y * size.z];
        int index = 0;
        for (int x = -size.x / 2; x < size.y / 2; x++)
        {
            for (int y = -size.y / 2; y < size.y / 2; y++)
            {
                for (int z = -size.z / 2; z < size.z / 2; z++)
                {
                    densities[index++] = shape.getDensity(x + offset.x, y + offset.y, z + offset.z);
                }
            }
        }
    }

    public void computeMesh()
    {
        
        Vector3[] triangles = generator.run(densities);
        genMesh(triangles);
    }

    private void genMesh(Vector3[] vertices)
    {
        int[] indices = new int[vertices.Length];
        for(int i = 0; i < indices.Length; i ++)
        {
            indices[i] = i;
        }
        Mesh mesh = chunkObject.GetComponent<MeshFilter>().mesh;

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = indices;

        mesh.RecalculateNormals();
        chunkObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public float[] getDensities()
    {
        return densities;
    }

    public void update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            offset += new Vector3(40, 0, 0);
            genDensities();
            computeMesh();
        }
        
        if(player.changedChunks())
        {
            Vector3Int deltaChunk = player.getChunkPosition() - getChunkFromPos(chunkObject.transform.position);
            if (Mathf.Abs(deltaChunk.x) >= 3)
            {
                if (deltaChunk.x < 0) deltaChunk.x -= 2; else deltaChunk.x += 2;
                float chunkOff = (deltaChunk.x) * 39;
                offset += new Vector3(chunkOff, 0, 0);
                genDensities();
                computeMesh();
                chunkObject.transform.Translate(new Vector3(chunkOff, 0, 0));
            }
            if (Mathf.Abs(deltaChunk.y) >= 3)
            {
                if (deltaChunk.y < 0) deltaChunk.y -= 2; else deltaChunk.y += 2;
                float chunkOff = (deltaChunk.y) * 39;
                offset += new Vector3(0, chunkOff, 0);
                genDensities();
                computeMesh();
                chunkObject.transform.Translate(new Vector3(0, chunkOff, 0));
            }
            if (Mathf.Abs(deltaChunk.z) >= 3)
            {
                if (deltaChunk.z < 0) deltaChunk.z -= 2; else deltaChunk.z += 2;
                float chunkOff = (deltaChunk.z) * 39;
                offset += new Vector3(0, 0, chunkOff);
                genDensities();
                computeMesh();
                chunkObject.transform.Translate(new Vector3(0, 0, chunkOff));
            }
        }
    }

    public static Vector3Int getChunkFromPos(Vector3 position)
    {
        Vector3 rawPos = new Vector3(position.x, position.y, position.z);
        rawPos.Scale(new Vector3(1 / 39f, 1 / 39f, 1 / 39f));
        Vector3Int chunkPos = signedTranslateToInt(rawPos, 0.5f);
        return chunkPos;
    }

    private static Vector3Int signedTranslateToInt(Vector3 vec, float off)
    {
        if (vec.x < 0) vec.x -= off; else vec.x += off;
        if (vec.y < 0) vec.y -= off; else vec.y += off;
        if (vec.z < 0) vec.z -= off; else vec.z += off;

        return new Vector3Int((int)vec.x, (int)vec.y, (int)vec.z);
    }
}
