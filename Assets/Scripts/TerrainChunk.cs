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
    Vector3Int relativeToPlayer;
    GameObject chunkObject;
    RunShader generator;

    public TerrainChunk(Vector3Int size, Vector3 offset, TerrainShape shape, RunShader generator)
    {
        this.size = size;
        this.generator = generator;
        this.shape = shape;
        this.offset = offset;

        genDensities();
        chunkObject = new GameObject();
        chunkObject.AddComponent<MeshFilter>();
        chunkObject.AddComponent<MeshRenderer>();
        chunkObject.AddComponent<MeshCollider>();
        chunkObject.GetComponent<MeshRenderer>().material = Resources.Load("Materials/Terrain", typeof(Material)) as Material;
        computeMesh();
        chunkObject.transform.Translate(offset);
    }

    public void setRelativePosition(Vector3Int relativeToPlayer)
    {
        this.relativeToPlayer = relativeToPlayer;
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
            offset += new Vector3(5, 0, 0) * Time.deltaTime;
            genDensities();
            computeMesh();
        }
    }
}
