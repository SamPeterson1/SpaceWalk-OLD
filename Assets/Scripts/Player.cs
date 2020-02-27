using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float sensitivity;
    public float speed;

    public float jumpForce;

    float rotX = 0.0f;
    float rotY = 0.0f;

    bool rising = false;
    bool falling = false;

    Rigidbody body;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, 0, -speed * Time.deltaTime));
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        }

        if (Input.GetKeyDown(KeyCode.Space) && !(rising || falling)) 
        {
            body.AddForce(new Vector3(0, 500, 0));
            rising = true;
            falling = false;
        }

        if(rising && body.velocity.y < 0)
        {
            rising = false;
            falling = true;
        }

        Vector2 mousePos = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        rotX += -mousePos.y * sensitivity;
        rotY += mousePos.x * sensitivity;
        Vector3 rotation = new Vector3(rotX, rotY, 0);
        transform.rotation = Quaternion.Euler(rotation);

        Vector3 foo = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        foo.Scale(new Vector3(1 / 39f, 1 / 39f, 1 / 39f));
        Debug.Log((int)foo.x + " " + (int)foo.y + " " + (int)foo.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        falling = false;
    }

}
