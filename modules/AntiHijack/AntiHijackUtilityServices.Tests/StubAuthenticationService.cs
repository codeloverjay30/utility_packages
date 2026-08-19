using AuthenticationUtilityServices;

namespace AntiHijackUtilityServices.Tests;

/// <summary>
/// A lightweight, performance-optimized manual stub for <see cref="IAuthenticationService"/> 
/// that safely bypasses CLR dynamic proxy limitations regarding ref structs.
/// </summary>
public class StubAuthenticationService : IAuthenticationService
{
    /// <summary>
    /// Gets or sets a value indicating whether the next signature verification should succeed.
    /// </summary>
    public bool VerificationResult { get; set; } = true;

    /// <summary>
    /// Safely processes the spans without triggering CLR InvalidProgramException.
    /// </summary>
    public bool VerifySignature(ReadOnlySpan<char> rawPayload, ReadOnlySpan<char> expectedSignature)
    {
        // 100% 棧安全，不進行任何反射或動態 IL 生成
        return VerificationResult;
    }
}
