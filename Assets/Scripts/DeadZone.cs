using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public ManagerGame game_manager_;

    void OnTriggerEnter()
    {
        game_manager_.Lose();
    }
}
