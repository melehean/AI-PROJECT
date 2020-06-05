using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System.Threading;

public class SocketController : RunAbleThread
{
    public bool run_ = true;
    public ManagerGame game_manager_;

    public readonly object receive_data_lock_ = new object();
    public readonly object send_data_lock_ = new object();
    public AutoResetEvent something_to_send = new AutoResetEvent(false);


    protected override void Run()
    {
        ForceDotNet.Force();
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");

            while(run_ && Running)
            {
                something_to_send.WaitOne();
                float[] state;
                lock(send_data_lock_)
                {
                    state = game_manager_.GetStateThreadIndependent();
                }
                client.SendFrame( string.Join(" ",state));
                //Debug.Log(state);
                string message = null;
                bool got_message = false;
                while (Running)
                {
                    got_message = client.TryReceiveFrameString(out message);
                    if (got_message) break;
                }

                if (got_message)
                {
                    //Debug.Log("Received " + message);
                    lock(receive_data_lock_)
                    {
                        game_manager_.SetAction(float.Parse(message));
                    }
                }
            } 
        }

        NetMQConfig.Cleanup();
    }
}
