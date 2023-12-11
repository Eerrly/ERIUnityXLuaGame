namespace KCPNet
{
    public class NetConstant
    {
        public static readonly string IP = "127.0.0.1";

        public static readonly int Port = 10086;

        public const int pvpFrameCmd = 4;
        public const int pvpReadyAct = 3;
        public const int pvpFrameAct = 2;
        public const int pvpPingAct = 1;
        
        /// <summary>
        /// 服务器返回的帧数据消息
        /// </summary>
        public const int pvpFrameType = (pvpFrameCmd << 8) + pvpFrameAct;
        /// <summary>
        /// 服务器返回的Ping消息
        /// </summary>
        public const int pvpPingType = (pvpFrameCmd << 8) + pvpPingAct;
    }
}
