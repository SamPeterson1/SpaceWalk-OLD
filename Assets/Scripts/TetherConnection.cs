using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherConnection : MonoBehaviour
{
    public GameObject tetherA;
    public GameObject tetherB;
    private LineRenderer renderer;
    public bool enabled;
    void Start()
    {
        renderer = GetComponent<LineRenderer>();
        renderer.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        renderer.SetPosition(0, tetherA.transform.position);
        renderer.SetPosition(1, tetherB.transform.position);
        if (!tetherA.GetComponent<Tether>().inRange(tetherB.GetComponent<Tether>().transform))
        {
            renderer.enabled = false;
            enabled = false;
        } else
        {
            renderer.enabled = true;
            enabled = true;
        }
    }
}
