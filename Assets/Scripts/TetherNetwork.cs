using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherNetwork : MonoBehaviour
{
    public Dictionary<Vector3, List<Tether>> tethers = new Dictionary<Vector3, List<Tether>>();
    public Dictionary<Vector3, List<Vector3>> unloadedTethers = new Dictionary<Vector3, List<Vector3>>();
    public GameObject tetherPrefab;
    public bool updated = false;

    public void AddTether(Tether tether, Vector3 chunkPos)
    {
        if(tethers.ContainsKey(chunkPos))
        {
            tethers.TryGetValue(chunkPos, out List<Tether> tethersInChunk);
            tethersInChunk.Add(tether);
        } else
        {
            List<Tether> tethersInChunk = new List<Tether>();
            tethersInChunk.Add(tether);
            tethers.Add(chunkPos, tethersInChunk);
        }
    }

    public void LoadTethersInChunk(Vector3 chunkPos)
    {
        if (!unloadedTethers.ContainsKey(chunkPos)) return;
        unloadedTethers.TryGetValue(chunkPos, out List<Vector3> tethersInChunk);
        foreach (Vector3 tether in tethersInChunk)
        {
            Instantiate(tetherPrefab, tether, Quaternion.identity);
        }
    }

    public void UnloadTethersInChunk(Vector3 chunkPos)
    {
        tethers.TryGetValue(chunkPos, out List<Tether> tethersInChunk);
        if (tethersInChunk != null)
        {
            List<Vector3> positions = new List<Vector3>();
            foreach (Tether tether in tethersInChunk)
            {
                positions.Add(tether.transform.position);
                Destroy(tether.gameObject);
            }

            if (unloadedTethers.ContainsKey(chunkPos))
            {
                unloadedTethers.Remove(chunkPos);
            }
            unloadedTethers.Add(chunkPos, positions);
        }
    }

}
