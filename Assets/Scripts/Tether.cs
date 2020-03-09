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
    public static List<Tether> disconnectedTethers;
    public static List<Tether> connectedTethers;
    public static List<Tether> tethers = new List<Tether>();
    private TetherConnection tetherConnection;
    public bool hasOxygen = false;
    public bool isRoot = false;
    public bool firstTether = false;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        /*
        foreach(TetherTree tree in tetherTrees.Values)
        {
            tree.AddToTree(this);
        }
        */
        if(tethers == null)
        {
            Debug.Log("OOF");
            tethers = new List<Tether>();
        }
        /*
        if(connectedTethers == null)
        {
            connectedTethers = new List<Tether>();
            disconnectedTethers = new List<Tether>();
        }
        */

        
        if (isRoot || tethers.Count == 0)
        {
            firstTether = true;
            supplier = this;
        }
        tethers.Add(this);
    }
 

    /*
    public Tether ClosestTetherNoRecursion()
    {
        Tether closest = null;
        float minDist = -1;
        foreach (Tether tether in tethers)
        {
            if (tether != this && tether.inRange(transform))
            {
                float dist = (tether.transform.position - transform.position).magnitude;
                if (minDist == -1 || dist < minDist)
                {
                    minDist = dist;
                    closest = tether;
                }
            }
        }

        return closest;
    }
    */

    /*
    public Tether ClosestTether()
    {
        Tether closest = null;
        float minDist = -1;
        foreach (Tether tether in tethers)
        {
            if (tether != this && tether.inRange(transform) && tether.supplier != this)
            {
                float dist = (tether.transform.position - transform.position).magnitude;
                if (minDist == -1 || dist < minDist)
                {
                    minDist = dist;
                    closest = tether;
                }
            }
        }

        return closest;
    }
    */

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

    /*
    bool TryConnectDisconnected()
    {
        bool retVal = false;

        Tether closest = FindClosestDisconnectedTether();
        while(closest != null)
        {
            connectedTethers.Add(closest);
            disconnectedTethers.Remove(closest);
            GameObject newConnection = Instantiate(connectionPrefab);
            TetherConnection connection = newConnection.GetComponent<TetherConnection>();
            tetherConnection = connection;
            connection.tetherA = gameObject;
            connection.tetherB = closest.gameObject;
            closest.supplier = this;
            closest.TryConnectDisconnected();
            retVal = true;
            closest = FindClosestDisconnectedTether();
        }
        return retVal;
    }
    */
    // Update is called once per frame
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

    /*
    public Tether FindClosestDisconnectedTether()
    {
        Tether closest = null;
        float minDist = -1;
        foreach (Tether tether in disconnectedTethers)
        {
            if (tether != this && tether.inRange(transform))
            {
                float dist = (tether.transform.position - transform.position).magnitude;
                if (minDist == -1 || dist < minDist)
                {
                    minDist = dist;
                    closest = tether;
                }
            }
        }

        return closest;
    }
    */

    public Tether FindClosestOxygenatedTether(bool oxygenated)
    {
        Tether closest = null;
        float minDist = -1;
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

        return closest;
    }

    /*
    public Tether FindClosestTether()
    {
        Tether closest = null;
        float minDist = -1;
        foreach(Tether tether in connectedTethers)
        {
            if (tether != this && tether.inRange(transform))
            {
                float dist = (tether.transform.position - transform.position).magnitude;
                if (minDist == -1 || dist < minDist)
                {
                    minDist = dist;
                    closest = tether;
                }
            }
        }

        return closest;
    }
    */
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
