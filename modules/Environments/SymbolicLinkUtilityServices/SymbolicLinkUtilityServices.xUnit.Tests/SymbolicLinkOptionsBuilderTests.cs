using Xunit;
using FluentAssertions; // 引入 FluentAssertions 命名空間
using SymbolicLinkUtilityServices;

namespace SymbolicLinkUtilityServices.xUnit.Tests;
public class SymbolicLinkOptionsBuilderTests
{
    [Fact]
    public void Build_WithRequiredParameters_ShouldReturnCorrectDefaultOptions()
    {
        // 1. Arrange
        string expectedLink = @"C:\data\link";
        string expectedTarget = @"D:\data\target";

        // 2. Act
        SymbolicLinkOptions options = new SymbolicLinkOptionsBuilder(expectedLink, expectedTarget)
            .Build();

        // 3. Assert (使用 FluentAssertions 語法)
        options.Should().NotBeNull();
        options.LinkPath.Should().Be(expectedLink);
        options.TargetPath.Should().Be(expectedTarget);

        // 驗證預設值
        options.EnsureTargetExists.Should().BeFalse();
        options.EnsureSourceIsLink.Should().BeTrue();
        options.LockObject.Should().BeNull();
    }

    [Fact]
    public void CheckTargetExists_WhenCalled_ShouldSetEnsureTargetExistsToTrue()
    {
        // Arrange
        var builder = new SymbolicLinkOptionsBuilder(@"C:\link", @"D:\target");

        // Act
        var options = builder.CheckTargetExists().Build();

        // Assert
        options.EnsureTargetExists.Should().BeTrue();
    }

    [Fact]
    public void WithLock_WhenObjectProvided_ShouldAssignLockObject()
    {
        // Arrange
        var builder = new SymbolicLinkOptionsBuilder(@"C:\link", @"D:\target");
        var myLock = new object();

        // Act
        var options = builder.WithLock(myLock).Build();

        // Assert
        // BeSameAs 會驗證兩個物件是否指向同一個記憶體位址（Reference Equality）
        options.LockObject.Should().BeSameAs(myLock);
    }
}