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
    GameObject chunkObject;
    RunShader generator;
    Player player;

    public Queue<Vector3> targetPositions { get; set; }

    public TerrainChunk(Vector3Int size, Vector3 offset, TerrainShape shape, RunShader generator)
    {
        this.size = size;
        this.generator = generator;
        this.shape = shape;
        this.offset = offset;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        targetPositions = new Queue<Vector3>();
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
        while(targetPositions.Count > 0)
        {
            Vector3 targetPosition = targetPositions.Dequeue();
            offset += targetPosition * 19;
            chunkObject.transform.position += targetPosition * 19;

        }
        genDensities();
        computeMesh();
        /*
        Vector3Int deltaChunk = player.getChunkPosition() - getChunkFromPos(chunkObject.transform.position);
        bool updated = false;
        if (Mathf.Abs(deltaChunk.x) == 3)
        {
            if (deltaChunk.x < 0) deltaChunk.x -= 2; else deltaChunk.x += 2;
            float chunkOff = (deltaChunk.x) * 19;
            offset += new Vector3(chunkOff, 0, 0);
            genDensities();
            computeMesh();
            chunkObject.transform.Translate(new Vector3(chunkOff, 0, 0));
            updated = true;
        }
        if (Mathf.Abs(deltaChunk.y) == 3)
        {
            if (deltaChunk.y < 0) deltaChunk.y -= 2; else deltaChunk.y += 2;
            float chunkOff = (deltaChunk.y) * 19;
            offset += new Vector3(0, chunkOff, 0);
            genDensities();
            computeMesh();
            chunkObject.transform.Translate(new Vector3(0, chunkOff, 0));
            updated = true;
        }
        if (Mathf.Abs(deltaChunk.z) == 3)
        {
            if (deltaChunk.z < 0) deltaChunk.z -= 2; else deltaChunk.z += 2;
            float chunkOff = (deltaChunk.z) * 19;
            offset += new Vector3(0, 0, chunkOff);
            genDensities();
            computeMesh();
            chunkObject.transform.Translate(new Vector3(0, 0, chunkOff));
            updated = true;
        }
        return updated;
        */
    }


    public void deform(Vector3 deformCenter, float radius)
    {
        int startX = (int)(deformCenter.x - radius);
        int startY = (int)(deformCenter.y - radius);
        int startZ = (int)(deformCenter.z - radius);

        int endX = (int)(deformCenter.x + radius);
        int endY = (int)(deformCenter.y + radius);
        int endZ = (int)(deformCenter.z + radius);

        for (int x = startX; x < endX; x ++)
        {
            for(int y = startY; y < endY; y ++)
            {
                for(int z = startZ; z < endZ; z ++)
                {
                    Vector3 relativeToChunk = (new Vector3(x, y, z) - chunkObject.transform.position);
                    relativeToChunk += new Vector3(10f, 10f, 10f);
                    if (relativeToChunk.x < 20 && relativeToChunk.x >= 0 && relativeToChunk.y < 20 && relativeToChunk.y >= 0 && relativeToChunk.z < 20 && relativeToChunk.z >= 0)
                    {
                        float dist = Mathf.Abs((new Vector3(x, y, z) - deformCenter).magnitude);
                        if (dist < 5)
                        {
                            int index = (int)relativeToChunk.x * 20 * 20 + (int)relativeToChunk.y * 20 + (int)relativeToChunk.z;
                            densities[index] -= dist - 5;
                        }
                    }
                }
            }
        }

        computeMesh();
    }

    public Vector3 CalculateChunkPos()
    {
        Vector3 chunkPos = getChunkFromPos(chunkObject.transform.position);
        foreach(Vector3 off in targetPositions)
        {
            chunkPos += off;
        }
        return chunkPos;
    }

    public static Vector3Int getChunkFromPos(Vector3 position)
    {
        Vector3 rawPos = new Vector3(position.x, position.y, position.z);
        Vector3Int chunkPos = signedTranslateToInt(rawPos / 19f, 0.5f);
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
