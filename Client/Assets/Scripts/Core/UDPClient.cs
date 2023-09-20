using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class UDPClient
{
    private Socket socket;
    private EndPoint endPoint;
    private Queue<Packet> sendQueue;
    private Queue<Packet> recvQueue;
    private byte[] recvBuffer = new byte[1024];
    private byte[] sendBuffer;
    private short indexer;
    private bool error;

    public bool IsConnected => socket != null && socket.Connected;

    public void Initialize(int maxClientCount)
    {
        indexer = 0;
        error = false;
        sendQueue = new Queue<Packet>(maxClientCount);
        recvQueue = new Queue<Packet>(maxClientCount);
    }

    public void Connect(string ip, int port)
    {
        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(endPoint);
    }

    public void DisConnect()
    {
        if(socket != null && socket.Connected)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }

    public void Send(Packet packet)
    {
        lock (sendQueue)
        {
            sendQueue.Enqueue(packet);
        }
    }

    public Queue<Packet> Recv()
    {
        return recvQueue;
    }

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
                    if (packet.head.act == (int)ACT.DATA)
                    {
                        packet.head.index = indexer++;
                    }
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
                packet.data = BufferPool.GetBuffer(Head.EndPointLength + packet.head.size);
                Array.Copy(recvBuffer, Head.Length, packet.data, 0, Head.EndPointLength + packet.head.size);

                recvQueue.Enqueue(packet);
            }
        }
    }

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
