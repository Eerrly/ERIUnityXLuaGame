public class FrameBuffer
{

    /// <summary>
    /// 输入
    /// </summary>
    public struct Input
    {
        /// <summary>
        /// 8 - bit
        /// | 0 0 0 0 | 0 0 0 | 0 |
        /// |   yaw   |  key  |pos|
        /// yaw  :   4 bit   :   (read & 0xF0) >> 4
        /// key  :   3 bit   :   (read & 0x0E) >> 1
        /// pos  :   1 bit   :   (read & 0x01)
        /// </summary>
        private byte raw;

        /// <summary>
        /// 8  1  2
        ///  \ | /
        /// 7——0——3
        ///  / | \
        /// 6  5  4
        /// </summary>
        public byte yaw
        {
            get => (byte)((0xF0 & raw) >> 4);
            set => raw = (byte)((raw & ~0xF0) | ((0xF & value) << 4));
        }

        /// <summary>
        /// [j] [k] [l]
        /// </summary>
        public byte key
        {
            get => (byte)((0x0E & raw) >> 1);
            set => raw = (byte)((raw & ~0x0E) | ((0x7 & value) << 1));
        }

        /// <summary>
        /// 玩家ID
        /// </summary>
        public byte pos
        {
            get => (byte)(0x01 & raw);
            set => raw = (byte)((raw & ~0x01) | value);
        }

        public Input(byte value)
        {
            raw = value;
        }

        public Input(byte pos, byte value)
        {
            raw = (byte)(~0x01 & (value << 1) | pos);
        }

        public override string ToString()
        {
            return $"pos:{pos}, raw:{raw}, yaw:{yaw}, btn:{key}";
        }

        public byte ToByte()
        {
            return raw;
        }

        public bool Compare(Input other)
        {
            return raw == other.raw;
        }

    }

    /// <summary>
    /// 帧数据
    /// </summary>
    public struct Frame
    {
        public int frame;
        public int playerCount;
        public Input i0;
        public Input i1;

        public static readonly Input defInput = new Input();
        public static readonly Frame defFrame = new Frame()
        {
            frame = 0,
            playerCount = 0,
            i0 = new Input(0, 0),
            i1 = new Input(1, 0),
        };

        public int Length
        {
            get
            {
                return playerCount;
            }
        }

        public Input this[int index]
        {
            get
            {
                if (index < playerCount)
                {
                    switch (index)
                    {
                        case 0: return i0;
                        case 1: return i1;
                    }
                }
                return defInput;
            }
            set
            {
                if (index < playerCount)
                {
                    switch (index)
                    {
                        case 0: i0 = value; break;
                        case 1: i1 = value; break;
                    }
                }
            }
        }

        public void SetInputByPos(int pos, Input result)
        {
            if (i0.pos == pos)
            {
                i0 = result;
            }
            else if(i1.pos == pos)
            {
                i1 = result;
            }
            else
            {
                Logger.Log(LogLevel.Warning, $"FrameBuffer.SetInputByPos pos not found! {pos},{playerCount},{frame}");
            }
        }

        public bool GetInputByPos(int pos, ref Input result)
        {
            if (i0.pos == pos)
            {
                result = i0;
                return true;
            }
            else if(i1.pos == pos)
            {
                result = i1;
                return true;
            }
            else
            {
                Logger.Log(LogLevel.Warning, $"FrameBuffer.GetInputByPos pos not found! {pos},{playerCount},{frame}");
                return false;
            }
        }

    }


    private int playerCount;
    private int capacity;
    private int inputSize;
    private int frameSize;
    private byte[] buffer;

    private Frame _lastGetFrame = Frame.defFrame;
    private int _lastSetFrameIndex = 0;

    public FrameBuffer(int playerCount, int capacity = 1000)
    {
        var size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Input));
        Logger.Log(LogLevel.Info, $"FrameBuffer.Create({playerCount}, {capacity}) sizeof(Input) = {size}");
        this.playerCount = playerCount;
        this.capacity = capacity;
        this.inputSize = size;

        this.frameSize = inputSize * this.playerCount + 4/*(frame)*/;
        this.buffer = new byte[frameSize * this.capacity];
        for (int i = 0; i < capacity; i++)
        {
            unsafe
            {
                fixed(byte* dest = &buffer[i * frameSize])
                {
                    // frame
                    *(int*)dest = -1;
                }
            }
        }
    }

    public void Reset()
    {
        _lastGetFrame = Frame.defFrame;
        _lastSetFrameIndex = 0;
    }

    public bool HasFrame(int frame)
    {
        unsafe
        {
            fixed (byte* dest = &buffer[(frame % capacity) * frameSize])
            {
                var currentFrame = *(int*)dest;
                if(frame != currentFrame)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool TryGetFrame(int frame, ref Frame result, bool reset = true)
    {
        unsafe
        {
            result.playerCount = playerCount;
            fixed(byte* dest = &buffer[(frame % capacity) * frameSize])
            {
                if(frame != 0 && _lastGetFrame.frame + 1 != frame)
                {
                    Logger.Log(LogLevel.Error, $"取帧数据必须逐帧 上一帧:{_lastGetFrame.frame} 要取的帧:{frame}");
                    return false;
                }
                var currentFrame = *(int*)dest;
                if(frame != currentFrame)
                {
                    Logger.Log(LogLevel.Error, $"找不到要取的帧:{frame}");
                    return false;
                }
                result.frame = currentFrame;
                result.playerCount = playerCount;
                
                result.i0.pos = 255;
                result.i1.pos = 255;
                if(playerCount > 0)
                {
                    result.i0 = *(Input*)(dest + 4/*(frame)*/ + 0 * inputSize);
                }
                if(playerCount > 1)
                {
                    result.i1 = *(Input*)(dest + 4/*(frame)*/ + 1 * inputSize);
                }
                if (reset)
                {
                    *(int*)dest = -1;
                }
                _lastGetFrame = result;
            }
        }
        return true;
    }

    public bool SyncFrame(int frame, ref Frame input, ref int diff)
    {
        unsafe
        {
            if(frame <= _lastSetFrameIndex)
            {
                return true;
            }

            fixed(byte* dest = &buffer[(frame % capacity) * frameSize])
            {
                diff = frame - _lastSetFrameIndex;
                if(diff > 1)
                {
                    Logger.Log(LogLevel.Error, $"存帧数据必须逐帧 上一帧:{_lastSetFrameIndex} 要存的帧:{frame}");
                    return false;
                }
                if(playerCount > 0)
                {
                    *(Input*)(dest + 4/*(frame)*/ + 0 * inputSize) = input.i0;
                }
                if(playerCount > 1)
                {
                    *(Input*)(dest + 4/*(frame)*/ + 1 * inputSize) = input.i0;
                }
                *(int*)dest = frame;
                _lastSetFrameIndex = frame;
            }
        }
        return true;
    }

}
