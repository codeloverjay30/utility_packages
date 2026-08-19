using System.Reflection;
using System.Reflection.Emit;

namespace AssemblyUtilityServices.Tests;

internal static class DynamicAssemblyFactory
{
    internal static Assembly Create(
        string? informationalVersion = null,
        Version? assemblyVersion = null)
    {
        var assemblyName = new AssemblyName(
            $"AssemblyUtilityServices.Tests.Dynamic.{Guid.NewGuid():N}")
        {
            Version = assemblyVersion ?? new Version(1, 0, 0, 0)
        };

        AssemblyBuilder assemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(
                assemblyName,
                AssemblyBuilderAccess.Run);

        if (informationalVersion is not null)
        {
            ConstructorInfo constructor = typeof(AssemblyInformationalVersionAttribute)
                .GetConstructor([typeof(string)])
                ?? throw new InvalidOperationException(
                    "AssemblyInformationalVersionAttribute constructor was not found.");

            var attribute = new CustomAttributeBuilder(
                constructor,
                [informationalVersion]);

            assemblyBuilder.SetCustomAttribute(attribute);
        }

        return assemblyBuilder;
    }
}
