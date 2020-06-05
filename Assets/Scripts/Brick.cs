using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    public ManagerGame game_manager_;
    public GameObject brickParticle;

    void OnCollisionEnter (Collision other)
    {
        GameObject x =  Instantiate(brickParticle, transform.position, Quaternion.identity);
        game_manager_.DestroyBrick();
        gameObject.SetActive(false);
        Destroy(x, 5);
    }

}