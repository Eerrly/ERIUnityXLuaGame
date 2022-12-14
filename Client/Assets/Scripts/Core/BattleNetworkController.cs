using System.Collections.Generic;

public class BattleNetworkController
{
    private UDPClient client;
    private object _sendLock = new object();

    public bool IsConnected
    {
        get
        {
            if (client != null && client.IsConnected)
            {
                return true;
            }
            return false;
        }
    }

    private static BattleNetworkController _instance = null;
    public static BattleNetworkController Instance => _instance;

    public void Initialize()
    {
        _instance = this;
    }

    public void Connect(string ip, int port)
    {
        try
        {
            if (client == null)
            {
                client = new UDPClient();
                client.AckNoDelay = true;
                client.WriteDelay = false;
            }
            client.Connect(ip, port);
        }
        catch(System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }

    public void SendInputToServer(int playerId, int frame, FrameBuffer.Input input)
    {
        try
        {
            if (client != null && client.IsConnected)
            {
                ByteBuffer buffer = ByteBuffer.Allocate(8, true);
                lock (_sendLock)
                {
                    buffer.Clear();
                    buffer.WriteInt(playerId);
                    buffer.WriteInt(frame);
                    buffer.WriteByte(input.ToByte());
                    if (client != null)
                    {
                        var result = buffer.ToArray();
                        client.Send(result, 0, result.Length);
                    }
                }
            }
        }
        catch(System.Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }

    public void RecvData(ref byte[] buffer, int index, int length)
    {
        if (client != null && client.IsConnected)
        {
            client.Recv(buffer, index, length);
        }
    }

    public void Update()
    {
        if(client != null && client.IsConnected)
        {
            client.Update();
        }
    }

}
