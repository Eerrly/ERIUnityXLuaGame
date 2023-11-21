using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// UDP客户端
/// </summary>
public class UDPClient
{
    private Socket socket;
    private EndPoint endPoint;
    /// <summary>
    /// 发送队列
    /// </summary>
    private Queue<Packet> sendQueue;
    /// <summary>
    /// 接受队列
    /// </summary>
    private Queue<Packet> recvQueue;
    private byte[] recvBuffer = new byte[1024];
    private byte[] sendBuffer;
    /// <summary>
    /// 序号
    /// </summary>
    private short indexer;

    /// <summary>
    /// 是否连接
    /// </summary>
    public bool IsConnected => socket != null && socket.Connected;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="maxClientCount">最大客户端连接数量</param>
    public void Initialize(int maxClientCount)
    {
        indexer = 0;
        sendQueue = new Queue<Packet>(maxClientCount);
        recvQueue = new Queue<Packet>(maxClientCount);
    }

    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口号</param>
    public void Connect(string ip, int port)
    {
        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(endPoint);
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void DisConnect()
    {
        if(socket != null && socket.Connected)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="packet">包体</param>
    public void Send(Packet packet)
    {
        lock (sendQueue)
        {
            sendQueue.Enqueue(packet);
        }
    }

    /// <summary>
    /// 接受
    /// </summary>
    /// <returns>包体队列</returns>
    public Queue<Packet> Recv()
    {
        return recvQueue;
    }

    /// <summary>
    /// 发送数据具体逻辑
    /// </summary>
    private void SendMethod()
    {
        while (sendQueue.Count > 0)
        {
            lock (sendQueue)
            {
                var packet = sendQueue.Dequeue();
                sendBuffer = BufferPool.GetBuffer(Head.Length + packet.head.size);
                unsafe
                {
                    fixed (byte* dest = sendBuffer)
                    {
                        *(Head*)dest = packet.head;
                    }
                }
                Array.Copy(packet.data, 0, sendBuffer, Head.Length, packet.head.size);
                socket.Send(sendBuffer, 0, sendBuffer.Length, SocketFlags.None);

                BufferPool.ReleaseBuff(sendBuffer);
            }
        }
    }

    /// <summary>
    /// 接受数据具体逻辑
    /// </summary>
    private void RecvMethod()
    {
        if (socket.Available > 0 && socket.Receive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None) > 0)
        {
            lock (recvQueue)
            {
                Packet packet = new Packet();
                unsafe
                {
                    fixed (byte* dest = recvBuffer)
                    {
                        packet.head = *(Head*)dest;
                    }
                }
                packet.data = BufferPool.GetBuffer(packet.head.size);
                Array.Copy(recvBuffer, Head.Length, packet.data, 0, packet.head.size);

                recvQueue.Enqueue(packet);
            }
        }
    }

    /// <summary>
    /// 线程轮询
    /// </summary>
    public void Update()
    {
        if (socket == null || !socket.Connected)
            return;

        try
        {
            SendMethod();
            RecvMethod();
        }
        catch(Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }

    }

}
