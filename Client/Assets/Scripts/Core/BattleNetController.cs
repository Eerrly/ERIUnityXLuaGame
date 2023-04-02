using System;
using System.Collections.Generic;

public struct Head
{
    public byte act;
}

public struct Packet
{
    public Head head;
    public ByteBuffer data;
    public int length;
}

/// <summary>
/// 战斗网络执行器
/// </summary>
public class BattleNetController
{
    private UDPClient client;
    private object _sendLock = new object();
    private object _recvLock = new object();
    private byte[] _sendBuffer;

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

    public void Initialize() {
        _sendBuffer = new byte[6];
    }

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
    public void SendInputMsg(int frame, FrameBuffer.Input input)
    {
        try
        {
            ByteBuffer buffer = ByteBuffer.Allocate(5, true);
            buffer.Clear();
            buffer.WriteInt(frame);
            buffer.WriteByte(input.ToByte());
            Head head = new Head()
            { 
                act = NetConstant.FrameAct 
            };
            Packet packet = new Packet()
            {
                head = head,
                data = buffer,
                length = 6,
            };
            SendData2Server(packet);
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    /// <summary>
    /// 将准备数据发给服务器
    /// </summary>
    public void SendReadyMsg()
    {
        try
        {
            FrameBuffer.Input input = new FrameBuffer.Input((byte)BattleConstant.SelfID, 0);
            ByteBuffer buffer = ByteBuffer.Allocate(5, true);
            buffer.Clear();
            buffer.WriteInt(0);
            buffer.WriteByte(input.ToByte());
            Head head = new Head()
            {
                act = NetConstant.ReadyAct
            };
            Packet packet = new Packet()
            {
                head = head,
                data = buffer,
                length = 6,
            };
            SendData2Server(packet);
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }


    /// <summary>
    /// 将数据发给服务器
    /// </summary>
    /// <param name="frame">帧</param>
    /// <param name="input">操作</param>
    public void SendData2Server(Packet packet)
    {
        try
        {
            if (client != null && client.IsConnected)
            {
                unsafe
                {
                    var sendBuffer = _sendBuffer;
                    fixed (byte* dest = sendBuffer)
                    {
                        *(Head*)dest = packet.head;
                    }
                    var dataLen = packet.length;
                    Array.Copy(packet.data.ToArray(), 0, sendBuffer, 1, 5);
                    packet.data.Dispose();
                    lock (_sendLock)
                    {
                        client.Send(sendBuffer, 0, dataLen);
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
