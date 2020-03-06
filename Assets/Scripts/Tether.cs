using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class Tether : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody body;
    void Start()
    {
        body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }

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
