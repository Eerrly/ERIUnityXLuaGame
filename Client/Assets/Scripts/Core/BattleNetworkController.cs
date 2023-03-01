using System.Collections.Generic;

public class BattleNetworkController
{
    private UDPClient client;
    private object _sendLock = new object();
    private object _recvLock = new object();

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
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    public void DisConnect()
    {
        try
        {
            if(client != null)
            {
                client.DisConnect();
            }
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    public void SendInputToServer(int playerId, int frame, FrameBuffer.Input input)
    {
        try
        {
            if (client != null && client.IsConnected)
            {
                ByteBuffer buffer = ByteBuffer.Allocate(9, true);
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
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    public int RecvData(ref byte[] buffer, int index, int length)
    {
        try
        {
            if (client != null && client.IsConnected)
            {
                lock (_recvLock)
                {
                    return client.Recv(buffer, index, length);
                }
            }
            return 0;
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
        return 0;
    }

    public void Update()
    {
        if(client != null && client.IsConnected)
        {
            client.Update();
        }
    }

}
