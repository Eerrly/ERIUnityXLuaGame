/********************************************************************
** Filename : GlobalDefine  
** Author : ake
** Date : 2018/1/30 20:55:44
** Summary : GlobalDefine 
***********************************************************************/

namespace Framework
{
	public static class FixedMathDefine
	{
		#region Fields
		/// <summary>
		/// 转换成表现用数据使用的系数
		/// </summary>
		public const float BaseInverseCalFactor = 0.0001f;
		public const int BaseCalFactor = 10000;
		public const long Pow2BaseCalFactor = BaseCalFactor* BaseCalFactor;

		/// <summary>
		/// 毫秒放大10倍 统一微秒 万分数
		/// </summary>
		public const int TimeScalerFactor = 10;

		public static readonly FixedNumber AngleMax = new FixedNumber(360* BaseCalFactor);
		public static readonly FixedNumber HalfAngleMax = AngleMax/2;
        public static readonly FixedNumber QuarterAngleMax = HalfAngleMax / 2;


		public const int BaseCalFactorPow2 = BaseCalFactor*BaseCalFactor;
		public const int HalfBaseCalFactor = BaseCalFactor/2;

		public const int QuaternionToEulerThreshold = 9999;

 
        public const int YawOffset = 50;
        public static readonly FixedNumber Pi = new FixedNumber(31416);
        public static readonly FixedNumber PiHalf = new FixedNumber(15708);
        public static readonly FixedNumber Deg2Rad = new FixedNumber(175);
        public static readonly FixedNumber Rad2Deg = new FixedNumber(572958);

		/// <summary>
		/// 输入角度划分
		/// </summary>
        public static readonly FixedNumber DivAngle = new FixedNumber(150000);
		/// <summary>
		/// 输入角度划分 的一半
		/// </summary>
        public static readonly FixedNumber HalfDivAngle = new FixedNumber(75000);

		#endregion

		#region public

		#endregion

		#region private

		#endregion
	}


}
