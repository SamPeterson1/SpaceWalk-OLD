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

    void Start()
    {
        ChunkIO.CreateFile();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
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

    public void deform(Vector3 pos, float radius, int subtract)
    {
       // int i = 0;
        foreach (TerrainChunk chunk in chunks)
        {
            Vector3 chunkCenter = chunk.chunkObject.transform.position;
            Vector3 toCenter = chunkCenter - pos;
            
            if (toCenter.magnitude - radius < 40)
            {
                //i++;
                chunk.deform(pos, radius, subtract);
            }
        }
        //Debug.Log(i);
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
        /*
        player.readChunkData();
        List<Vector3> remove = new List<Vector3>();
        foreach (TerrainChunk chunk in active.Values)
        {
            Vector3 displacement = chunk.CalculateChunkPos() - player.getChunkPosition();
            if (Mathf.Abs(displacement.x) >= dist+1 || Mathf.Abs(displacement.y) >= dist+1 || Mathf.Abs(displacement.z) >= dist+1)
            {
                Debug.Log("remocve"  + recyclable.Count);
                remove.Add(chunk.chunkObject.transform.position);
                chunk.chunkObject.transform.position = Vector3.zero;
                chunk.chunkObject.SetActive(false);
                recyclable.Enqueue(chunk);
                Debug.Log("remocve" + recyclable.Count);
            }
        }

        

        for(int x = -dist; x <= dist; x ++)
        {
            for(int y = -dist; y <= dist; y ++)
            {
                for(int z = -dist; z <= dist; z ++)
                {
                    Vector3 center = (player.getChunkPosition() + new Vector3(x, y, z)) * (TerrainChunk.SIZE - 1);
                    if (!active.ContainsKey(center))
                    {
                        if (recyclable.Count > 0)
                        {
                            Debug.Log("Recycle" + recyclable.Count);
                            TerrainChunk chunk = recyclable.Dequeue();
                            chunk.chunkObject.transform.position = center;
                            needUpdate.Enqueue(chunk);
                        }
                        else
                        {
                            Debug.Log("No recycle");
                            TerrainChunk chunk = new TerrainChunk(center, shape, shader);
                            chunk.chunkObject.transform.position = center;
                            active.Add(center, chunk);
                            needUpdate.Enqueue(chunk);
                        }
                    }
                }
            }
        }

        foreach (Vector3 vec in remove)
        {
            active.Remove(vec);
        }

        if (needUpdate.Count > 0)
        {
            needUpdate.Dequeue().update();
        }
        */

        player.readChunkData();
        Vector3 deltaChunk = player.getDeltaChunk();
        player.readPastChunk();
        if (player.changedChunks())
        {
            foreach (TerrainChunk chunk in chunks)
            {

                Vector3 chunkPos = chunk.CalculateChunkPos();
                Vector3 displacement = chunkPos - player.getChunkPosition();
                /*
                if (Mathf.Abs(displacement.x) >= 3 || Mathf.Abs(displacement.y) >= 3 || Mathf.Abs(displacement.z) >= 3)
                {
                    Vector3 foo = deltaChunk * -1;
                    if (foo.x != 0) foo.x += 4f * Mathf.Sign(foo.x);
                    if (foo.y != 0) foo.y += 4f * Mathf.Sign(foo.y);
                    if (foo.z != 0) foo.z += 4f * Mathf.Sign(foo.z);
                    chunk.chunkObject.SetActive(false);
                    chunk.chunkObject.transform.position = (chunkPos + foo) * 19;
                    needUpdate.Enqueue(chunk);
                }
                 */
                if (Mathf.Abs(displacement.x) >= 3)
                {
                    chunk.chunkObject.transform.Translate(new Vector3(-5 * 39 * Mathf.Sign(displacement.x), 0, 0));
                    needUpdate.Enqueue(chunk);
                    chunk.chunkObject.SetActive(false);
                } else if (Mathf.Abs(displacement.y) >= 3)
                {
                    chunk.chunkObject.transform.Translate(new Vector3(0, -5 * 39 * Mathf.Sign(displacement.y), 0));
                    needUpdate.Enqueue(chunk);
                    chunk.chunkObject.SetActive(false);
                } else if (Mathf.Abs(displacement.z) >= 3)
                {
                    chunk.chunkObject.transform.Translate(new Vector3(0, 0, -5 * 39 * Mathf.Sign(displacement.z)));
                    needUpdate.Enqueue(chunk);
                    chunk.chunkObject.SetActive(false);
                }




            }
        }

        //Optimizer.begin();
        //while (Optimizer.getDeltaTime() < 5)
        //{
            if (needUpdate.Count > 0)
            {
                TerrainChunk chunk = needUpdate.Dequeue();
                chunk.Save();
                chunk.update();
            }
            //{
             //   break;
            //}
        //}
        

    }

}
