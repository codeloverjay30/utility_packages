using FluentAssertions;

namespace ExceptionsUtilityServices.Tests;

public class InvalidMembersExceptionTests
{
    [Fact]
    public void Validate_WithInvalidPoco_ShouldThrowInvalidMembersException()
    {
        // Arrange
        var invalidPoco = new TestPoco { Name = "" }; // 假設 Name 有 [Required]

        // Act
        Action act = () => MemberValidationEngine.Validate(invalidPoco);

       // Assert
        act.Should().Throw<InvalidMembersException>()
           .And.ValidationErrors.Should()
           .BeOfType<Dictionary<string, string>>() // 確保識別為字典類型，以便使用`ContainKey`方法
           .Which.Should().ContainKey(nameof(TestPoco.Name));
    }
}
