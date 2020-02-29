using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    Planet planet;
    Rigidbody body;
    // Start is called before the first frame update
    void Awake()
    {
        planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<Planet>();
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        planet.Attract(transform);
    }
}
