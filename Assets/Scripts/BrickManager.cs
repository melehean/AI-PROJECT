using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
    public int cols_, rows_;
    public float space_;
    public float start_x_, start_y_;
    public GameObject brick_prefab_;
    public ManagerGame game_manager_;

    private List<GameObject> bricks;

    private readonly List<Color> colors = new List<Color> {
        new Color(255f/255, 0f/255, 0f/255),
        new Color(255f/255, 138f/255, 0f/255),
        new Color(255f/255, 245f/255, 0f/255),
        new Color(4f/255, 255f/255, 0f/255),
        new Color(0f/255, 255f/255, 233f/255),
        new Color(0f/255, 58f/255, 255f/255),
        new Color(92f/255, 0f/255, 176f/255),
    };

    // Start is called before the first frame update
    void Start()
    {
        bricks = new List<GameObject>();
        Vector3 brickDim = brick_prefab_.transform.localScale;
        
        for (int y = 0; y < rows_; y++)
        {
            float yPos = (y - 0.5f * (rows_ - 1)) * (brickDim.z + space_);
            for (int x = 0; x < cols_; x++)
            {
                float xPos = (x - 0.5f * (cols_ - 1)) * (brickDim.x + space_);
                Vector3 pos = transform.position + new Vector3(start_x_ + xPos, start_y_ + yPos , 0);
                GameObject brick = Instantiate(brick_prefab_, pos, Quaternion.identity);
                brick.GetComponent<Brick>().game_manager_ = game_manager_;
                brick.GetComponent<MeshRenderer>().material.color = colors[rows_ - 1 - y];
                brick.transform.parent = this.transform;
                bricks.Add(brick);
            }
        }
    }

    public List<float> GetState()
    {
        List<float> state = new List<float>();
        bricks.ForEach((brick) => state.Add(brick.activeSelf ? 1.0f : 0.0f));
        return state;
    }

    public int GetActiveBrickCount()
    {
        return bricks.FindAll((brick) => brick.activeSelf).Count;
    }

    public void Reset()
    {
        bricks.ForEach((brick) => brick.SetActive(true));
    }
}
