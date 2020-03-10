using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    TerrainChunk chunk;
    TerrainShape shape;
    public ComputeShader compute;
    public ComputeShader densities;
    public RunShader shader;
    public ColorGenerator colorGen;
    private List<TerrainChunk> chunks;

    private Plane[] view;
    private Camera camera;
    public int dist;

    public GameObject chunkPrefab;

    private Player player;

    private Queue<TerrainChunk> needUpdate;

    private BinaryWriter chunkData;

    public List<Biome> biomes;
    public NoiseSettings settings;
    public TetherNetwork tetherNetwork;

    void Start()
    {
        ChunkIO.CreateFile();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        tetherNetwork = GameObject.FindGameObjectWithTag("Planet").GetComponent<TetherNetwork>();
        TerrainChunk.chunkPrefab = chunkPrefab;

        float foo = Time.time;
        shader = new RunShader(compute);
        shape = new TerrainShape(biomes);
        shape.settings = settings;
        shape.shader = densities;
        chunks = new List<TerrainChunk>();
        needUpdate = new Queue<TerrainChunk>();

        

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    genChunk(new Vector3(x * 39, y * 39, z * 39) + new Vector3(0, 990, 0));
                }
            }
        }

        Debug.Log(Time.realtimeSinceStartup - foo + " TIME");
    }

    public void deform(Vector3 pos, float radius, float rate, bool subtract)
    {
        foreach (TerrainChunk chunk in chunks)
        {
            Vector3 chunkCenter = chunk.chunkObject.transform.position;
            Vector3 toCenter = chunkCenter - pos;
            
            if (toCenter.magnitude - radius < 40)
            {
                chunk.deform(pos, radius, rate, subtract);
            }
        }
    }

    void genChunk(Vector3 offset)
    {
        TerrainChunk chunk = new TerrainChunk(offset, shape, shader, chunkData, this);
        chunks.Add(chunk);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (TerrainChunk chunk in chunks)
            {
                chunk.genDensities();
                chunk.computeMesh();
            }
            TerrainChunk.SaveData(shape.biomePoints);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach(TerrainChunk chunk in chunks)
            {
                chunk.Save();
            }
            TerrainChunk.SaveData(shape.biomePoints);
        } else if(Input.GetKeyDown(KeyCode.L))
        {
            shape.biomePoints = TerrainChunk.loadSaveData(shape.biomeGenerator);
            foreach (TerrainChunk chunk in chunks)
            {
                chunk.genDensities();
                chunk.computeMesh();
            }
        }

        QueueChunks();
        UpdateChunks();
        
    }

    void UpdateChunks()
    {
        if (needUpdate.Count > 0)
        {
            TerrainChunk chunk = needUpdate.Dequeue();
            chunk.Save(); /* Save current chunk data before ovewriting */
            chunk.update();
            tetherNetwork.LoadTethersInChunk(chunk.chunkObject.transform.position);
        }
    }

    void QueueChunks()
    {
        player.readChunkData();
        player.readPastChunk();
        if (player.changedChunks())
        {
            foreach (TerrainChunk chunk in chunks)
            {
                Vector3 displacement = chunk.CalculateChunkPos() - player.getChunkPosition();
                if (Mathf.Abs(displacement.x) >= 3)
                {
                    tetherNetwork.UnloadTethersInChunk(chunk.chunkObject.transform.position);
                    chunk.chunkObject.transform.Translate(new Vector3(-5 * 39 * Mathf.Sign(displacement.x), 0, 0));
                    needUpdate.Enqueue(chunk);
                    chunk.chunkObject.SetActive(false);
                }
                else if (Mathf.Abs(displacement.y) >= 3)
                {
                    chunk.chunkObject.transform.Translate(new Vector3(0, -5 * 39 * Mathf.Sign(displacement.y), 0));
                    needUpdate.Enqueue(chunk);
                    chunk.chunkObject.SetActive(false);
                }
                else if (Mathf.Abs(displacement.z) >= 3)
                {
                    chunk.chunkObject.transform.Translate(new Vector3(0, 0, -5 * 39 * Mathf.Sign(displacement.z)));
                    needUpdate.Enqueue(chunk);
                    chunk.chunkObject.SetActive(false);
                }
            }
        }
    }
}
