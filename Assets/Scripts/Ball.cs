using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour
{
    public float speed;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(new Vector3(speed, speed, 0));
    }

    public void Reset()
    {
        transform.position = new Vector3(0, 1, 0);
        rb.velocity = new Vector3(0 , 0, 0);
        rb.angularVelocity = new Vector3(0, 0, 0);
        rb.AddForce(new Vector3(speed, speed, 0));
    }

    public List<float> GetState()
    {
        Vector3 ballPos = rb.transform.position;
        Vector3 ballVel = rb.velocity;
        List<float> state = new List<float> {
            ballPos.x,
            ballPos.y,
            ballVel.x,
            ballVel.y,
        };
        return state;
    }

}

