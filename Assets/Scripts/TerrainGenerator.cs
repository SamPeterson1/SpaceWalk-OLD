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
    private Queue<TerrainChunk> needUpdate;

    private Player player;
    private int chunkCount = 0;
    private bool updatingChunks;
    private float maxchunkLoadTimeMillis = 5;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        float foo = Time.time;
        shader = new RunShader(compute);
        shape = new TerrainShape();
        chunks = new List<TerrainChunk>();
        needUpdate = new Queue<TerrainChunk>();

        for(int x = -4; x <= 4; x ++)
        {
            for(int y = -4; y <= 4; y ++)
            {
                for(int z = -4; z <= 4; z ++)
                {
                    genChunk(new Vector3(x*19, y*19, z*19) + new Vector3(0, 1030, 0));
                }
            }
        }

        Debug.Log(Time.realtimeSinceStartup - foo + " TIME");

        
    }



    void genChunk(Vector3 offset)
    {
        TerrainChunk chunk = new TerrainChunk(new Vector3Int(20, 20, 20), offset, shape, shader);
        chunks.Add(chunk);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q)) { 
            foreach (TerrainChunk chunk in chunks)
            {
                chunk.deform(player.transform.position, 20);
            }
        }

        if (player.changedChunks())
        {
            foreach(TerrainChunk chunk in chunks)
            {
                Vector3 chunkPos = chunk.CalculateChunkPos();
                Vector3 displacement = chunkPos - player.getChunkPosition();
                if(Mathf.Abs(displacement.x) >= 5 || Mathf.Abs(displacement.y) >= 5 || Mathf.Abs(displacement.z) >= 5)
                {
                    chunk.targetPositions.Enqueue(player.getDeltaChunk() * -9);
                    needUpdate.Enqueue(chunk);
                }
                
            }
        }

        if(needUpdate.Count > 0)
        {
            needUpdate.Dequeue().update();
        }
    }

}
