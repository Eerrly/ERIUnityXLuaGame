﻿using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 行为
/// </summary>
enum ACT
{
    HEARTBEAT,
    DATA,
    JOIN,
}

/// <summary>
/// 头数据
/// </summary>
public struct Head
{
    public int size;
    public byte act;
    public short index;

    public static readonly int Length = 8;
    public static readonly int EndPointLength = 16;
}

/// <summary>
/// 包体
/// </summary>
public struct Packet
{
    public Head head;
    public byte[] data;
}

/// <summary>
/// 战斗网络执行器
/// </summary>
public class BattleNetController
{
    private UDPClient client;

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
                client.Initialize(BattleConstant.MaxClientCount);
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
            client.DisConnect();
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
            byte[] buffer = BufferPool.GetBuffer(5);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(input.ToByte());
                writer.Write(frame);
                writer.Dispose();
            }
            Head head = new Head()
            {
                act = (byte)ACT.DATA,
                size = 5,
                index = 0,
            };
            Packet packet = new Packet()
            {
                head = head,
                data = buffer,
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

            byte[] buffer = BufferPool.GetBuffer(5);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(input.ToByte());
                writer.Write(0);
                writer.Dispose();
            }
            Head head = new Head()
            {
                act = (byte)ACT.JOIN,
                size = 5,
                index = 0,
            };
            Packet packet = new Packet()
            {
                head = head,
                data = buffer,
            };
            SendData2Server(packet);
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    public void SendHeartBeatMsg()
    {
        try
        {
            byte[] buffer = BufferPool.GetBuffer(5);
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write((byte)0);
                writer.Write(0);
                writer.Dispose();
            }
            Head head = new Head()
            {
                act = (byte)ACT.HEARTBEAT,
                size = 5,
                index = 0,
            };
            Packet packet = new Packet()
            {
                head = head,
                data = buffer,
            };
            SendData2Server(packet);
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    public void SendData2Server(Packet packet)
    {
        try
        {
            client.Send(packet);
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
    }

    public Queue<Packet> RecvData()
    {
        return client.Recv();
    }

    public void Update()
    {
        client.Update();
    }

}
