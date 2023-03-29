using System.Collections.Generic;

/// <summary>
/// 战斗网络执行器
/// </summary>
public class BattleNetController
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

    public void Initialize() { }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口号</param>
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

    /// <summary>
    /// 断开连接
    /// </summary>
    public void DisConnect()
    {
        try
        {
            if(client != null && client.IsConnected)
            {
                client.DisConnect();
            }
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    /// <summary>
    /// 将操作数据发给服务器
    /// </summary>
    /// <param name="frame">帧</param>
    /// <param name="input">操作</param>
    public void SendInputToServer(int frame, FrameBuffer.Input input)
    {
        try
        {
            if (client != null && client.IsConnected)
            {
                ByteBuffer buffer = ByteBuffer.Allocate(5, true);
                lock (_sendLock)
                {
                    buffer.Clear();
                    buffer.WriteInt(frame);
                    buffer.WriteByte(input.ToByte());
                    if (client != null)
                    {
                        client.Send(buffer.ToArray(), 0, 5);
                    }
                }
            }
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    /// <summary>
    /// 接受数据
    /// </summary>
    /// <param name="buffer">数据</param>
    /// <param name="index">起始索引</param>
    /// <param name="length">长度</param>
    /// <returns></returns>
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

    /// <summary>
    /// 轮询
    /// </summary>
    public void Update()
    {
        if(client != null && client.IsConnected)
        {
            client.Update();
        }
    }

}
