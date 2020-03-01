using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    TerrainChunk chunk;
    TerrainShape shape;
    public ComputeShader compute;
    public ComputeShader densities;
    public RunShader shader;
    private List<TerrainChunk> chunks;

    private Plane[] view;
    private Camera camera;
    public int dist;

    private Player player;
    private int chunkCount = 0;
    private bool updatingChunks;
    private float maxchunkLoadTimeMillis = 5;

    private Queue<TerrainChunk> recyclable;
    private Queue<TerrainChunk> needUpdate;
    private Dictionary<Vector3, TerrainChunk> active;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        float foo = Time.time;
        shader = new RunShader(compute);
        shape = new TerrainShape();
        shape.shader = densities;
        chunks = new List<TerrainChunk>();
        needUpdate = new Queue<TerrainChunk>();
        recyclable = new Queue<TerrainChunk>();
        active = new Dictionary<Vector3, TerrainChunk>();

        Debug.Log(Time.realtimeSinceStartup - foo + " TIME");

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
        TerrainChunk chunk = new TerrainChunk(offset, shape, shader);
        active.Add(offset, chunk);
    }

    // Update is called once per frame
    void Update()
    {
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
                
                if (Mathf.Abs(displacement.x) >= 3 || Mathf.Abs(displacement.y) >= 3 || Mathf.Abs(displacement.z) >= 3)
                {
                    chunk.chunkObject.transform.Translate(deltaChunk * -5f * (TerrainChunk.SIZE-1));
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
                needUpdate.Dequeue().update();
            }
            //{
             //   break;
            //}
        //}
        

    }

}
