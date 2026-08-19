using System;
using System.Buffers;
using Xunit;
using FluentAssertions;
using ExceptionsUtilityServices;

namespace ExceptionsUtilityServices.Tests;

/// <summary>
/// Extended defense test suite targeting edge cases and infrastructure boundary failures.
/// </summary>
public class GuardClauseExtendedTests
{
    #region --- 多節點夾雜有效字元測試 ---

    [Fact]
    public void ThrowIfNullOrWhitespace_WithMixedSegments_ShouldNotThrowException()
    {
        // Arrange: 第一節點全空白，第二節點包含有效字元，第三節點全空白
        var firstSegment = new BufferSegment<char>("   ".AsMemory());
        var secondSegment = firstSegment.Append("  Architect  ".AsMemory());
        var thirdSegment = secondSegment.Append(" \r\n ".AsMemory());
        var mixedSequence = new ReadOnlySequence<char>(firstSegment, 0, thirdSegment, thirdSegment.Memory.Length);

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(mixedSequence, "mixedParam");

        // Assert: 嚴格遵循鐵律，不拋出任何異常
        act.Should().NotThrow();
    }

    #endregion

    #region --- 非支援型別守衛拋出 NotSupportedException 測試 ---

    [Fact]
    public void ThrowIfNullOrEmpty_WithUnsupportedGenericType_ShouldThrowNotSupportedException()
    {
        // Arrange: 傳入自訂的非記憶體結構（例如 DateTime）
        DateTime unsupportedTarget = DateTime.UtcNow;

        // Act: 使用 Action 封裝以利進行真實 Message 檢驗
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty<DateTime, char>(unsupportedTarget, "invalidTypeParam");

        // Assert: 驗證是否精準阻絕非預期型別
        act.Should().Throw<NotSupportedException>()
           .WithMessage("*is not a recognized high-performance memory structure*");
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