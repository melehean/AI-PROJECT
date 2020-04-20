using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArkanoidAgent : Agent
{
    public GameObject ball;
    Vector3 ballStartPos;

    public float paddleSpeed = 0.5f;
    private Vector3 agentPos = new Vector3(0, 0, 0);
    float xPos;
    void Start()
    {
        ballStartPos = ball.transform.position;
    }

    public override void AgentStep(float[] act)
    {
        if (brain.brainParameters.actionSpaceType == StateType.continuous)
        {
            // gameObject.transform.Rotate(new Vector3(0, 0, 1), act[0]);
            // gameObject.transform.Rotate(new Vector3(1, 0, 0), act[1]);
            xPos = transform.position.x + (-act[0] * paddleSpeed);
            agentPos = new Vector3(Mathf.Clamp(xPos, -8f, 8f), 0f, 0f);
            transform.position = agentPos;

            if (done == false)
            {
                reward = 0.1f;
            }
        }
        else
        {
            int action = (int)act[0];
            Debug.Log(action);
            if (action == 0 || action == 1)
            {
                if (action == 0)
                {
                    //gameObject.transform.Rotate(new Vector3(0, 0, -1), 2);
                    xPos = transform.position.x + (-act[0] * paddleSpeed);
                    agentPos = new Vector3(Mathf.Clamp(xPos, -8f, 8f), 0f, 0f);
                    transform.position = agentPos;
                }
                else
                {
                    //gameObject.transform.Rotate(new Vector3(0, 0, 1), 2);
                    xPos = transform.position.x + (-act[0] * paddleSpeed);
                    agentPos = new Vector3(Mathf.Clamp(xPos, -8f, 8f), 0f, 0f);
                    transform.position = agentPos;
                }
            }
            if (action == 2 || action == 3)
            {
                if (action == 2)
                {
                    //         gameObject.transform.Rotate(new Vector3(-1, 0, 0), 2);
                }
                else
                {
                    //       gameObject.transform.Rotate(new Vector3(1, 0, 0), 2);
                }
            }
            if (done == false)
            {
                reward = 0.1f;
            }
        }

        if (ball.transform.position.y < -2f)
        {
            GM.instance.LoseLife();
            done = true;
            reward = -1f;
        }
    }

    public override List<float> CollectState()
    {
        List<float> state = new List<float>();
        state.Add(gameObject.transform.rotation.z);
        state.Add(gameObject.transform.rotation.x);
        state.Add((ball.transform.position.x - gameObject.transform.position.x));
        state.Add((ball.transform.position.y - gameObject.transform.position.y));
        state.Add((ball.transform.position.z - gameObject.transform.position.z));
        state.Add(ball.transform.GetComponent<Rigidbody>().velocity.x);
        state.Add(ball.transform.GetComponent<Rigidbody>().velocity.y);
        state.Add(ball.transform.GetComponent<Rigidbody>().velocity.z);
        return state;
    }

    public override void AgentReset()
    {
        gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        ball.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = ballStartPos;
    }
}
