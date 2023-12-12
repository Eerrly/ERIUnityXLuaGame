using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace KCPNet
{
    public class CacheUnit
    {
        public byte[] data;
        public int length;
    }
    
    public class KcpClient
    {
        public bool IsConnected => _client != null && _client.Connected && _kcp != null && _kcp.State != 0xFFFFFF;
        
        private EndPoint _endPoint;
        
        private Queue<Packet> _sendQueue = new Queue<Packet>();
        private SwitchQueue<Packet> _recvQueue = new SwitchQueue<Packet>();

        private object _sendLock = new object();
        private byte[] _sendBuffer = new byte[Head.MaxSendLength];
        private byte[] _recvBuffer = new byte[Head.MaxRecvLength];

        private Socket _client;
        private KCP _kcp;
        /// <summary>
        /// 会话ID
        /// </summary>
        private uint _conv = 0;
        /// <summary>
        /// 序号
        /// </summary>
        private ushort _index = 0;
        
        private int _usedRecvBufferSize = 0;
        
        /// <summary>
        /// KCP发送窗口大小
        /// </summary>
        private const int kSndWnd = 512;
        private Queue<CacheUnit> _finalSendQueue = new Queue<CacheUnit>();
        
        public KcpClient(uint conv)
        {
            _conv = conv;
            ResetData();
        }
        
        private static readonly DateTime m_UtcTime = new DateTime(1970, 1, 1);
        private static UInt32 iclock()
        {
            return (UInt32)(Convert.ToInt64(DateTime.UtcNow.Subtract(m_UtcTime).TotalMilliseconds) & 0xffffffff);
        }
        
        private Stopwatch _watch;
        public long Time
        {
            get
            {
                if (_watch == null)
                {
                    _watch = new Stopwatch();
                    _watch.Start();
                }
                return _watch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// 客户端链接
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        public void Connect(string ip, int port)
        {
            ResetData();
            if (UDP_Connect(ip, port))
            {
                Kcp_Init(_conv);
            }
        }

        /// <summary>
        /// 客户端发送数据
        /// </summary>
        /// <param name="packet"></param>
        public void Send(Packet packet)
        {
            if (_client == null || !_client.Connected || _kcp == null || _sendQueue.Count >= kSndWnd) return;
            
            lock (_sendLock)
            {
                _sendQueue.Enqueue(packet);
            }
        }

        /// <summary>
        /// 客户端接受数据
        /// </summary>
        /// <param name="packets"></param>
        public void Recive(List<Packet> packets)
        {
            if(!_client.Connected) return;
            
            _recvQueue.Switch();
            packets.Clear();
            packets.Capacity = Math.Max(packets.Capacity, _recvQueue.Count);

            while (_recvQueue.Count > 0) packets.Add(_recvQueue.Pop());
        }

        /// <summary>
        /// 网络轮询
        /// </summary>
        public void Update()
        {
            if (_client == null || !_client.Connected || _kcp == null) return;
            
            lock (_sendLock)
            {
                while (_sendQueue.Count > 0 && _kcp != null && _kcp.WaitSnd() < kSndWnd)
                {
                    var packet = _sendQueue.Dequeue();
                    unsafe
                    {
                        var length = Head.Length + (int)packet.Head.length;
                        var sendBuffer = _sendBuffer;
                        fixed (byte* dest = sendBuffer)
                        {
                            packet.Head.index = _index++;
                            *((Head*)dest) = packet.Head;
                        }
                        Array.Copy(packet.Data, 0, sendBuffer, Head.Length, packet.Head.length);
                        BufferPool.ReleaseBuff(packet.Data);
                        
                        _kcp.Send(sendBuffer, 0, length);
                    }
                }
            }

            var needFlush = false;
            while (_finalSendQueue.Count > 0)
            {
                CacheUnit unit = null;
                lock (_sendLock)
                {
                    unit = _finalSendQueue.Dequeue();
                }

                try
                {
                    needFlush = true;
                    _client.Send(unit.data, 0, unit.length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Exception, $"发送数据发生异常 错误:{ex.Message}");
                }
                finally
                {
                    if (unit != null)
                    {
                        BufferPool.ReleaseBuff(unit.data);
                        unit = null;
                    }
                }
            }

            _kcp.Update(iclock(), needFlush);

            var size = Head.MaxRecvLength - _usedRecvBufferSize;
            if (size > 0)
            {
                var error = string.Empty;
                try
                {
                    var length = _client.ReceiveFrom(_recvBuffer, _usedRecvBufferSize, size, SocketFlags.None, ref _endPoint);
                    if (length > 0)
                    {
                        _usedRecvBufferSize += length;
                        SplitPackets();
                    }
                    else if (length < 0)
                    {
                        error = $"接收数据发生异常 接受数据小于0, 长度:{length}";
                    }
                }
                catch (SocketException ex)
                {
                    if(ex.ErrorCode != (int)SocketError.WouldBlock) error = $"接收数据发生异常 错误码:{ex.ErrorCode} 错误:{ex.Message}";
                }
                catch (Exception ex)
                {
                    error = $"接收数据发生异常 错误:{ex.Message}";
                }

                if (error != string.Empty)
                {
                    Logger.Log(LogLevel.Exception, error);
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// 从RecvBuffer中拆分出多个Packets,不足一个Packet的部分,存留在Buffer中留待下次拆分
        /// </summary>
        private void SplitPackets()
        {
            try
            {
                var offset = 0;
                var result = _kcp.Input(_recvBuffer, ref offset, _usedRecvBufferSize);
                if (offset == 0 || result == -1) return;
                _usedRecvBufferSize = _usedRecvBufferSize - offset;
                if (_usedRecvBufferSize > 0)
                {
                    Array.Copy(_recvBuffer, offset, _recvBuffer, 0, _usedRecvBufferSize);
                }

                var data = BufferPool.GetBuffer(1024);
                for (var size = _kcp.PeekSize(); size > 0; size = _kcp.PeekSize())
                {
                    if (data.Length < size)
                    {
                        BufferPool.ReleaseBuff(data);
                        data = BufferPool.GetBuffer(size);
                    }

                    if (_kcp.Recv(data) > 0)
                    {
                        ParsePacket(data, size);
                    }
                }
                BufferPool.ReleaseBuff(data);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Exception, $"拆分数据包发生异常 错误:{ex.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="length">数据长度</param>
        private void ParsePacket(byte[] data, int length)
        {
            var packet = new Packet();
            unsafe
            {
                fixed (byte* src = data)
                {
                    packet.Head = *((Head*)src);
                }
            }
            if(length != packet.Head.length + Head.Length)
            {
                Logger.Log(LogLevel.Error, $"接收数据长度不匹配 数据长度:{length} 目标长度:{packet.Head.length + Head.Length}");
                return;
            }

            packet.Data = BufferPool.GetBuffer((int)packet.Head.length);
            packet.Length = (int)packet.Head.length;
            Array.Copy(data, Head.Length, packet.Data, 0, (int)packet.Head.length);

            packet.RecvTime = Time;
            
            _recvQueue.Push(packet);
        }
        
        /// <summary>
        /// 客户端KCP发送数据
        /// </summary>
        /// <param name="data">数据体</param>
        /// <param name="size">数据长度</param>
        private void Kcp_Send(byte[] data, int size)
        {
            var unit = new CacheUnit
            {
                data = BufferPool.GetBuffer(size),
                length = size
            };
            Array.Copy(data, unit.data, size);

            lock (_sendLock)
            {
                _finalSendQueue.Enqueue(unit);
            }
        }
        
        /// <summary>
        /// 客户端KCP初始化
        /// </summary>
        /// <param name="conv">会话ID</param>
        private void Kcp_Init(uint conv)
        {
            if (_kcp != null)
            {
                _kcp.Reset(conv, Kcp_Send);
            }
            else
            {
                _kcp = new KCP(conv, Kcp_Send);
            }
            _kcp.NoDelay(1, 1, 2, 1); //快速模式
            _kcp.WndSize(kSndWnd, kSndWnd);
        }

        /// <summary>
        /// 客户端UDP链接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool UDP_Connect(string ip, int port)
        {
            try
            {
                if (_client != null && _client.Connected)
                {
                    Disconnect();
                }

                _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _client.SendBufferSize = Head.MaxSendLength;
                _client.ReceiveBufferSize = Head.MaxRecvLength;
                if (!System.Net.IPAddress.TryParse(ip, out var address))
                {
                    address = System.Net.IPAddress.Any;
                    Logger.Log(LogLevel.Error, $"链接 [IPAddress.TryParse] 发生错误, 转为 [IPAddress.Any] ip:{ip} address:{address}");
                }

                _endPoint = new IPEndPoint(address, port);
                _client.Blocking = false;
                try
                {
                    _client.Connect(ip, port);
                }
                catch (System.Exception ex)
                {
                    _client = null;
                    Logger.Log(LogLevel.Exception, $"链接 [Connect] 发生异常 错误:{ex.Message}");
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Log(LogLevel.Exception, $"链接 发生异常 错误:{ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 客户端断开链接
        /// </summary>
        /// <param name="error">错误信息</param>
        public void Disconnect()
        {
            foreach (var unit in _finalSendQueue)
            {
                BufferPool.ReleaseBuff(unit.data);
            }
            _finalSendQueue.Clear();
            
            if (_client != null)
            {
                try
                {
                    if (_client.Connected)
                    {
                        _client.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Log(LogLevel.Exception, $"客户端 [Shutdown] 发生异常 错误:{ex.Message}");
                }
                try
                {
                    if (_client != null)
                    {
                        _client.Close();
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Log(LogLevel.Exception, $"客户端 [Close] 发生异常 错误:{ex.Message}");
                }
                _client = null;
            }

            lock (_sendLock)
            {
                while (_sendQueue.Count > 0)
                {
                    var packet = _sendQueue.Dequeue();
                    if (packet != null)
                    {
                        BufferPool.ReleaseBuff(packet.Data);
                        packet.Data = null;
                    }
                }
                _sendQueue.Clear();
            }

            ResetData();
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        private void ResetData()
        {
            while (_recvQueue.Count > 0)
            {
                var packet = _recvQueue.Pop();
                if (packet.Data != null)
                {
                    BufferPool.ReleaseBuff(packet.Data);
                    packet.Data = null;
                }
            }
            _recvQueue.Switch();
            while (_recvQueue.Count > 0)
            {
                var packet = _recvQueue.Pop();
                if (packet.Data != null)
                {
                    BufferPool.ReleaseBuff(packet.Data);
                    packet.Data = null;
                }
            }
            _recvQueue.Clear();
        }

    }
    
}