using System.ComponentModel.DataAnnotations;

namespace ExceptionsUtilityServices.Tests;

public class TestPoco
{
    [Required(AllowEmptyStrings = true, ErrorMessage = "Username is required.")]
    public string Name { get; set; } = string.Empty;
}
