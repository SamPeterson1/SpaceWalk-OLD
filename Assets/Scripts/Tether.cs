using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class Tether : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody body;
    public static readonly float RANGE = 10f;
    public Tether supplier = null;
    public GameObject connectionPrefab;
    private TetherConnection tetherConnection;
    private TetherNetwork network;
    public bool hasOxygen = false;
    public bool isRoot = false;
    public bool firstTether = false;

    void Start()
    {
        network = GameObject.FindGameObjectWithTag("Planet").GetComponent<TetherNetwork>();
        body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        
        if (isRoot || network.tethers.Count == 0)
        {
            hasOxygen = true;
            firstTether = true;
            supplier = this;
        }
        //TetherNetwork.AddTether(this);
    }

    private void OnDestroy()
    {
        
    }

    void updateHasOxygen()
    {
        if (!firstTether && (tetherConnection == null || !tetherConnection.enabled))
        {
            Tether closest = FindClosestOxygenatedTether(!hasOxygen);
            if (closest != null)
            {
                if (tetherConnection == null)
                {
                    tetherConnection = Instantiate(connectionPrefab).GetComponent<TetherConnection>();
                }
                tetherConnection.tetherA = gameObject;
                tetherConnection.tetherB = closest.gameObject;
                supplier = closest;
            }
        }

        if(supplier == null)
        {
            hasOxygen = false;
            return;
        }
        if(!inRange(supplier.transform))
        {
            hasOxygen = false;
        } else
        {
            hasOxygen = supplier.hasOxygen;
        }
    }

    void Update()
    { 
        RaycastHit hit;
        if (body.constraints == RigidbodyConstraints.FreezeAll && Physics.Raycast(transform.position, -transform.position, out hit, 1.2f))
        {
            Vector3 localUp = transform.up;
            Vector3 target = hit.normal;
            transform.rotation *= Quaternion.FromToRotation(localUp, target);
        } else
        {
            body.constraints = RigidbodyConstraints.None;
        }
        
        updateHasOxygen();
    }

    public Tether FindClosestOxygenatedTether(bool oxygenated)
    {
        Tether closest = null;
        float minDist = -1;
        foreach (List<Tether> tethers in network.tethers.Values)
        {
            foreach (Tether tether in tethers)
            {
                if (tether != this && tether.inRange(transform) && tether.hasOxygen == oxygenated)
                {
                    float dist = (tether.transform.position - transform.position).magnitude;
                    if (minDist == -1 || dist < minDist)
                    {
                        minDist = dist;
                        closest = tether;
                    }
                }
            }
        }

        return closest;
    }
    public bool inRange(Transform other)
    {
        Vector3 pos = other.position;
        return (transform.position - pos).magnitude < RANGE;
    }

    private void OnCollisionExit(Collision collision)
    {
        body.constraints = RigidbodyConstraints.None;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.tag.Equals("Chunk"))
        {
            body.constraints = RigidbodyConstraints.FreezeAll;
            
        } else
        {
            body.constraints = RigidbodyConstraints.None;
        }
        
    }
}
