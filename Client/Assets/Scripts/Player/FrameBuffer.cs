public class FrameBuffer
{

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
        /// 7！！0！！3
        ///  / | \
        /// 6  5  4
        /// </summary>
        public byte yaw
        {
            get { return (byte)((0xF0 & raw) >> 4); }
            set { raw = (byte)((raw & ~0xF0) | ((0xF & value) << 4)); }
        }

        /// <summary>
        /// [j] [k] [l]
        /// </summary>
        public byte key
        {
            get { return (byte)((0x0E & raw) >> 1); }
            set { raw = (byte)((raw & ~0x0E) | ((0x7 & value) << 1)); }
        }

        public byte pos
        {
            get { return (byte)(0x01 & raw); }
            set { raw = (byte)((raw & ~0x01) | value); }
        }

        public Input(byte value)
        {
            raw = value;
        }

        public Input(byte pos, uint value)
        {
            raw = (byte)(~0xF & value | (byte)(0x01 & pos));
        }

        public override string ToString()
        {
            return string.Format("pos:{0}, raw:{1}, yaw:{2}, btn:{3}", pos, raw, yaw, key);
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


    public struct Frame
    {
        public int frame;
        public int playerCount;
        public Input i0;

        public static readonly Input defInput = new Input();
        public static readonly Frame defFrame = new Frame()
        {
            frame = 0,
            playerCount = 0,
            i0 = new Input(0),
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
        }

        public bool GetInputByPos(int pos, ref Input result)
        {
            if (i0.pos == pos)
            {
                result = i0;
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}
