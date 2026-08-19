using CoordinateUtilityServices;
using System;
using Xunit;

namespace CoordinateUtilityServices.Tests
{
    public class ResolutionScalerTests
    {
        // 定義開發時的基準解析度 (例如 1080x1920)
        private const double BaseW = 1080;
        private const double BaseH = 1920;

        [Fact]
        public void Transform_StandardFullRes_ReturnsCorrectScaling()
        {
            // 模擬：基準 1080x1920 -> 實際 1440x2560 (剛好 1.333 倍)
            var scaler = new ResolutionScaler(BaseW , BaseH , 1440 , 2560);
            var basePoint = new Point(540 , 960); // 基準中心點

            var actualPoint = scaler.Transform(basePoint);

            // 預期結果：540 * (1440/1080) = 720, 960 * (2560/1920) = 1280
            Assert.Equal(720 , actualPoint.X , 2);
            Assert.Equal(1280 , actualPoint.Y , 2);
        }

        [Fact]
        public void Transform_WithOffset_HandlesNotchCorrectly()
        {
            // 模擬：手機寬度 1080，但頂部有 100px 的狀態列/瀏海偏移
            // 實際螢幕 1080x2400，但遊戲畫布從 Y=100 開始，且高度只剩 2300
            var offset = new Padding { Top = 100 , Left = 0 , Right = 0 , Bottom = 0 };
            var scaler = new ResolutionScaler(BaseW , BaseH , 1080 , 2400 , offset);

            // 點擊基準畫面最左上角 (0, 0)
            var basePoint = new Point(0 , 0);

            var actualPoint = scaler.Transform(basePoint);

            // 預期結果：X 應為 0，但 Y 應被推到 100 (Offset.Top)
            Assert.Equal(0 , actualPoint.X , 2);
            Assert.Equal(100 , actualPoint.Y , 2);
        }

        [Theory]
        [InlineData(100 , 200)]
        [InlineData(540 , 960)]
        [InlineData(1080 , 1920)]
        public void InverseTransform_ShouldBeReversible(double x , double y)
        {
            // 測試 Transform 後再 InverseTransform 是否回到原點
            // 模擬一個複雜的環境：2K 螢幕且有左右各 50px 的安全邊距
            var offset = new Padding(50 , 0 , 50 , 0);
            var scaler = new ResolutionScaler(BaseW , BaseH , 1440 , 2960 , offset);
            var originalPoint = new Point(x , y);

            // 執行正向再逆向
            var transformed = scaler.Transform(originalPoint);
            var reversed = scaler.InverseTransform(transformed);

            // 驗證精度 (允許 0.0001 的誤差)
            Assert.Equal(originalPoint.X , reversed.X , 4);
            Assert.Equal(originalPoint.Y , reversed.Y , 4);
        }

        [Fact]
        public void ToRoundedInt_ShouldUseAwayFromZero()
        {
            // 驗證我們的 Point 轉換整數邏輯是否符合點擊預期
            var p1 = new Point(10.5 , 20.5);
            var (x1 , y1) = p1.ToRoundedInt();

            // AwayFromZero: 10.5 -> 11
            Assert.Equal(11 , x1);
            Assert.Equal(21 , y1);
        }
    }
}
