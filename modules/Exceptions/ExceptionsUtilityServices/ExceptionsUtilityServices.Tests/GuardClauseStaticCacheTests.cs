using System;
using Xunit;
using FluentAssertions;
using ExceptionsUtilityServices;

namespace ExceptionsUtilityServices.Tests;

/// <summary>
/// Advanced pipeline compilation and performance cache boundary validation.
/// </summary>
public class GuardClauseStaticCacheTests
{
    #region --- 雙泛型管道原生 String 特化測試 ---

    [Fact]
    public void ThrowIfNullOrEmpty_WithEmptyNativeString_ShouldThrowArgumentEmptyOrWhitespaceException()
    {
        // Arrange
        string emptyString = string.Empty;

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty<string, char>(emptyString, "nativeStringParam");

        // Assert: 驗證 Unsafe.As 是否成功經由快取路由至字串特化處理
        act.Should().Throw<ArgumentEmptyOrWhitespaceException>()
           .Where(e => e.ParamName == "nativeStringParam")
           .WithMessage("*The native string input cannot be empty.*");
    }

    [Fact]
    public void ThrowIfNullOrEmpty_WithValidNativeString_ShouldPassSuccessfully()
    {
        // Arrange
        string validString = "Clean Architecture";

        // Act
        Action act = () => ArgumentEmptyOrWhitespaceException.ThrowIfNullOrEmpty<string, char>(validString, "validStringParam");

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}