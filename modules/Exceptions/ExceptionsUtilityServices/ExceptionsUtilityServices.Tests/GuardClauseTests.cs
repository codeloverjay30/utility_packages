using System;
using System.Buffers;
using Xunit;
using FluentAssertions;
using ExceptionsUtilityServices;
using System.IO;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Exceptions;

namespace ExceptionsUtilityServices.Tests;

/// <summary>
/// Contains defensive infrastructure verification for memory-span guard clauses.
/// </summary>
public class GuardClauseTests
{
    #region --- ThrowIfNullOrEmpty 測試 ---

    [Fact]
    public void ThrowIfNullOrEmpty_WithEmptySpan_ShouldThrowException()
    {
        // Arrange & Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty((ReadOnlySpan<int>)Span<int>.Empty, "mySpan");

        // Assert
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(p => p.ParamName == "mySpan")
           .WithMessage("*crystalline memory span*");
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WithEmptyReadOnlySequence_ShouldThrowException()
    {
        // Arrange
        var emptySequence = ReadOnlySequence<byte>.Empty;

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty(emptySequence, "seqParam");

        // Assert
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(p => p.ParamName == "seqParam");
    }

    #endregion

    #region --- ThrowIfNullOrWhitespace 測試 ---

    [Fact]
    public void ThrowIfNullOrWhitespace_WithWhitespaceReadOnlyMemory_ShouldThrowException()
    {
        // Arrange
        ReadOnlyMemory<char> memory = "    ".AsMemory();

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(memory, "memParam");

        // Assert
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(p => p.ParamName == "memParam")
           .WithMessage("*cannot consist only of white-space characters*");
    }

    [Fact]
    public void ThrowIfNullOrWhitespace_WithAllWhitespaceMultiSegmentSequence_ShouldThrowArgumentEmptyOrWhitespaceException()
    {
        // Arrange
        var firstSegment = new BufferSegment<char>("   ".AsMemory());
        var secondSegment = firstSegment.Append("      ".AsMemory());
        var thirdSegment = secondSegment.Append("\t\r\n".AsMemory());
        var multiSegmentWhitespaceSequence = new ReadOnlySequence<char>(firstSegment, 0, thirdSegment, thirdSegment.Memory.Length);

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(multiSegmentWhitespaceSequence, "whitespaceSeq");

        // Assert
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(p => p.ParamName == "whitespaceSeq")
           .WithMessage("*Sequence cannot consist only of white-space characters*");
    }

    [Fact]
    public void ThrowIfNullOrWhitespace_WithValidMultiSegmentSequence_ShouldNotThrow()
    {
        // Arrange
        var firstSegment = new BufferSegment<char>("   ".AsMemory());
        var secondSegment = firstSegment.Append("  精實架構師  ".AsMemory());
        var multiSegmentSequence = new ReadOnlySequence<char>(firstSegment, 0, secondSegment, secondSegment.Memory.Length);

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrWhitespace(multiSegmentSequence, "validSeq");

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region --- MismatchedDataStructureException 測試 ---

    [Fact]
    public void Create_WithMismatchedRuntimeTypes_ShouldFormatMessageCorrectlyWithoutCrashing()
    {
        // Arrange
        var expectedType = typeof(ReadOnlySpan<byte>);
        var actualType = typeof(ArraySegment<char>);
        string formatString = "Expected structure: {0}, but received: {1}.";

        // Act
        // 修復 CS0023：直接調用工廠方法，不使用 Action 封裝，回傳強型別物件進行主體斷言
        var exception = MismatchedDataStructureException<object, object>.Create(expectedType, actualType, formatString);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain(nameof(ReadOnlySpan<byte>))
                         .And.Contain(nameof(ArraySegment<char>));
    }

    #endregion

    #region --- 多執行緒與平行存取緩衝快取測試 ---

    [Fact]
    public void IsEmpty_UnderHighConcurrency_ShouldBeThreadSafeAndPerformant()
    {
        // Arrange
        var sequence = ReadOnlySequence<long>.Empty;
        var actions = new List<Action>();

        for (int i = 0; i < 100; i++)
        {
            actions.Add(() =>
            {
                Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty(sequence, "concurrentParam");
                act.Should().Throw<ArgumentEmptyOrWhitespaceException>();
            });
        }

        // Act & Assert
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };
        Action parallelAct = () => Parallel.Invoke(parallelOptions, actions.ToArray());

        parallelAct.Should().NotThrow();
    }

    #endregion

    #region --- 惡意惡化測試 ---

    [Fact]
    public void ThrowIfNullOrEmpty_WithDisposedUnmanagedMemoryStream_ShouldThrowObjectDisposedExceptionHandled()
    {
        // Arrange
        UnmanagedMemoryStream stream;
        unsafe
        {
            // 配置一個具有實質容量 (10位元組) 的不安全記憶體區塊
            byte* buffer = stackalloc byte[10];
            stream = new UnmanagedMemoryStream(buffer, 10, 10, FileAccess.ReadWrite);
        }

        stream.Dispose(); // 徹底摧毀其內部狀態

        // Act
        // 嚴格遵循鐵律：使用 Action 攔截並驗證
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty(stream, "disposedStream");

        // Assert
        act.Should().Throw<ObjectDisposedException>();
    }


    #endregion

    #region --- 協助建立 Multi-Segment 測試的內部輔助類別 ---

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