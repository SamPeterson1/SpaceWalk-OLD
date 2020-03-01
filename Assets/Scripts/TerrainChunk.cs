using static System.Buffer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    float[] densities;
    TerrainShape shape;
    Vector3 offset;
    public GameObject chunkObject;
    RunShader generator;


    public Vector3 toPlayer;
    public Bounds bounds;

    public static int SIZE = 40;

    public TerrainChunk(Vector3 offset, TerrainShape shape, RunShader generator)
    {

        this.generator = generator;
        this.shape = shape;
        this.offset = offset;
        bounds = new Bounds(offset, new Vector3(SIZE, SIZE, SIZE));
        genDensities();
        chunkObject = new GameObject();
        chunkObject.AddComponent<MeshFilter>();
        chunkObject.AddComponent<MeshRenderer>();
        chunkObject.AddComponent<MeshCollider>();
        chunkObject.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Terrain", typeof(Material)) as Material;
        chunkObject.transform.Translate(offset);
        computeMesh();
    }

    private void genDensities()
    {
        
        /*
        densities = new float[SIZE * SIZE * SIZE];
        int index = 0;
        for (int x = -SIZE / 2; x < SIZE / 2; x++)
        {
            for (int y = -SIZE / 2; y < SIZE / 2; y++)
            {
                for (int z = -SIZE / 2; z < SIZE / 2; z++)
                {
                    densities[index++] = shape.getDensity(x + offset.x, y + offset.y, z + offset.z);
                }
            }
        }
        */

        densities = shape.getDensities(offset);
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
        offset = chunkObject.transform.position;
        genDensities();
        computeMesh();
        chunkObject.SetActive(true);
    }


    public void deform(Vector3 deformCenter, float radius, int subtract)
    {
        int startX = (int)(deformCenter.x - radius);
        int startY = (int)(deformCenter.y - radius);
        int startZ = (int)(deformCenter.z - radius);

        int endX = (int)(deformCenter.x + radius);
        int endY = (int)(deformCenter.y + radius);
        int endZ = (int)(deformCenter.z + radius);

        bool updated = false;

        for (int x = startX; x < endX; x ++)
        {
            for(int y = startY; y < endY; y ++)
            {
                for(int z = startZ; z < endZ; z ++)
                {
                    Vector3 relativeToChunk = (new Vector3(x, y, z) - chunkObject.transform.position);
                    relativeToChunk += new Vector3(20, 20, 20);
                    if (relativeToChunk.x < 40 && relativeToChunk.x >= 0 && relativeToChunk.y < 40 && relativeToChunk.y >= 0 && relativeToChunk.z < 40 && relativeToChunk.z >= 0)
                    {
                        float dist = Mathf.Abs((new Vector3(x, y, z) - deformCenter).magnitude);
                        if (dist < 5)
                        {
                            int index = (int)relativeToChunk.x * 40 * 40 + (int)relativeToChunk.y * 40 + (int)relativeToChunk.z;
                            densities[index] -= (dist - 5) * 0.1f * subtract;
                            updated = true;
                        }
                    }
                }
            }
        }

        if (updated)
        {
            computeMesh();
        }
    }

    public Vector3 CalculateChunkPos()
    {
        Vector3 chunkPos = getChunkFromPos(chunkObject.transform.position);
        return chunkPos;
    }

    public static Vector3Int getChunkFromPos(Vector3 position)
    {
        Vector3 rawPos = new Vector3(position.x, position.y, position.z);
        Vector3Int chunkPos = signedTranslateToInt(rawPos / 39f, 0.5f);
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
