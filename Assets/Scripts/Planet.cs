using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{

    public float gravity = -10f;
    public void Attract(Transform body)
    {

        Vector3 bodyUp = body.up;
        Vector3 targetDir = (body.position - transform.position).normalized;

        body.rotation = Quaternion.FromToRotation(bodyUp, targetDir) * body.rotation;
        Rigidbody rigidbody = body.gameObject.GetComponent<Rigidbody>();
        rigidbody.AddForce(gravity * targetDir);
    }
}
