using System;
using System.Buffers;
using System.Exceptions;
using Xunit;
using FluentAssertions;

namespace ExceptionsUtilityServices.Tests;

/// <summary>
/// Infrastructure defense suite verifying advanced edge cases, formatting resiliency, and structural variance.
/// </summary>
public class GuardClauseDeepDefenseTests
{
    #region --- 惡意格式化字串的防禦性測試 ---

    [Fact]
    public void Create_WithCorruptedFormatPlaceholder_ShouldNotThrowFormatExceptionButFallbackSafely()
    {
        // Arrange
        var expectedType = typeof(ReadOnlyMemory<char>);
        var actualType = typeof(string);
        // 刻意傳入錯誤的預留位置 {2}，若無內部防禦將會噴出 FormatException
        string corruptedFormat = "Expected: {0}, Actual: {1}, But missing this: {2}";

        // Act
        // 使用 Action 驗證執行工廠方法時是否具備吞噬崩潰並降級的能力
        Action act = () => MismatchedDataStructureException<object, object>.Create(expectedType, actualType, corruptedFormat);

        // Assert: 系統必須展現高度韌性，不拋出 FormatException，而是成功建立 Exception 物件
        act.Should().NotThrow<FormatException>();
        
        var exception = MismatchedDataStructureException<object, object>.Create(expectedType, actualType, corruptedFormat);
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Mismatched structure")
                         .And.Contain(nameof(ReadOnlyMemory<char>))
                         .And.Contain(corruptedFormat); // 確保原始軌跡有被完整保留供工程師追蹤
    }

    #endregion

    #region --- 全形與特殊空白混合多節點序列測試 ---

    [Fact]
    public void ThrowIfNullOrWhitespace_WithSpecialUnicodeWhitespaces_ShouldThrowCorrectException()
    {
        // Arrange: 第一節點包含全形空白(\u3000)，第二節點包含 Ideographic Space 等
        var firstSegment = new BufferSegment<char>("\u3000\u3000".AsMemory());
        var secondSegment = firstSegment.Append("  \t  ".AsMemory());
        var specialSequence = new ReadOnlySequence<char>(firstSegment, 0, secondSegment, secondSegment.Memory.Length);

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(specialSequence, "unicodeWhitespaceParam");

        // Assert: 必須精準識別出這屬於全空白序列並拋出正確異常
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(p => p.ParamName == "unicodeWhitespaceParam")
           .WithMessage("*Sequence cannot consist only of white-space characters*");
    }

    #endregion

    #region --- 空字串與特殊字元邊界極限測試 ---

    [Fact]
    public void ThrowIfNullOrEmpty_WithZeroLengthStandardArray_ShouldBeInterruptedByGuardClause()
    {
        // Arrange
        int[] emptyManagedArray = Array.Empty<int>();

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty<int[], int>(emptyManagedArray, "emptyArrayParam");

        // Assert: 驗證 Unsafe.As 是否精確對接 Managed Standard Array 的空值檢驗
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .WithMessage("*managed standard array cannot be empty*");
    }

    #endregion

    #region --- 內部輔助測試結構 ---

    private class BufferSegment<T> : ReadOnlySequenceSegment<T>
    {
        public BufferSegment(ReadOnlyMemory<T> memory)
        {
            Memory = memory;
        }

        public BufferSegment<T> Append(ReadOnlyMemory<T> nextMemory)
        {
            var nextSegment = new BufferSegment<T>(nextMemory)
            {
                RunningIndex = RunningIndex + Memory.Length
            };
            Next = nextSegment;
            return nextSegment;
        }
    }

    #endregion
}