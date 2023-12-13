using System;
using System.Collections.Generic;
using System.IO;
using KCPNet;

/// <summary>
/// 接收到的数据
/// </summary>
public class RecvData
{
    public byte[] data;

    public int length;

    public int cmd;
    
    public long recvTime;
}

/// <summary>
/// 战斗网络执行器
/// </summary>
public class BattleNetController
{
    public int MinPing = 0;
    public int Ping = 0;

    private KcpClient _socketClient;
    
    private MemoryStream _sendStream;
    private MemoryStream _receiveStream;
    private object _sendLock;
    private BinaryReader _binaryReader;
    private BinaryWriter _binaryWriter;
    
    private RecvData _recvData;
    private List<Packet> _netData;

    /// <summary>
    /// 是否连接
    /// </summary>
    public bool IsConnected => _socketClient != null && _socketClient.IsConnected;
    
    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void DoConnect(string ip, int port)
    {
        if (_socketClient == null) _socketClient = new KcpClient(0);
        _socketClient.Connect(ip, port);
    }

    /// <summary>
    /// 断开服务器
    /// </summary>
    public void DoDisconnect()
    {
        if(_socketClient != null && _socketClient.IsConnected) _socketClient.Disconnect();
    }

    /// <summary>
    /// 发送数据给服务器
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="cmd">命令</param>
    /// <param name="act">动作</param>
    /// <param name="length">数据长度</param>
    private void SendDataToRealServer(byte[] data, int cmd, int act, int length)
    {
        lock (_sendLock)
        {
            var packet = new Packet();
            var head = new Head
            {
                cmd = (byte)cmd,
                act = (byte)act,
                length = (uint)length
            };
            packet.Head = head;
            packet.Data = data;

            if (_socketClient != null) _socketClient.Send(packet);
        }
    }

    /// <summary>
    /// 发送数据给服务器
    /// </summary>
    /// <param name="stream">数据流</param>
    /// <param name="cmd">命令</param>
    /// <param name="act">动作</param>
    public void SendToServer(MemoryStream stream, int cmd, int act)
    {
        var length = (int)stream.Position;
        if (length <= 0)
        {
            Logger.Log(LogLevel.Error, $"发送数据长度错误 长度:{length}");
            return;
        }

        var result = BufferPool.GetBuffer(length);
        stream.Seek(0, SeekOrigin.Begin);
        var _ = stream.Read(result, 0, length);
        stream.Reset();
        
        SendDataToRealServer(result, cmd, act, length);
    }

    /// <summary>
    /// 接受数据的回调
    /// </summary>
    /// <param name="recv"></param>
    public void RecvCallback(RecvData recv)
    {
        if (recv == null) return;
        
        _receiveStream.Reset();
        _receiveStream.Write(recv.data, 0, recv.length);
        _receiveStream.Seek(0, SeekOrigin.Begin);

        if (recv.cmd == NetConstant.pvpFrameType)
        {
            while (_binaryReader.BaseStream.Position < _binaryReader.BaseStream.Length)
            {
                var frameBuffer = BattleManager.Instance.battle.frameBuffer;
                var inputFrame = FrameBuffer.Frame.defFrame;
                inputFrame.playerCount = BattleManager.Instance.BattleClientData.players.Length;
                inputFrame.frame = _binaryReader.ReadInt32();
                
                if (inputFrame.frame != 0)
                {
                    for (var i = 0; i < inputFrame.playerCount; i++)
                    {
                        var input = inputFrame[i];
                        input.pos = 7;
                        inputFrame[i] = input;
                    }
                }
                var playerInputCnt = _binaryReader.ReadByte();
                for (var i = 0; i < playerInputCnt; i++)
                {
                    var input = new FrameBuffer.Input(_binaryReader.ReadByte());
                    inputFrame[input.pos] = input;
                }

                var diff = 0;
                while (!frameBuffer.SyncFrame(inputFrame.frame, ref inputFrame, ref diff))
                {
                    Logger.Log(LogLevel.Error, $"无法同步服务器返回的帧数据 [frame]->{inputFrame.frame} [diff]->{diff}");
                    DoDisconnect();
                    break;
                }
                BattleManager.Instance.AsyncServerFrame.Add(inputFrame.frame);
                BattleManager.Instance.SyncClientServerOffsetTime(inputFrame.frame);
                Logger.Log(LogLevel.Info, $"接受到服务器返回的帧数据 已同步 [frame]->{inputFrame.frame} [y0]->{inputFrame[0].yaw}");
            }
        }
        else if (recv.cmd == NetConstant.pvpPingType)
        {
            Ping = (int)(recv.recvTime - _binaryReader.ReadInt64());
            if (MinPing == 0) MinPing = Ping;
            MinPing = Math.Min(Ping, MinPing);
            Logger.Log(LogLevel.Info, $"接受到服务器返回的Ping数据 [ping]->{Ping}");
        }
        else if (recv.cmd == NetConstant.pvpReadyType)
        {
            BattleManager.Instance.battle.battleEntity.Frame = 0;
            BattleManager.Instance.battleStarted = true;
            Logger.Log(LogLevel.Info, $"接受到服务器返回的准备数据");
        }
    }

