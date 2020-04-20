using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GM : MonoBehaviour
{
    bool dead = false;
    public int score = 0;
    public int bricks = 63;
    public float resetDelay = 1f;
    public Text scoreText;
    public GameObject gameOver;
    public GameObject youWon;
    public GameObject bricksPrefab;
    public GameObject paddle;
    public GameObject deathParticles;
    public static GM instance = null;

    private GameObject clonePaddle;
    private Vector3 bricksPosition = new Vector3(-9.75f, 1f, 3.38f);

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        Setup();
    }

    void Setup()
    {
        clonePaddle = Instantiate(paddle, transform.position, Quaternion.identity) as GameObject;
        Instantiate(bricksPrefab, bricksPosition, Quaternion.identity);
    }

    void CheckGameOver()
    {
        if (score == 63)
        {
            youWon.SetActive(true);
            Time.timeScale = .25f;
            Invoke("Reset", resetDelay);
        }
        if (dead == true)
        {
            gameOver.SetActive(true);
            Time.timeScale = 1f;
            Invoke("Reset", resetDelay);
        }
    }

    void Reset()
    {
        Time.timeScale = 1f;
        Application.LoadLevel(Application.loadedLevel);

    }

    public void LoseLife()
    {
        dead = true;
        Instantiate(deathParticles, clonePaddle.transform.position, Quaternion.identity);
        Destroy(clonePaddle);
        Invoke("SetupPaddle", resetDelay);
        CheckGameOver();

    }

    public void SetupPaddle()
    {
        clonePaddle = Instantiate(paddle, transform.position, Quaternion.identity) as GameObject;
    }

    public void DestroyBrick()
    {
        score++;
        scoreText.text = "Score: " + score;
        bricks--;
        CheckGameOver();
    }
}
