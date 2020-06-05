using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ManagerGame : MonoBehaviour
{
    public BrickManager brick_manager_;
    public Paddle paddle_;
    public Ball ball_;
    public int score_ = 0;
    public int lives_ = 1;
    public Text score_text_;
    public Text game_over_text_;
    public Text won_text_;
    public Text state_text_;
    public int prize_ = 0;
    public float action_ = -1;
    public float game_state_ = 0;
    public int episode_ = 0;

    public float[] state_;
    private SocketController socket_controller_;

    void Start()
    {
        socket_controller_ = new SocketController();
        socket_controller_.Start();
        socket_controller_.game_manager_ = this;
        Time.timeScale = 0f;
        game_over_text_.enabled = false;
        won_text_.enabled = false;
    }

    public float[] GetState()
    {
        List<float> state = new List<float>();
        state.Add(game_state_);
        state.Add(prize_);
        state.AddRange(ball_.GetState());
        state.AddRange(paddle_.GetState());
        state.AddRange(brick_manager_.GetState());
        return state.ToArray();
    }

    public float[] GetStateThreadIndependent()
    {
        prize_ = prize_ - (int)state_[1];
		return state_;
    }

    void Reset()
    {
        brick_manager_.Reset();
        paddle_.Reset();
        ball_.Reset();
        score_ = 0;
        lives_ = 1;
        Time.timeScale = 0f;
        action_ = -1;
        game_state_ = 0;
        episode_++;
    }

    public void SetAction(float action)
    {
        action_ = action;
    }
    
    void Update()
    {
        float action;
        lock(socket_controller_.receive_data_lock_)
        {
            action = action_;
        }
        if (action != -1)
        {
            Time.timeScale = 1f;
            game_over_text_.enabled = false;
            won_text_.enabled = false;
        }

        if(brick_manager_.GetActiveBrickCount() == 0)
        {
            Time.timeScale = 0f;
            action_ = -1;
            game_state_ = 1;
            won_text_.enabled = true;
	    lock(socket_controller_.send_data_lock_)
	    {
            	prize_ = 100;
	    }
        }

        switch (action)
        {
            case 0: paddle_.MovePaddle(0.0f); break;
            case 1: paddle_.MovePaddle(-1.0f); break;
            case 2: paddle_.MovePaddle(1.0f); break;
            case 3: Reset(); break;

        }

        lock(socket_controller_.send_data_lock_)
        {
            state_ = GetState();
            socket_controller_.something_to_send.Set();
        }

        var physics = state_.Skip(2).Take(5).Select(num => String.Format("{0:0.0}", num));
        string direction = state_.ElementAt(7) == 0 ? "none" : (state_.ElementAt(7) == 1 ? "right" : "left");
        var bricks = state_.Skip(8).Select((n, i) => String.Format("{1}{0}", n, i % brick_manager_.cols_ == 0 ? "\n" : ""));
        state_text_.text = "Episode: " + episode_
                    + "\nBall PosX: " + physics.ElementAt(0)
                    + "\nBall PosY: " + physics.ElementAt(1)
                    + "\nBall VelX: " + physics.ElementAt(2)
                    + "\nBall VelY: " + physics.ElementAt(3)
                    + "\nPaddle PosX: " + physics.ElementAt(4)
                    + "\nPaddle direction: " + direction
                    + "\nBrick Status:" + String.Join(" ", bricks);
    }

    public void DestroyBrick()
    {
        score_++;
	lock(socket_controller_.send_data_lock_)
	{
        	prize_ = prize_ + 1;
	}
        score_text_.text = "Score: " + score_;
       
    }

    public void Lose()
    {
        Time.timeScale = 0f;
        action_ = -1;
        lives_--;
        game_over_text_.enabled = true;
        game_state_ = -1;
	lock(socket_controller_.send_data_lock_)
	{
        	prize_ = -brick_manager_.GetActiveBrickCount();
	}
    }

    private void OnDestroy()
    {
        socket_controller_.Stop();
    }
}