    /// <summary>
    /// 接受数据的处理逻辑
    /// </summary>
    private void HandleNetData()
    {
        _socketClient.Recive(_netData);
        foreach (var t in _netData)
        {
            _recvData.cmd = (t.Head.cmd << 8) + t.Head.act;
            _recvData.data = t.Data;
            _recvData.length = t.Length;
            _recvData.recvTime = t.RecvTime;
            try
            {
                RecvCallback(_recvData);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Exception, $"处理服务器返回的数据发生异常 [index]->{t.Head.index} [data]->{Util.DebugBytes(_recvData.data, (uint)_recvData.length)}");
            }
            BufferPool.ReleaseBuff(t.Data);
        }
        _netData.Clear();
    }

    /// <summary>
    /// 尝试接受数据
    /// </summary>
    public void TryRecivePackages()
    {
        if(!IsConnected) return;
        HandleNetData();
    }

    /// <summary>
    /// 尝试轮询
    /// </summary>
    public void TryUpdate()
    {
        if(!IsConnected) return;

        _socketClient.Update();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        _sendLock = new object();
        _sendStream = new MemoryStream(1024);
        _receiveStream = new MemoryStream(1024);
        _binaryReader = new BinaryReader(_receiveStream);
        _binaryWriter = new BinaryWriter(_sendStream);
        _netData = new List<Packet>();
        _recvData = new RecvData();
    }

    /// <summary>
    /// 发送准备消息
    /// </summary>
    /// <param name="frame">帧号</param>
    /// <param name="playerId">玩家ID</param>
    public void SendReadyMsg(int frame, byte playerId)
    {
        if(!IsConnected) return;

        lock (_sendLock)
        {
            _binaryWriter.Seek(0, SeekOrigin.Begin);
            _binaryWriter.Write(frame);
            _binaryWriter.Write(playerId);
            SendToServer(_sendStream, NetConstant.pvpFrameCmd, NetConstant.pvpReadyAct);
        }
    }

    /// <summary>
    /// 发送心跳消息
    /// </summary>
    public void SendPingMsg()
    {
        if(!IsConnected) return;

        lock (_sendLock)
        {
            _binaryWriter.Seek(0, SeekOrigin.Begin);
            _binaryWriter.Write(_socketClient.Time);
            SendToServer(_sendStream, NetConstant.pvpFrameCmd, NetConstant.pvpPingAct);
        }
    }

    /// <summary>
    /// 发送玩家操作数据
    /// </summary>
    /// <param name="frame">帧号</param>
    /// <param name="input">输入</param>
    public void SendInputMsg(int frame, FrameBuffer.Input input)
    {
        if(!IsConnected) return;

        lock (_sendLock)
        {
            _binaryWriter.Seek(0, SeekOrigin.Begin);
            _binaryWriter.Write(frame);
            _binaryWriter.Write(input.ToByte());
            SendToServer(_sendStream, NetConstant.pvpFrameCmd, NetConstant.pvpFrameAct);
        }
    }

}
