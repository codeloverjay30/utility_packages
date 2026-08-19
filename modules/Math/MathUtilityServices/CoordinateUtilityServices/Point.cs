using System;
using System.Diagnostics.CodeAnalysis;

namespace CoordinateUtilityServices
{
    /// <summary>
    /// 高精度 2D 座標點，適用於自動化點擊與數學運算。
    /// </summary>
    public readonly struct Point : IEquatable<Point>
    {
        public required double X { get; init; }
        public required double Y { get; init; }

        [SetsRequiredMembers]
        public Point(double x , double y)
        {
            X = x;
            Y = y;
        }

        #region 高精度運算方法

        /// <summary>
        /// 針對縮放進行計算，使用 double 確保在不同解析度轉換間將誤差降至最低。
        /// </summary>
        /// <param name="scaleX">水平縮放比例</param>
        /// <param name="scaleY">垂直縮放比例</param>
        public Point Scale(double scaleX , double scaleY)
            => new(X * scaleX , Y * scaleY);

        /// <summary>
        /// 輸出適合 ADB 或 Appium 點擊指令的整數座標。
        /// 使用 MidpointRounding.AwayFromZero 確保 0.5 會進位，符合大多數 UI 座標直覺。
        /// </summary>
        public (int x , int y) ToRoundedInt()
            => ((int)Math.Round(X , MidpointRounding.AwayFromZero) ,
                (int)Math.Round(Y , MidpointRounding.AwayFromZero));

        #endregion

        #region 運算子多載 (Operator Overloading)

        public static Point operator +(Point a , Point b) => new(a.X + b.X , a.Y + b.Y);
        public static Point operator -(Point a , Point b) => new(a.X - b.X , a.Y - b.Y);
        public static Point operator *(Point a , double multiplier) => new(a.X * multiplier , a.Y * multiplier);

        #endregion

        #region 常用覆寫

        public override string ToString() => $"({X:F2}, {Y:F2})";

        public bool Equals(Point other) => X.Equals(other.X) && Y.Equals(other.Y);

        public override bool Equals(object? obj) => obj is Point other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X , Y);

        public static bool operator ==(Point left , Point right) => left.Equals(right);

        public static bool operator !=(Point left , Point right) => !left.Equals(right);

        #endregion
    }
}
