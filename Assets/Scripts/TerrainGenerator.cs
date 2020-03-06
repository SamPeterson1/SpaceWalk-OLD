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
    private int chunkCount = 0;
    private bool updatingChunks;
    private float maxchunkLoadTimeMillis = 5;

    private Queue<TerrainChunk> recyclable;
    private Queue<TerrainChunk> needUpdate;
    private Dictionary<Vector3, TerrainChunk> active;

    private BinaryWriter chunkData;

    public NoiseSettings settings;
    readonly string shaderThing = "151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203,117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165,71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41,55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89,18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250,124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189,28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34,242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31,181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114,67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180";

    void Start()
    {
        ChunkIO.CreateFile();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        TerrainChunk.chunkPrefab = chunkPrefab;

        Debug.Log("Count: " + shaderThing.Split(',').Length);

        float foo = Time.time;
        shader = new RunShader(compute);
        shape = new TerrainShape();
        shape.settings = settings;
        shape.shader = densities;
        chunks = new List<TerrainChunk>();
        needUpdate = new Queue<TerrainChunk>();
        recyclable = new Queue<TerrainChunk>();
        active = new Dictionary<Vector3, TerrainChunk>();

        

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
        foreach (TerrainChunk chunk in active.Values)
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
        active.Add(offset, chunk);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (TerrainChunk chunk in active.Values)
            {
                chunk.genDensities();
                chunk.computeMesh();
            }
            TerrainChunk.SaveData();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach(TerrainChunk chunk in active.Values)
            {
                chunk.Save();
            }
            TerrainChunk.SaveData();
        } else if(Input.GetKeyDown(KeyCode.L))
        {
            TerrainChunk.loadSaveData();
            foreach (TerrainChunk chunk in active.Values)
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
            foreach (TerrainChunk chunk in active.Values)
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
