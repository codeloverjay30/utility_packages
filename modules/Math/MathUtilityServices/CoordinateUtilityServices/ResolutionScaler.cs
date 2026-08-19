namespace CoordinateUtilityServices
{
    public class ResolutionScaler : IResolutionScaler
    {
        private readonly double _baseW;
        private readonly double _baseH;

        public double ScaleX { get; }
        public double ScaleY { get; }

        // 畫布位移 (處理瀏海、導覽列)
        public Padding Offset { get; }

        public ResolutionScaler(
            double baseWidth ,
            double baseHeight ,
            double currentWidth ,
            double currentHeight ,
            Padding? offset = null)
        {
            _baseW = baseWidth;
            _baseH = baseHeight;

            Offset = offset ?? new Padding(0);

            // 計算扣除偏移後的實際可用畫布大小
            double usableWidth = currentWidth - Offset.Horizontal;
            double usableHeight = currentHeight - Offset.Vertical;

            ScaleX = usableWidth / _baseW;
            ScaleY = usableHeight / _baseH;
        }

        /// <summary>
        /// 正向轉換：基準座標 -> 實際螢幕點擊位置
        /// </summary>
        public Point Transform(Point basePoint)
        {
            return new Point
            {
                X = (basePoint.X * ScaleX) + Offset.Left ,
                Y = (basePoint.Y * ScaleY) + Offset.Top
            };
        }

        /// <summary>
        /// 逆向轉換：實際螢幕座標 -> 基準開發座標
        /// 用於：將手機上偵測到的特徵點位置，存回您的開發腳本中。
        /// </summary>
        public Point InverseTransform(Point actualPoint)
        {
            return new Point
            {
                // 先扣除位移，再除以縮放比例
                X = (actualPoint.X - Offset.Left) / ScaleX ,
                Y = (actualPoint.Y - Offset.Top) / ScaleY
            };
        }
    }
}
