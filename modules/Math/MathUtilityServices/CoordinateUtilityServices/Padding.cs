using System;
using System.Diagnostics.CodeAnalysis;

namespace CoordinateUtilityServices
{
    /// <summary>
    /// 定義四個方向的邊距 (Left, Top, Right, Bottom)。
    /// 適用於 UI 區域裁切或點擊範圍內縮計算。
    /// </summary>
    public readonly struct Padding : IEquatable<Padding>
    {
        public required int Left { get; init; }
        public required int Top { get; init; }
        public required int Right { get; init; }
        public required int Bottom { get; init; }

        /// <summary>
        /// 快速建立四邊相等的 Padding。
        /// </summary>
        [SetsRequiredMembers]
        public Padding(int all) : this(all , all , all , all) { }

        /// <summary>
        /// 指定四個方向的 Padding。
        /// </summary>
        [SetsRequiredMembers]
        public Padding(int left , int top , int right , int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        #region 實用工具方法

        /// <summary>
        /// 將 Padding 套用到指定的 Point，產生內縮後的新座標。
        /// </summary>
        public Point ApplyTo(Point point)
            => new(point.X + Left - Right , point.Y + Top - Bottom);

        /// <summary>
        /// 取得水平方向的總和 (Left + Right)。
        /// </summary>
        public int Horizontal => Left + Right;

        /// <summary>
        /// 取得垂直方向的總和 (Top + Bottom)。
        /// </summary>
        public int Vertical => Top + Bottom;

        /// <summary>
        /// 根據比例縮放邊距（常用於解析度轉換時同步調整 UI 邊距）。
        /// </summary>
        public Padding Scale(double factor) => new(
            (int)Math.Round(Left * factor) ,
            (int)Math.Round(Top * factor) ,
            (int)Math.Round(Right * factor) ,
            (int)Math.Round(Bottom * factor)
        );

        #endregion

        #region 標準介面實作

        public override string ToString() => $"L:{Left}, T:{Top}, R:{Right}, B:{Bottom}";

        public bool Equals(Padding other) =>
            Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

        public override bool Equals(object? obj) => obj is Padding other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Left , Top , Right , Bottom);

        public static bool operator ==(Padding left , Padding right) => left.Equals(right);

        public static bool operator !=(Padding left , Padding right) => !left.Equals(right);

        #endregion
    }
}
