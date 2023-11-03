using System;

namespace Framework
{
	public struct FixedRect
	{
		private FixedNumber m_XMin;

        private FixedNumber m_YMin;

        private FixedNumber m_Width;

        private FixedNumber m_Height;

        public FixedNumber x
		{
			get
			{
				return this.m_XMin;
			}
			set
			{
				this.m_XMin = value;
			}
		}

        public FixedNumber y
		{
			get
			{
				return this.m_YMin;
			}
			set
			{
				this.m_YMin = value;
			}
		}

		public FixedVector2 position
		{
			get
			{
				return new FixedVector2(this.m_XMin, this.m_YMin);
			}
			set
			{
				this.m_XMin = value.x;
				this.m_YMin = value.y;
			}
		}

		public FixedVector2 center
		{
			get
			{
				return new FixedVector2(this.x + (this.m_Width / 2 ), this.y + (this.m_Height / 2));
			}
			set
			{
				this.m_XMin = value.x - (this.m_Width / 2);
				this.m_YMin = value.y - (this.m_Height / 2);
			}
		}

		public FixedVector2 min
		{
			get
			{
				return new FixedVector2(this.xMin, this.yMin);
			}
			set
			{
				this.xMin = value.x;
				this.yMin = value.y;
			}
		}

		public FixedVector2 max
		{
			get
			{
				return new FixedVector2(this.xMax, this.yMax);
			}
			set
			{
				this.xMax = value.x;
				this.yMax = value.y;
			}
		}

        public FixedNumber width
		{
			get
			{
				return this.m_Width;
			}
			set
			{
				this.m_Width = value;
			}
		}

        public FixedNumber height
		{
			get
			{
				return this.m_Height;
			}
			set
			{
				this.m_Height = value;
			}
		}

		public FixedVector2 size
		{
			get
			{
				return new FixedVector2(this.m_Width, this.m_Height);
			}
			set
			{
				this.m_Width = value.x;
				this.m_Height = value.y;
			}
		}

        public FixedNumber xMin
		{
			get
			{
				return this.m_XMin;
			}
			set
			{
                FixedNumber xMax = this.xMax;
				this.m_XMin = value;
				this.m_Width = xMax - this.m_XMin;
			}
		}

        public FixedNumber yMin
		{
			get
			{
				return this.m_YMin;
			}
			set
			{
                FixedNumber yMax = this.yMax;
				this.m_YMin = value;
				this.m_Height = yMax - this.m_YMin;
			}
		}

        public FixedNumber xMax
		{
			get
			{
				return this.m_Width + this.m_XMin;
			}
			set
			{
				this.m_Width = value - this.m_XMin;
			}
		}

        public FixedNumber yMax
		{
			get
			{
				return this.m_Height + this.m_YMin;
			}
			set
			{
				this.m_Height = value - this.m_YMin;
			}
		}

        public FixedRect(FixedNumber left, FixedNumber top, FixedNumber width, FixedNumber height)
		{
			this.m_XMin = left;
			this.m_YMin = top;
			this.m_Width = width;
			this.m_Height = height;
		}

		public FixedRect(FixedRect source)
		{
			this.m_XMin = source.m_XMin;
			this.m_YMin = source.m_YMin;
			this.m_Width = source.m_Width;
			this.m_Height = source.m_Height;
		}

        public static FixedRect MinMaxRect(FixedNumber left, FixedNumber top, FixedNumber right, FixedNumber bottom)
		{
			return new FixedRect(left, top, right - left, bottom - top);
		}

        public void Set(FixedNumber left, FixedNumber top, FixedNumber width, FixedNumber height)
		{
			this.m_XMin = left;
			this.m_YMin = top;
			this.m_Width = width;
			this.m_Height = height;
		}

		public override string ToString()
		{
			object[] array = new object[]
			{
			this.x,
			this.y,
			this.width,
			this.height
			};
			return string.Format("(x:{0:F2}, y:{1:F2}, width:{2:F2}, height:{3:F2})", array);
		}

        //public string ToString(string format)
        //{
        //    object[] array = new object[]
        //    {
        //    this.x.ToString(format),
        //    this.y.ToString(format),
        //    this.width.ToString(format),
        //    this.height.ToString(format)
        //    };
        //    return string.Format("(x:{0}, y:{1}, width:{2}, height:{3})", array);
        //}

		public bool Contains(FixedVector2 point)
		{
			return point.x >= this.xMin && point.x < this.xMax && point.y >= this.yMin && point.y < this.yMax;
		}

		public bool Contains(FixedVector3 point)
		{
			return point.x >= this.xMin && point.x < this.xMax && point.y >= this.yMin && point.y < this.yMax;
		}

        public bool Contains(FixedNumber x, FixedNumber y)
        {
            return x >= this.xMin && x < this.xMax && y >= this.yMin && y < this.yMax;
        }

		public bool Contains(FixedVector3 point, bool allowInverse)
		{
			if (!allowInverse)
			{
				return this.Contains(point);
			}
			bool flag = false;
			if ((this.width < 0 && point.x <= this.xMin && point.x > this.xMax) || (this.width >= 0 && point.x >= this.xMin && point.x < this.xMax))
			{
				flag = true;
			}
			return flag && ((this.height < 0 && point.y <= this.yMin && point.y > this.yMax) || (this.height >= 0 && point.y >= this.yMin && point.y < this.yMax));
		}

		private static FixedRect OrderMinMax(FixedRect rect)
		{
			if (rect.xMin > rect.xMax)
			{
                FixedNumber xMin = rect.xMin;
				rect.xMin = rect.xMax;
				rect.xMax = xMin;
			}
			if (rect.yMin > rect.yMax)
			{
                FixedNumber yMin = rect.yMin;
				rect.yMin = rect.yMax;
				rect.yMax = yMin;
			}
			return rect;
		}

		public bool Overlaps(FixedRect other)
		{
			return other.xMax > this.xMin && other.xMin < this.xMax && other.yMax > this.yMin && other.yMin < this.yMax;
		}

		public bool Overlaps(FixedRect other, bool allowInverse)
		{
			FixedRect rect = this;
			if (allowInverse)
			{
				rect = FixedRect.OrderMinMax(rect);
				other = FixedRect.OrderMinMax(other);
			}
			return rect.Overlaps(other);
		}

		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.width.GetHashCode() << 2 ^ this.y.GetHashCode() >> 2 ^ this.height.GetHashCode() >> 1;
		}

		public override bool Equals(object other)
		{
			if (!(other is FixedRect))
			{
				return false;
			}
			FixedRect vRect = (FixedRect)other;
			return this.x.Equals(vRect.x) && this.y.Equals(vRect.y) && this.width.Equals(vRect.width) && this.height.Equals(vRect.height);
		}

		public static bool operator !=(FixedRect lhs, FixedRect rhs)
		{
			return lhs.x != rhs.x || lhs.y != rhs.y || lhs.width != rhs.width || lhs.height != rhs.height;
		}

		public static bool operator ==(FixedRect lhs, FixedRect rhs)
		{
			return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
		}
	}

}

